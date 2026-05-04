using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using SofiaTransport.Api.DependencyInjection;
using SofiaTransport.Api.Middleware;
using SofiaTransport.Infrastructure.DependencyInjection;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Realtime;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();
        for (var i = 0; i < 10; i++)
        {
            try
            {
                db.Database.Migrate();
                break;
            }
            catch (Exception ex) when (i < 9)
            {
                // Tables already exist from schema.sql initdb — migration is not required
                if (ex is PostgresException { SqlState: "42P07" } || (ex.InnerException is PostgresException { SqlState: "42P07" }))
                    break;

                var delay = TimeSpan.FromSeconds(Math.Pow(2, i));
                Log.Warning(ex, "Migration attempt {Attempt} failed, retrying in {Delay}s", i + 1, delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }
    }

    app.UseExceptionHandling();
    app.UseSecurityHeaders();
    app.UseRateLimiting();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseResponseCompression();
    app.UseCors();
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));
    app.MapHub<VehicleHub>(VehicleHub.HubPath).RequireAuthorization();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

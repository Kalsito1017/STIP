using Microsoft.EntityFrameworkCore;
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
        // Schema is managed by docker-entrypoint-initdb.d/schema.sql.
        // EnsureCreated uses CREATE TABLE IF NOT EXISTS — idempotent if tables pre-exist.
        db.Database.EnsureCreated();
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
    app.MapHub<VehicleHub>(VehicleHub.HubPath);

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

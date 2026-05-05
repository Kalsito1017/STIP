using System.Text.Json;
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
    app.MapGet("/health", async (TransportDbContext db) =>
    {
        var checks = new Dictionary<string, object>();
        var healthy = true;

        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1");
            checks["database"] = new { status = "healthy" };
        }
        catch (Exception ex)
        {
            healthy = false;
            checks["database"] = new { status = "unhealthy", error = ex.Message };
        }

        var result = new
        {
            status = healthy ? "healthy" : "degraded",
            timestamp = DateTime.UtcNow,
            checks
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return healthy
            ? Results.Ok(result)
            : Results.Content(json, "application/json", System.Text.Encoding.UTF8, statusCode: 503);
    });
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

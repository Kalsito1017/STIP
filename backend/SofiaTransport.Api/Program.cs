using Serilog;
using SofiaTransport.Api.DependencyInjection;
using SofiaTransport.Api.Middleware;
using SofiaTransport.Infrastructure.DependencyInjection;
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

    builder.Services.AddApiServices();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

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
    app.MapControllers();
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

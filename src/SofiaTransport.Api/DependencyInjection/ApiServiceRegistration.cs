using System.Reflection;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Api.DependencyInjection;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(
                typeof(SofiaTransport.Application.Routes.GetRoutesQuery))!));

        services.AddControllers();
        services.AddSignalR();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        services.AddResponseCompression(options =>
            options.EnableForHttps = true);

        return services;
    }
}

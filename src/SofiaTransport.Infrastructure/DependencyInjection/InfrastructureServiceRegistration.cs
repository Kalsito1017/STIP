using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Infrastructure.Cache;
using SofiaTransport.Infrastructure.GTFS;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using SofiaTransport.Infrastructure.Realtime;
using StackExchange.Redis;

namespace SofiaTransport.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TransportDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection") ?? configuration["DB_CONNECTION_STRING"],
                npgsql => npgsql.UseNetTopologySuite()));

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration["REDIS_CONNECTION"] ?? "localhost:6379"));

        services.AddSingleton<IVehicleCache, RedisVehicleCache>();
        services.AddSingleton<IVehicleBroadcaster, VehicleBroadcaster>();

        services.AddHttpClient<IGtfsFeedClient, GtfsFeedClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["GTFS_RT_FEED_URL"] ?? "https://localhost");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IStopRepository, StopRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IDelayLogRepository, DelayLogRepository>();
        services.AddScoped<IReliabilityScoreRepository, ReliabilityScoreRepository>();

        return services;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Infrastructure.Auth;
using SofiaTransport.Infrastructure.Cache;
using SofiaTransport.Infrastructure.GTFS;
using SofiaTransport.Infrastructure.ML;
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

        var redisConn = configuration["REDIS_CONNECTION"] ?? "localhost:6379";
        var redisConfig = ConfigurationOptions.Parse(redisConn);
        redisConfig.AbortOnConnectFail = false;
        redisConfig.ConnectRetry = 5;
        redisConfig.ConnectTimeout = 5000;
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConfig));

        services.AddSingleton<IVehicleCache, RedisVehicleCache>();
        services.AddSingleton<ITripUpdateCache, RedisTripUpdateCache>();
        services.AddSingleton<IAlertCache, RedisAlertCache>();
        services.AddSingleton<IVehicleBroadcaster, VehicleBroadcaster>();
        services.AddSingleton<IRealtimeBroadcaster, RealtimeBroadcaster>();

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        services.AddHttpClient<IGtfsFeedClient, GtfsFeedClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["GTFS_RT_FEED_URL"] ?? "https://localhost");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddPolicyHandler(retryPolicy);

        var tripUpdatesUrl = configuration["GTFS_RT_TRIP_UPDATES_URL"];
        if (!string.IsNullOrEmpty(tripUpdatesUrl))
        {
            services.AddHttpClient<ITripUpdateFeedClient, TripUpdateFeedClient>(client =>
            {
                client.BaseAddress = new Uri(tripUpdatesUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            }).AddPolicyHandler(retryPolicy);
        }

        var alertsUrl = configuration["GTFS_RT_ALERTS_URL"];
        if (!string.IsNullOrEmpty(alertsUrl))
        {
            services.AddHttpClient<IAlertFeedClient, AlertFeedClient>(client =>
            {
                client.BaseAddress = new Uri(alertsUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            }).AddPolicyHandler(retryPolicy);
        }

        services.AddHttpClient<IMLService, ML.MLService>(client =>
        {
            client.BaseAddress = new Uri(configuration["ML_SERVICE_URL"] ?? "http://localhost:8000");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IStopRepository, StopRepository>();
        services.AddScoped<IStopTimeRepository, StopTimeRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IDelayLogRepository, DelayLogRepository>();
        services.AddScoped<IReliabilityScoreRepository, ReliabilityScoreRepository>();
        services.AddScoped<IShapeRepository, ShapeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
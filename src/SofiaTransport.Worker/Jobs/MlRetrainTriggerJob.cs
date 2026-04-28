using Microsoft.EntityFrameworkCore;
using Quartz;
using SofiaTransport.Infrastructure.Persistence;

namespace SofiaTransport.Worker.Jobs;

[DisallowConcurrentExecution]
public class MlRetrainTriggerJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MlRetrainTriggerJob> _logger;
    private readonly IConfiguration _config;

    public MlRetrainTriggerJob(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<MlRetrainTriggerJob> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _config = config;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("ML retrain trigger job started");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var logs = await db.DelayLogs
            .Where(d => d.RecordedAt >= thirtyDaysAgo)
            .Select(d => new
            {
                d.RouteId,
                d.StopId,
                d.ScheduledArrival,
                d.DelaySeconds
            })
            .ToListAsync(context.CancellationToken);

        var mlUrl = _config["ML_SERVICE_URL"] ?? "http://ml:8000";
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{mlUrl}/internal/retrain",
            new { delay_logs = logs }, context.CancellationToken);

        if (response.IsSuccessStatusCode)
            _logger.LogInformation("ML retrain triggered successfully ({Count} records)", logs.Count);
        else
            _logger.LogWarning("ML retrain trigger failed: {StatusCode}", response.StatusCode);
    }
}

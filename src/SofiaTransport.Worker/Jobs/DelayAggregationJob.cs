using Microsoft.EntityFrameworkCore;
using Quartz;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Persistence;

namespace SofiaTransport.Worker.Jobs;

[DisallowConcurrentExecution]
public class DelayAggregationJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DelayAggregationJob> _logger;

    public DelayAggregationJob(IServiceScopeFactory scopeFactory, ILogger<DelayAggregationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Delay aggregation job started");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var logs = await db.DelayLogs
            .Where(d => d.RecordedAt >= yesterday && d.RecordedAt < yesterday.AddDays(1))
            .ToListAsync(context.CancellationToken);

        var scores = logs
            .GroupBy(l => l.RouteId)
            .Where(g => g.Key is not null)
            .Select(g =>
            {
                var entries = g.Where(e => e.DelaySeconds.HasValue).ToList();
                var onTime = entries.Count > 0
                    ? (double)entries.Count(e => Math.Abs(e.DelaySeconds!.Value) <= 60) / entries.Count
                    : 0;
                var avgDelay = entries.Average(e => e.DelaySeconds) ?? 0;
                var peakEntries = entries.Where(e => e.ScheduledArrival.Hour is >= 7 and <= 9 or >= 17 and <= 19).ToList();
                var peakOnTime = peakEntries.Count > 0
                    ? (double)peakEntries.Count(e => Math.Abs(e.DelaySeconds!.Value) <= 60) / peakEntries.Count
                    : onTime;

                return new ReliabilityScore
                {
                    RouteId = g.Key!,
                    ScoreDate = yesterday,
                    OnTimePct = onTime,
                    AvgDelaySeconds = avgDelay,
                    Score = ReliabilityScore.Calculate(onTime, avgDelay),
                    PeakScore = ReliabilityScore.Calculate(peakOnTime, avgDelay),
                    SampleCount = entries.Count
                };
            });

        foreach (var score in scores)
        {
            var existing = await db.ReliabilityScores
                .FirstOrDefaultAsync(r => r.RouteId == score.RouteId && r.ScoreDate == score.ScoreDate,
                    context.CancellationToken);

            if (existing is not null)
            {
                existing.OnTimePct = score.OnTimePct;
                existing.AvgDelaySeconds = score.AvgDelaySeconds;
                existing.Score = score.Score;
                existing.PeakScore = score.PeakScore;
                existing.SampleCount = score.SampleCount;
            }
            else
            {
                db.ReliabilityScores.Add(score);
            }
        }

        await db.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Delay aggregation complete — {Count} routes scored", scores.Count());
    }
}

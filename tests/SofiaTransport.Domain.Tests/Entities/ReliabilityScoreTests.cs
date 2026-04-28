using Xunit;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Domain.Tests.Entities;

public class ReliabilityScoreTests
{
    [Theory]
    [InlineData(1.0, 0, 100.0)]             // 100% on-time, 0 delay → perfect score
    [InlineData(0.9, 60, 85.0)]            // 90% on-time, 60s delay → 90 - 5 = 85
    [InlineData(0.8, 120, 70.0)]           // 80%, 120s → 80 - 10 = 70
    [InlineData(0.5, 300, 25.0)]           // 50%, 300s → 50 - 25 = 25
    [InlineData(0.0, 600, -50.0)]          // 0%, 600s → 0 - 50 = -50
    public void Calculate_ReturnsCorrectScore(double onTimePct, double avgDelaySeconds, double expected)
    {
        var result = ReliabilityScore.Calculate(onTimePct, avgDelaySeconds);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PenaltyFactor_IsFive()
    {
        Assert.Equal(5.0, ReliabilityScore.PenaltyFactor);
    }

    [Fact]
    public void Entity_PropertiesAreSetCorrectly()
    {
        var score = new ReliabilityScore
        {
            RouteId = "r-204",
            ScoreDate = new DateTime(2026, 1, 15),
            OnTimePct = 0.85,
            AvgDelaySeconds = 90,
            Score = 77.5,
            PeakScore = 70.0,
            SampleCount = 150
        };

        Assert.Equal("r-204", score.RouteId);
        Assert.Equal(new DateTime(2026, 1, 15), score.ScoreDate);
        Assert.Equal(0.85, score.OnTimePct);
        Assert.Equal(90, score.AvgDelaySeconds);
        Assert.Equal(77.5, score.Score);
        Assert.Equal(70.0, score.PeakScore);
        Assert.Equal(150, score.SampleCount);
    }
}

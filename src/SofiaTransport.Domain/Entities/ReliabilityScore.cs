namespace SofiaTransport.Domain.Entities;

public class ReliabilityScore
{
    public string RouteId { get; set; } = string.Empty;
    public DateTime ScoreDate { get; set; }
    public double OnTimePct { get; set; }
    public double AvgDelaySeconds { get; set; }
    public double Score { get; set; }
    public double PeakScore { get; set; }
    public int SampleCount { get; set; }

    public const double PenaltyFactor = 5.0;

    public static double Calculate(double onTimePct, double avgDelaySeconds)
    {
        return onTimePct * 100 - avgDelaySeconds / 60.0 * PenaltyFactor;
    }
}

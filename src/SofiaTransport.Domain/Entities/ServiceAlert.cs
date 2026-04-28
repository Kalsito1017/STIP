using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Entities;

public class ServiceAlert
{
    public string AlertId { get; set; } = string.Empty;
    public string HeaderText { get; set; } = string.Empty;
    public string? DescriptionText { get; set; }
    public string? Url { get; set; }
    public int Cause { get; set; }
    public int Effect { get; set; }
    public int? Severity { get; set; }
    public List<ActivePeriod> ActivePeriods { get; set; } = [];
    public List<InformedEntity> InformedEntities { get; set; } = [];
    public DateTime RecordedAt { get; set; }
}
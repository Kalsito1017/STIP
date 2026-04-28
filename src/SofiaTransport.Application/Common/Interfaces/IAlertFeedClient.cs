using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IAlertFeedClient
{
    Task<IReadOnlyList<ServiceAlert>> FetchAlertsAsync(CancellationToken ct);
}
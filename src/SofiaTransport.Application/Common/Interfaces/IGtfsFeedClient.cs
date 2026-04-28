using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IGtfsFeedClient
{
    Task<IReadOnlyList<Vehicle>> FetchVehiclePositionsAsync(CancellationToken ct);
}

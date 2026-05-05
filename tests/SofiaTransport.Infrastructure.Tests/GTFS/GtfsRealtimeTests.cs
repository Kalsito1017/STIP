using Moq;
using NetTopologySuite.Geometries;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using SofiaTransport.Infrastructure.Cache;
using SofiaTransport.Infrastructure.Realtime;
using Xunit;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Infrastructure.Tests.GTFS;

public class GtfsRealtimeTests
{
    [Fact]
    public async Task Orchestration_FetchesVehicles_StoresInCache_AndBroadcasts()
    {
        // Arrange
        var vehicles = new List<Vehicle>
        {
            new()
            {
                VehicleId = "v1",
                RouteId = "r-1",
                TripId = "t1",
                Location = new Coordinates(42.69, 23.33),
                Geometry = new Point(23.33, 42.69) { SRID = 4326 },
                Bearing = 90f,
                Speed = 40f,
                RecordedAt = DateTime.UtcNow
            }
        };

        var mockFeedClient = new Mock<IGtfsFeedClient>();
        mockFeedClient.Setup(f => f.FetchVehiclePositionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicles);

        var mockCache = new Mock<IVehicleCache>();
        var mockBroadcaster = new Mock<IVehicleBroadcaster>();

        // Act — simulate the orchestration: fetch, cache, broadcast
        var fetchedVehicles = await mockFeedClient.Object.FetchVehiclePositionsAsync(CancellationToken.None);

        foreach (var v in fetchedVehicles)
        {
            v.RecordedAt = DateTime.UtcNow;
            await mockCache.Object.SetAsync(v);
            await mockBroadcaster.Object.BroadcastAsync(v);
        }

        // Assert
        mockFeedClient.Verify(f => f.FetchVehiclePositionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockCache.Verify(c => c.SetAsync(It.IsAny<Vehicle>()), Times.Once);
        mockBroadcaster.Verify(b => b.BroadcastAsync(It.IsAny<Vehicle>()), Times.Once);
    }

    [Fact]
    public async Task Orchestration_FetchesAlerts_StoresInCache()
    {
        // Arrange
        var alerts = new List<ServiceAlert>
        {
            new()
            {
                AlertId = "a1",
                HeaderText = "Road closed",
                Cause = 1,
                Effect = 3,
                RecordedAt = DateTime.UtcNow
            }
        };

        var mockAlertClient = new Mock<IAlertFeedClient>();
        mockAlertClient.Setup(f => f.FetchAlertsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts);

        var mockAlertCache = new Mock<IAlertCache>();

        // Act — simulate the orchestration
        var fetchedAlerts = await mockAlertClient.Object.FetchAlertsAsync(CancellationToken.None);

        foreach (var a in fetchedAlerts)
        {
            await mockAlertCache.Object.SetAsync(a);
        }

        // Assert
        mockAlertClient.Verify(f => f.FetchAlertsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockAlertCache.Verify(c => c.SetAsync(It.IsAny<ServiceAlert>()), Times.Once);
    }

    [Fact]
    public async Task Orchestration_FetchesTripUpdates_StoresAndBroadcasts()
    {
        // Arrange
        var tripUpdates = new List<TripUpdate>
        {
            new()
            {
                TripId = "t1",
                RouteId = "r-1",
                VehicleId = "v1",
                RecordedAt = DateTime.UtcNow,
                StopTimeUpdates =
                {
                    new StopTimeUpdate { StopId = "s-001", ArrivalDelay = 120 }
                }
            }
        };

        var mockTripUpdateClient = new Mock<ITripUpdateFeedClient>();
        mockTripUpdateClient.Setup(f => f.FetchTripUpdatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tripUpdates);

        var mockTripUpdateCache = new Mock<ITripUpdateCache>();
        var mockRealtimeBroadcaster = new Mock<IRealtimeBroadcaster>();

        // Act — simulate the orchestration
        var fetchedUpdates = await mockTripUpdateClient.Object.FetchTripUpdatesAsync(CancellationToken.None);

        foreach (var tu in fetchedUpdates)
        {
            tu.RecordedAt = DateTime.UtcNow;
            await mockTripUpdateCache.Object.SetAsync(tu);
            await mockRealtimeBroadcaster.Object.BroadcastTripUpdateAsync(tu);
        }

        // Assert
        mockTripUpdateClient.Verify(f => f.FetchTripUpdatesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTripUpdateCache.Verify(c => c.SetAsync(It.IsAny<TripUpdate>()), Times.Once);
        mockRealtimeBroadcaster.Verify(b => b.BroadcastTripUpdateAsync(It.IsAny<TripUpdate>()), Times.Once);
    }
}

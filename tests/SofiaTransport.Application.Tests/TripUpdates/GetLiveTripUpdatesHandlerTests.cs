using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.TripUpdates;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.TripUpdates;

public class GetLiveTripUpdatesHandlerTests
{
    [Fact]
    public async Task Handle_NoRouteFilter_CallsGetAllAsync()
    {
        // Arrange
        var updates = new List<TripUpdate>
        {
            new()
            {
                TripId = "t-001",
                RouteId = "r-1",
                StartTime = "08:00",
                StartDate = "20260501",
                ScheduleRelationship = 0,
                VehicleId = "v-001",
                StopTimeUpdates = new List<StopTimeUpdate>
                {
                    new()
                    {
                        StopSequence = 1,
                        StopId = "s-001",
                        ArrivalDelay = 60,
                        ArrivalTime = 28800,
                        DepartureDelay = 30,
                        DepartureTime = 28830,
                        ScheduleRelationship = 0
                    }
                },
                RecordedAt = new DateTime(2026, 5, 1, 8, 5, 0, DateTimeKind.Utc)
            }
        };

        var mockCache = new Mock<ITripUpdateCache>();
        mockCache.Setup(c => c.GetAllAsync()).ReturnsAsync(updates);

        var handler = new GetLiveTripUpdatesHandler(mockCache.Object);

        // Act
        var result = await handler.Handle(new GetLiveTripUpdatesQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result);
        var dto = result[0];
        Assert.Equal("t-001", dto.TripId);
        Assert.Equal("r-1", dto.RouteId);
        Assert.Single(dto.StopTimeUpdates);
        Assert.Equal(60, dto.StopTimeUpdates[0].ArrivalDelay);
        mockCache.Verify(c => c.GetAllAsync(), Times.Once);
        mockCache.Verify(c => c.GetByRouteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRouteFilter_CallsGetByRouteAsync()
    {
        // Arrange
        var updates = new List<TripUpdate>
        {
            new()
            {
                TripId = "t-002",
                RouteId = "r-204",
                ScheduleRelationship = 0,
                StopTimeUpdates = [],
                RecordedAt = DateTime.UtcNow
            }
        };

        var mockCache = new Mock<ITripUpdateCache>();
        mockCache.Setup(c => c.GetByRouteAsync("r-204")).ReturnsAsync(updates);

        var handler = new GetLiveTripUpdatesHandler(mockCache.Object);

        // Act
        var result = await handler.Handle(new GetLiveTripUpdatesQuery("r-204"), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("t-002", result[0].TripId);
        mockCache.Verify(c => c.GetByRouteAsync("r-204"), Times.Once);
        mockCache.Verify(c => c.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_MapsStopTimeUpdatesCorrectly()
    {
        // Arrange
        var updates = new List<TripUpdate>
        {
            new()
            {
                TripId = "t-003",
                RouteId = "r-1",
                ScheduleRelationship = 3,
                StopTimeUpdates = new List<StopTimeUpdate>
                {
                    new()
                    {
                        StopSequence = 1,
                        StopId = "s-001",
                        ArrivalDelay = 120,
                        ArrivalTime = 36000,
                        DepartureDelay = 90,
                        DepartureTime = 36030,
                        ScheduleRelationship = 1
                    },
                    new()
                    {
                        StopSequence = 2,
                        StopId = "s-002",
                        ArrivalDelay = 180,
                        ArrivalTime = 36600,
                        DepartureDelay = null,
                        DepartureTime = null,
                        ScheduleRelationship = 0
                    }
                },
                RecordedAt = DateTime.UtcNow
            }
        };

        var mockCache = new Mock<ITripUpdateCache>();
        mockCache.Setup(c => c.GetAllAsync()).ReturnsAsync(updates);

        var handler = new GetLiveTripUpdatesHandler(mockCache.Object);

        // Act
        var result = await handler.Handle(new GetLiveTripUpdatesQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].StopTimeUpdates.Count);

        var stu1 = result[0].StopTimeUpdates[0];
        Assert.Equal(1, stu1.StopSequence);
        Assert.Equal("s-001", stu1.StopId);
        Assert.Equal(120, stu1.ArrivalDelay);
        Assert.Equal(90, stu1.DepartureDelay);

        var stu2 = result[0].StopTimeUpdates[1];
        Assert.Equal(2, stu2.StopSequence);
        Assert.Equal("s-002", stu2.StopId);
        Assert.Equal(180, stu2.ArrivalDelay);
        Assert.Null(stu2.DepartureDelay);
    }
}

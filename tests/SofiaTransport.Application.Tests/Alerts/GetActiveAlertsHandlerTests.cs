using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Alerts;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Application.Tests.Alerts;

public class GetActiveAlertsHandlerTests
{
    [Fact]
    public async Task Handle_NoRouteFilter_CallsGetAllAsync()
    {
        // Arrange
        var alerts = new List<ServiceAlert>
        {
            new()
            {
                AlertId = "a-001",
                HeaderText = "Route 1 delay",
                DescriptionText = "Delays due to construction",
                Url = "https://example.com/alert1",
                Cause = 1,
                Effect = 1,
                Severity = 3,
                ActivePeriods = new List<ActivePeriod>
                {
                    new() { Start = 1000, End = 2000 }
                },
                InformedEntities = new List<InformedEntity>
                {
                    new() { RouteId = "r-1", AgencyId = "ag-1" }
                },
                RecordedAt = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc)
            }
        };

        var mockCache = new Mock<IAlertCache>();
        mockCache.Setup(c => c.GetAllAsync()).ReturnsAsync(alerts);

        var handler = new GetActiveAlertsHandler(mockCache.Object);

        // Act
        var result = await handler.Handle(new GetActiveAlertsQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result);
        var dto = result[0];
        Assert.Equal("a-001", dto.AlertId);
        Assert.Equal("Route 1 delay", dto.HeaderText);
        Assert.Single(dto.ActivePeriods);
        Assert.Single(dto.InformedEntities);
        mockCache.Verify(c => c.GetAllAsync(), Times.Once);
        mockCache.Verify(c => c.GetByRouteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRouteFilter_CallsGetByRouteAsync()
    {
        // Arrange
        var alerts = new List<ServiceAlert>
        {
            new()
            {
                AlertId = "a-002",
                HeaderText = "Route 204 detour",
                Cause = 2,
                Effect = 2,
                ActivePeriods = [],
                InformedEntities = [],
                RecordedAt = DateTime.UtcNow
            }
        };

        var mockCache = new Mock<IAlertCache>();
        mockCache.Setup(c => c.GetByRouteAsync("r-204")).ReturnsAsync(alerts);

        var handler = new GetActiveAlertsHandler(mockCache.Object);

        // Act
        var result = await handler.Handle(new GetActiveAlertsQuery("r-204"), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("a-002", result[0].AlertId);
        mockCache.Verify(c => c.GetByRouteAsync("r-204"), Times.Once);
        mockCache.Verify(c => c.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var mockCache = new Mock<IAlertCache>();
        mockCache.Setup(c => c.GetAllAsync()).ReturnsAsync(Array.Empty<ServiceAlert>());

        var handler = new GetActiveAlertsHandler(mockCache.Object);

        // Act
        var result = await handler.Handle(new GetActiveAlertsQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}

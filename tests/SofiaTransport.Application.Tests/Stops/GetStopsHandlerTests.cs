using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Stops;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Application.Tests.Stops;

public class GetStopsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllStops()
    {
        var stops = new List<Stop>
        {
            new() { StopId = "s-001", StopName = "Orlov Most", Location = new Coordinates(42.6897, 23.3342) },
            new() { StopId = "s-002", StopName = "NDK", Location = new Coordinates(42.6871, 23.3186) },
        };

        var mockRepo = new Mock<IStopRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(stops);

        var handler = new GetStopsHandler(mockRepo.Object);
        var result = await handler.Handle(new GetStopsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.StopId == "s-001" && s.StopName == "Orlov Most");
        Assert.Contains(result, s => s.StopId == "s-002" && s.StopName == "NDK");
        Assert.All(result, s => Assert.True(s.Lat >= 42.5 && s.Lat <= 42.85));
        Assert.All(result, s => Assert.True(s.Lon >= 23.1 && s.Lon <= 23.6));
    }
}

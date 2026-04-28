using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Routes;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Routes;

public class GetRoutesHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllRoutes()
    {
        var routes = new List<Route>
        {
            new() { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new() { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus },
        };

        var mockRepo = new Moq.Mock<IRouteRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(routes);

        var handler = new GetRoutesHandler(mockRepo.Object);
        var result = await handler.Handle(new GetRoutesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.RouteId == "r-1" && r.ShortName == "1");
        Assert.Contains(result, r => r.RouteId == "r-204" && r.ShortName == "204");
    }

    [Fact]
    public async Task Handle_EmptyRepository_ReturnsEmptyList()
    {
        var mockRepo = new Moq.Mock<IRouteRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Route>());

        var handler = new GetRoutesHandler(mockRepo.Object);
        var result = await handler.Handle(new GetRoutesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}

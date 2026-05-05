using Xunit;
using SofiaTransport.Application.Routes;

namespace SofiaTransport.Application.Tests.Routes;

public class GetRouteDelayPatternValidatorTests
{
    [Fact]
    public void Validate_ValidRouteId_Passes()
    {
        // Arrange
        var validator = new GetRouteDelayPatternValidator();
        var query = new GetRouteDelayPatternQuery("r-1");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyRouteId_Fails()
    {
        // Arrange
        var validator = new GetRouteDelayPatternValidator();
        var query = new GetRouteDelayPatternQuery("");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteId");
    }
}

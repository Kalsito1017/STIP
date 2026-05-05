using Xunit;
using SofiaTransport.Application.Routes;

namespace SofiaTransport.Application.Tests.Routes;

public class GetRouteDetailValidatorTests
{
    [Fact]
    public void Validate_ValidRouteId_Passes()
    {
        // Arrange
        var validator = new GetRouteDetailValidator();
        var query = new GetRouteDetailQuery("r-1");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyRouteId_Fails()
    {
        // Arrange
        var validator = new GetRouteDetailValidator();
        var query = new GetRouteDetailQuery("");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteId");
    }

    [Fact]
    public void Validate_RouteIdExceedsMaxLength_Fails()
    {
        // Arrange
        var validator = new GetRouteDetailValidator();
        var query = new GetRouteDetailQuery(new string('x', 51));

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteId");
    }
}

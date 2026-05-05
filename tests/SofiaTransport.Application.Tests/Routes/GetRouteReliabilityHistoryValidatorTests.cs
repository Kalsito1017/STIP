using Xunit;
using SofiaTransport.Application.Routes;

namespace SofiaTransport.Application.Tests.Routes;

public class GetRouteReliabilityHistoryValidatorTests
{
    [Fact]
    public void Validate_ValidWithNoDates_Passes()
    {
        // Arrange
        var validator = new GetRouteReliabilityHistoryValidator();
        var query = new GetRouteReliabilityHistoryQuery("r-1");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ValidWithFromBeforeTo_Passes()
    {
        // Arrange
        var validator = new GetRouteReliabilityHistoryValidator();
        var query = new GetRouteReliabilityHistoryQuery("r-1", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyRouteId_Fails()
    {
        // Arrange
        var validator = new GetRouteReliabilityHistoryValidator();
        var query = new GetRouteReliabilityHistoryQuery("");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteId");
    }

    [Fact]
    public void Validate_FromGreaterThanTo_Fails()
    {
        // Arrange
        var validator = new GetRouteReliabilityHistoryValidator();
        var query = new GetRouteReliabilityHistoryQuery(
            "r-1",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(-7));

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "From date must be before or equal to To date.");
    }
}

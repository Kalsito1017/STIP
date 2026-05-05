using Xunit;
using SofiaTransport.Application.Stops;

namespace SofiaTransport.Application.Tests.Stops;

public class GetNearbyStopsValidatorTests
{
    [Fact]
    public void Validate_ValidCoordinates_Passes()
    {
        // Arrange
        var validator = new GetNearbyStopsValidator();
        var query = new GetNearbyStopsQuery(42.6977, 23.3219, 1.0);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_LatBelowMinimum_Fails()
    {
        // Arrange
        var validator = new GetNearbyStopsValidator();
        var query = new GetNearbyStopsQuery(42.4, 23.3219, 1.0);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage!.Contains("42.5"));
    }

    [Fact]
    public void Validate_LatAboveMaximum_Fails()
    {
        // Arrange
        var validator = new GetNearbyStopsValidator();
        var query = new GetNearbyStopsQuery(42.86, 23.3219, 1.0);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage!.Contains("42.85"));
    }

    [Fact]
    public void Validate_LonBelowMinimum_Fails()
    {
        // Arrange
        var validator = new GetNearbyStopsValidator();
        var query = new GetNearbyStopsQuery(42.6977, 23.0, 1.0);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage!.Contains("23.1"));
    }

    [Fact]
    public void Validate_LonAboveMaximum_Fails()
    {
        // Arrange
        var validator = new GetNearbyStopsValidator();
        var query = new GetNearbyStopsQuery(42.6977, 23.7, 1.0);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage!.Contains("23.6"));
    }

    [Fact]
    public void Validate_RadiusOutsideRange_Fails()
    {
        // Arrange
        var validator = new GetNearbyStopsValidator();
        var query = new GetNearbyStopsQuery(42.6977, 23.3219, 10.0);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage!.Contains("0.1 and 5.0"));
    }
}

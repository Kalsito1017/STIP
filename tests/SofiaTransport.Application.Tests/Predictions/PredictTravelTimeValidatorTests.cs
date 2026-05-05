using Xunit;
using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Application.Tests.Predictions;

public class PredictTravelTimeValidatorTests
{
    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var validator = new PredictTravelTimeValidator();
        var command = new PredictTravelTimeCommand(
            "s-001", "s-002", "r-1", DateTime.UtcNow.AddHours(1));

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyFromStopId_Fails()
    {
        // Arrange
        var validator = new PredictTravelTimeValidator();
        var command = new PredictTravelTimeCommand(
            "", "s-002", "r-1", DateTime.UtcNow.AddHours(1));

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FromStopId");
    }

    [Fact]
    public void Validate_EmptyToStopId_Fails()
    {
        // Arrange
        var validator = new PredictTravelTimeValidator();
        var command = new PredictTravelTimeCommand(
            "s-001", "", "r-1", DateTime.UtcNow.AddHours(1));

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ToStopId");
    }

    [Fact]
    public void Validate_EmptyRouteId_Fails()
    {
        // Arrange
        var validator = new PredictTravelTimeValidator();
        var command = new PredictTravelTimeCommand(
            "s-001", "s-002", "", DateTime.UtcNow.AddHours(1));

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteId");
    }

    [Fact]
    public void Validate_DepartureTimeInPast_Fails()
    {
        // Arrange
        var validator = new PredictTravelTimeValidator();
        var command = new PredictTravelTimeCommand(
            "s-001", "s-002", "r-1", DateTime.UtcNow.AddDays(-1));

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DepartureTime");
    }
}

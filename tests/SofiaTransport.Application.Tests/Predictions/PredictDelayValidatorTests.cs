using Xunit;
using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Application.Tests.Predictions;

public class PredictDelayValidatorTests
{
    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var validator = new PredictDelayValidator();
        var command = new PredictDelayCommand("r-1", "s-001", 12, 3, 5);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyRouteId_Fails()
    {
        // Arrange
        var validator = new PredictDelayValidator();
        var command = new PredictDelayCommand("", "s-001", 12, 3, 5);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteId");
    }

    [Fact]
    public void Validate_EmptyStopId_Fails()
    {
        // Arrange
        var validator = new PredictDelayValidator();
        var command = new PredictDelayCommand("r-1", "", 12, 3, 5);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StopId");
    }

    [Theory]
    [InlineData(-1, "Hour")]
    [InlineData(24, "Hour")]
    public void Validate_HourOutOfRange_Fails(int hour, string expectedProperty)
    {
        // Arrange
        var validator = new PredictDelayValidator();
        var command = new PredictDelayCommand("r-1", "s-001", hour, 3, 5);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == expectedProperty);
    }

    [Theory]
    [InlineData(-1, "DayOfWeek")]
    [InlineData(7, "DayOfWeek")]
    public void Validate_DayOfWeekOutOfRange_Fails(int dayOfWeek, string expectedProperty)
    {
        // Arrange
        var validator = new PredictDelayValidator();
        var command = new PredictDelayCommand("r-1", "s-001", 12, dayOfWeek, 5);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == expectedProperty);
    }

    [Fact]
    public void Validate_StopSequenceZero_Fails()
    {
        // Arrange
        var validator = new PredictDelayValidator();
        var command = new PredictDelayCommand("r-1", "s-001", 12, 3, 0);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StopSequence");
    }
}

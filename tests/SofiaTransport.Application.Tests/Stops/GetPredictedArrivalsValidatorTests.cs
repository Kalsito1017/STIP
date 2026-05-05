using Xunit;
using SofiaTransport.Application.Stops;

namespace SofiaTransport.Application.Tests.Stops;

public class GetPredictedArrivalsValidatorTests
{
    [Fact]
    public void Validate_ValidStopId_Passes()
    {
        // Arrange
        var validator = new GetPredictedArrivalsValidator();
        var query = new GetPredictedArrivalsQuery("s-001");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyStopId_Fails()
    {
        // Arrange
        var validator = new GetPredictedArrivalsValidator();
        var query = new GetPredictedArrivalsQuery("");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StopId");
    }
}

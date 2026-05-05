using Xunit;
using SofiaTransport.Application.Analytics;

namespace SofiaTransport.Application.Tests.Analytics;

public class GetDelayHeatmapValidatorTests
{
    [Fact]
    public void Validate_ValidWithNoDates_Passes()
    {
        // Arrange
        var validator = new GetDelayHeatmapValidator();
        var query = new GetDelayHeatmapQuery();

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ValidWithFromBeforeTo_Passes()
    {
        // Arrange
        var validator = new GetDelayHeatmapValidator();
        var query = new GetDelayHeatmapQuery(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_FromGreaterThanTo_Fails()
    {
        // Arrange
        var validator = new GetDelayHeatmapValidator();
        var query = new GetDelayHeatmapQuery(DateTime.UtcNow, DateTime.UtcNow.AddDays(-7));

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "From date must be before or equal to To date.");
    }
}

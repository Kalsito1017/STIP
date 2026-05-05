using Xunit;
using SofiaTransport.Application.Analytics;

namespace SofiaTransport.Application.Tests.Analytics;

public class GetReliabilityRankingValidatorTests
{
    [Fact]
    public void Validate_ValidTop_Passes()
    {
        // Arrange
        var validator = new GetReliabilityRankingValidator();
        var query = new GetReliabilityRankingQuery(10);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_TopZero_Fails()
    {
        // Arrange
        var validator = new GetReliabilityRankingValidator();
        var query = new GetReliabilityRankingQuery(0);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Top must be between 1 and 100.");
    }

    [Fact]
    public void Validate_TopExceedsMax_Fails()
    {
        // Arrange
        var validator = new GetReliabilityRankingValidator();
        var query = new GetReliabilityRankingQuery(101);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Top must be between 1 and 100.");
    }
}

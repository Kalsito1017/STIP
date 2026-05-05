using Xunit;
using SofiaTransport.Application.Routes;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Routes;

public class GetRoutesValidatorTests
{
    [Fact]
    public void Validate_ValidQuery_NullType_Passes()
    {
        // Arrange
        var validator = new GetRoutesValidator();
        var query = new GetRoutesQuery(null);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ValidQuery_ValidEnum_Passes()
    {
        // Arrange
        var validator = new GetRoutesValidator();
        var query = new GetRoutesQuery(TransitType.Bus);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidTransitType_Fails()
    {
        // Arrange
        var validator = new GetRoutesValidator();
        var query = new GetRoutesQuery((TransitType)999);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Type must be a valid transit type.");
    }
}

using Xunit;
using SofiaTransport.Application.Vehicles;

namespace SofiaTransport.Application.Tests.Vehicles;

public class GetLiveVehiclesValidatorTests
{
    [Fact]
    public void Validate_ValidWithoutRouteId_Passes()
    {
        // Arrange
        var validator = new GetLiveVehiclesValidator();
        var query = new GetLiveVehiclesQuery(null);

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ValidRouteId_Passes()
    {
        // Arrange
        var validator = new GetLiveVehiclesValidator();
        var query = new GetLiveVehiclesQuery("r-1");

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RouteIdExceedsMaxLength_Fails()
    {
        // Arrange
        var validator = new GetLiveVehiclesValidator();
        var query = new GetLiveVehiclesQuery(new string('x', 51));

        // Act
        var result = validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RouteId");
    }
}

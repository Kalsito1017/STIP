using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using SofiaTransport.Application.Predictions;
using SofiaTransport.Infrastructure.ML;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.ML;

public class MLServiceTests
{
    private static Mock<HttpMessageHandler> CreateMessageHandler(HttpStatusCode statusCode, string content)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            });
        return handler;
    }

    [Fact]
    public async Task PredictDelayAsync_SendsCorrectJson_ReturnsResponse()
    {
        // Arrange
        var responseJson = JsonSerializer.Serialize(
            new { predictedDelaySeconds = 180.0, confidenceInterval = new[] { 120.0, 240.0 }, modelVersion = "v1.0" },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var handler = CreateMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://ml.example.com/") };
        var service = new MLService(httpClient);

        // Act
        var result = await service.PredictDelayAsync("r-1", "s-001", 9, 2, 1, CancellationToken.None);

        // Assert
        Assert.Equal(180.0, result.PredictedDelaySeconds);
        Assert.Equal(120.0, result.ConfidenceInterval[0]);
        Assert.Equal(240.0, result.ConfidenceInterval[1]);
        Assert.Equal("v1.0", result.ModelVersion);
    }

    [Fact]
    public async Task PredictDelayAsync_HandlesHttpError()
    {
        // Arrange
        var handler = CreateMessageHandler(HttpStatusCode.InternalServerError, "");
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://ml.example.com/") };
        var service = new MLService(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.PredictDelayAsync("r-1", "s-001", 9, 2, 1, CancellationToken.None));
    }

    [Fact]
    public async Task PredictTravelTimeAsync_SendsCorrectJson_ReturnsResponse()
    {
        // Arrange
        var responseJson = JsonSerializer.Serialize(
            new { predictedTravelTimeSeconds = 600.0, confidenceInterval = new[] { 500.0, 700.0 }, modelVersion = "v2.0" },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var handler = CreateMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://ml.example.com/") };
        var service = new MLService(httpClient);

        // Act
        var result = await service.PredictTravelTimeAsync("s-001", "s-002", "r-1", DateTime.UtcNow, CancellationToken.None);

        // Assert
        Assert.Equal(600.0, result.PredictedTravelTimeSeconds);
        Assert.Equal(500.0, result.ConfidenceInterval[0]);
        Assert.Equal(700.0, result.ConfidenceInterval[1]);
        Assert.Equal("v2.0", result.ModelVersion);
    }

    [Fact]
    public async Task PredictTravelTimeAsync_HandlesHttpError()
    {
        // Arrange
        var handler = CreateMessageHandler(HttpStatusCode.ServiceUnavailable, "");
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://ml.example.com/") };
        var service = new MLService(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.PredictTravelTimeAsync("s-001", "s-002", "r-1", DateTime.UtcNow, CancellationToken.None));
    }

    [Fact]
    public async Task PredictDelaysBatchAsync_SendsBatchRequest_ReturnsResponse()
    {
        // Arrange
        var responseResults = new List<object>
        {
            new { predictedDelaySeconds = 120.0, confidenceInterval = new[] { 60.0, 180.0 }, modelVersion = "v1.0", input = new { routeId = "r-1", stopId = "s-001", hour = 9, dayOfWeek = 1, stopSequence = 1 } }
        };
        var responseJson = JsonSerializer.Serialize(
            new { results = responseResults },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var handler = CreateMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://ml.example.com/") };
        var service = new MLService(httpClient);

        var request = new BatchPredictDelayRequest(new List<PredictDelayRequest>
        {
            new("r-1", "s-001", 9, 1, 1)
        });

        // Act
        var result = await service.PredictDelaysBatchAsync(request, CancellationToken.None);

        // Assert
        Assert.Single(result.Results);
        Assert.Equal(120.0, result.Results[0].PredictedDelaySeconds);
    }

    [Fact]
    public async Task PredictDelaysBatchAsync_HandlesHttpError()
    {
        // Arrange
        var handler = CreateMessageHandler(HttpStatusCode.BadGateway, "");
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://ml.example.com/") };
        var service = new MLService(httpClient);

        var request = new BatchPredictDelayRequest(new List<PredictDelayRequest>
        {
            new("r-1", "s-001", 9, 1, 1)
        });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.PredictDelaysBatchAsync(request, CancellationToken.None));
    }
}

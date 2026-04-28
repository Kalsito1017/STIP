using System.Text;
using System.Text.Json;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Infrastructure.ML;

public class MLService : IMLService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public MLService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<PredictDelayResponse> PredictDelayAsync(
        string routeId, string stopId, int hour, int dayOfWeek,
        int stopSequence, CancellationToken ct)
    {
        var request = new { routeId, stopId, hour, dayOfWeek, stopSequence };
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/predict", content, ct);
        response.EnsureSuccessStatusCode();

        var resultJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<PredictDelayResponse>(resultJson, JsonOptions);
        return result ?? new PredictDelayResponse(0, new List<double> { 0, 0 }, "unknown");
    }

    public Task<TravelTimePredictionResponse> PredictTravelTimeAsync(
        string fromStopId, string toStopId, string routeId, DateTime departureTime, CancellationToken ct)
    {
        throw new NotImplementedException("Travel time prediction is computed via heuristic handler.");
    }
}

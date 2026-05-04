using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Api.Controllers;

public record PredictDelayRequest(
    string RouteId,
    string StopId,
    int Hour,
    int DayOfWeek,
    int StopSequence
);

public record PredictTravelTimeRequest(
    string FromStopId,
    string ToStopId,
    string RouteId,
    DateTime DepartureTime
);

[ApiController]
[Route("api/predictions")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PredictionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("delay")]
    [ProducesResponseType(typeof(PredictDelayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PredictDelayResponse>> PredictDelay([FromBody] PredictDelayRequest request)
    {
        var result = await _mediator.Send(new PredictDelayCommand(
            request.RouteId, request.StopId, request.Hour,
            request.DayOfWeek, request.StopSequence));
        return Ok(result);
    }

    [HttpPost("travel-time")]
    [ProducesResponseType(typeof(TravelTimePredictionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TravelTimePredictionResponse>> PredictTravelTime([FromBody] PredictTravelTimeRequest request)
    {
        var result = await _mediator.Send(new PredictTravelTimeCommand(
            request.FromStopId, request.ToStopId, request.RouteId, request.DepartureTime));
        return Ok(result);
    }
}

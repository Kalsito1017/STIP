using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Stops;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/stops")]
[Authorize]
public class StopsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StopsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StopDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StopDto>>> GetAll()
    {
        var stops = await _mediator.Send(new GetStopsQuery());
        return Ok(stops);
    }

    [HttpGet("nearby")]
    [ProducesResponseType(typeof(IReadOnlyList<StopDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<StopDto>>> GetNearby([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double radiusKm = 1.0)
    {
        var stops = await _mediator.Send(new GetNearbyStopsQuery(lat, lon, radiusKm));
        return Ok(stops);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StopDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StopDto>> GetById(string id)
    {
        var stop = await _mediator.Send(new GetStopByIdQuery(id));
        return stop is not null ? Ok(stop) : NotFound();
    }

    [HttpGet("{id}/congestion")]
    [ProducesResponseType(typeof(IReadOnlyList<StopCongestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<StopCongestionDto>>> GetCongestion(string id, [FromQuery] DateTime? date)
    {
        var congestion = await _mediator.Send(new GetStopCongestionQuery(id, date));
        return Ok(congestion);
    }

    [HttpGet("{id}/predicted-arrivals")]
    [ProducesResponseType(typeof(IReadOnlyList<PredictedArrivalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PredictedArrivalDto>>> GetPredictedArrivals(string id)
    {
        var arrivals = await _mediator.Send(new GetPredictedArrivalsQuery(id));
        return Ok(arrivals);
    }
}

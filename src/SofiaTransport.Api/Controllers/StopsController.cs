using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Stops;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/stops")]
public class StopsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StopsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stops = await _mediator.Send(new GetStopsQuery());
        return Ok(stops);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var stops = await _mediator.Send(new GetStopsQuery());
        var stop = stops.FirstOrDefault(s => s.StopId == id);
        return stop is not null ? Ok(stop) : NotFound();
    }

    [HttpGet("{id}/congestion")]
    public async Task<IActionResult> GetCongestion(string id, [FromQuery] DateTime? date)
    {
        var congestion = await _mediator.Send(new GetStopCongestionQuery(id, date));
        return Ok(congestion);
    }
}

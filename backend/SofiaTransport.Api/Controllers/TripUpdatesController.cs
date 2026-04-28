using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.TripUpdates;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/tripupdates")]
[Authorize]
public class TripUpdatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TripUpdatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("live")]
    public async Task<ActionResult<IReadOnlyList<TripUpdateDto>>> GetLive([FromQuery] string? routeId)
    {
        var updates = await _mediator.Send(new GetLiveTripUpdatesQuery(routeId));
        return Ok(updates);
    }
}
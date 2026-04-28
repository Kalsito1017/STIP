using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Vehicles;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("live")]
    public async Task<IActionResult> GetLive([FromQuery] string? routeId)
    {
        var vehicles = await _mediator.Send(new GetLiveVehiclesQuery(routeId));
        return Ok(vehicles);
    }
}

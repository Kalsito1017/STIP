using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Routes;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/routes")]
public class RoutesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoutesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var routes = await _mediator.Send(new GetRoutesQuery());
        return Ok(routes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var route = await _mediator.Send(new GetRouteDetailQuery(id));
        return route is not null ? Ok(route) : NotFound();
    }

    [HttpGet("{id}/reliability")]
    public async Task<IActionResult> GetReliability(string id)
    {
        var route = await _mediator.Send(new GetRouteDetailQuery(id));
        return route is not null ? Ok(route.LatestReliability) : NotFound();
    }

    [HttpGet("{id}/delay-pattern")]
    public async Task<IActionResult> GetDelayPattern(string id, [FromQuery] DateTime? date)
    {
        var pattern = await _mediator.Send(new GetRouteDelayPatternQuery(id, date));
        return Ok(pattern);
    }
}

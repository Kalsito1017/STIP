using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Routes;
using SofiaTransport.Application.Shapes;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/routes")]
public class RoutesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoutesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RouteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteDto>>> GetAll([FromQuery] TransitType? type)
    {
        var routes = await _mediator.Send(new GetRoutesQuery(type));
        return Ok(routes);
    }

    [HttpGet("shapes")]
    [ProducesResponseType(typeof(RouteShapeCollection), StatusCodes.Status200OK)]
    public async Task<ActionResult<RouteShapeCollection>> GetAllShapes()
    {
        var shapes = await _mediator.Send(new GetAllRouteShapesQuery());
        return Ok(shapes);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RouteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteDetailDto>> GetById(string id)
    {
        var route = await _mediator.Send(new GetRouteDetailQuery(id));
        return route is not null ? Ok(route) : NotFound();
    }

    [HttpGet("{id}/shape")]
    [ProducesResponseType(typeof(RouteShapeCollection), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteShapeCollection>> GetShape(string id)
    {
        var shape = await _mediator.Send(new GetRouteShapeQuery(id));
        return shape is not null ? Ok(shape) : NotFound();
    }

    [HttpGet("{id}/reliability")]
    [ProducesResponseType(typeof(ReliabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReliabilityDto>> GetReliability(string id)
    {
        var route = await _mediator.Send(new GetRouteDetailQuery(id));
        return route is not null ? Ok(route.LatestReliability) : NotFound();
    }

    [HttpGet("{id}/reliability-history")]
    [ProducesResponseType(typeof(IReadOnlyList<ReliabilityHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReliabilityHistoryDto>>> GetReliabilityHistory(string id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var history = await _mediator.Send(new GetRouteReliabilityHistoryQuery(id, from, to));
        return Ok(history);
    }

    [HttpGet("{id}/delay-pattern")]
    [ProducesResponseType(typeof(IReadOnlyList<DelayPatternDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DelayPatternDto>>> GetDelayPattern(string id, [FromQuery] DateTime? date)
    {
        var pattern = await _mediator.Send(new GetRouteDelayPatternQuery(id, date));
        return Ok(pattern);
    }
}

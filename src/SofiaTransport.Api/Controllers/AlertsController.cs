using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Alerts;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceAlertDto>>> GetActive([FromQuery] string? routeId)
    {
        var alerts = await _mediator.Send(new GetActiveAlertsQuery(routeId));
        return Ok(alerts);
    }
}
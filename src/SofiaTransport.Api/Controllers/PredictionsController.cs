using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/predictions")]
public class PredictionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PredictionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("delay")]
    public async Task<IActionResult> PredictDelay([FromBody] PredictDelayRequest request)
    {
        var result = await _mediator.Send(new PredictDelayCommand(
            request.RouteId, request.StopId, request.Hour,
            request.DayOfWeek, request.StopSequence));
        return Ok(result);
    }
}

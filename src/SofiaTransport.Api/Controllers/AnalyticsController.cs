using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Analytics;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("heatmap/delays")]
    public async Task<IActionResult> GetDelayHeatmap([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var heatmap = await _mediator.Send(new GetDelayHeatmapQuery(from, to));
        return Ok(heatmap);
    }

    [HttpGet("reliability/ranking")]
    public async Task<IActionResult> GetReliabilityRanking([FromQuery] int top = 10, [FromQuery] bool best = true)
    {
        var ranking = await _mediator.Send(new GetReliabilityRankingQuery(top, best));
        return Ok(ranking);
    }

    [HttpGet("peak-hours")]
    public async Task<IActionResult> GetPeakHours([FromQuery] DateTime? date)
    {
        var peakHours = await _mediator.Send(new GetPeakHoursQuery(date));
        return Ok(peakHours);
    }
}

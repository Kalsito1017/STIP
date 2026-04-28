using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Analytics;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("overview")]
    [ProducesResponseType(typeof(SystemOverviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemOverviewDto>> GetOverview()
    {
        var overview = await _mediator.Send(new GetSystemOverviewQuery());
        return Ok(overview);
    }

    [HttpGet("heatmap/delays")]
    [ProducesResponseType(typeof(IReadOnlyList<HeatmapPointDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HeatmapPointDto>>> GetDelayHeatmap([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var heatmap = await _mediator.Send(new GetDelayHeatmapQuery(from, to));
        return Ok(heatmap);
    }

    [HttpGet("reliability/ranking")]
    [ProducesResponseType(typeof(IReadOnlyList<ReliabilityRankingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReliabilityRankingDto>>> GetReliabilityRanking([FromQuery] int top = 10, [FromQuery] bool best = true)
    {
        var ranking = await _mediator.Send(new GetReliabilityRankingQuery(top, best));
        return Ok(ranking);
    }

    [HttpGet("peak-hours")]
    [ProducesResponseType(typeof(IReadOnlyList<PeakHourDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PeakHourDto>>> GetPeakHours([FromQuery] DateTime? date)
    {
        var peakHours = await _mediator.Send(new GetPeakHoursQuery(date));
        return Ok(peakHours);
    }
}

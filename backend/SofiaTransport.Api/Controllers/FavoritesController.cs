using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Application.Favorites;

namespace SofiaTransport.Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FavoritesController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim is null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException();
        return userId;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FavoriteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FavoriteDto>>> GetAll()
    {
        var favorites = await _mediator.Send(new GetUserFavoritesQuery(GetUserId()));
        return Ok(favorites);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FavoriteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FavoriteDto>> Add([FromBody] AddFavoriteRequest request)
    {
        var favorite = await _mediator.Send(new AddFavoriteCommand(GetUserId(), request.EntityType, request.EntityId));
        return CreatedAtAction(nameof(GetAll), favorite);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(long id)
    {
        var removed = await _mediator.Send(new RemoveFavoriteCommand(GetUserId(), id));
        return removed ? NoContent() : NotFound();
    }
}

public record AddFavoriteRequest(string EntityType, string EntityId);

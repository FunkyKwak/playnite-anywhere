using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayniteAnywhere.Backend.Data;

namespace PlayniteAnywhere.Backend.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly PlayniteAnywhereDbContext db;

    public GamesController(PlayniteAnywhereDbContext db)
    {
        this.db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetGames()
    {
        var games = await db.Games
            .AsNoTracking()
            .Select(game => new
            {
                id = game.Id,
                name = game.Name,
                favorite = game.Favorite,
                source = game.Source,
                completionStatus = game.CompletionStatus,
                platforms = game.Platforms,
                cover = game.CoverSize.HasValue
                    ? $"/api/covers/{game.Id}"
                    : null
            })
            .ToListAsync();

        return Ok(games);
    }
}
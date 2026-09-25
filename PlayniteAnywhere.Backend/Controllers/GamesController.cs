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
                cover = $"/api/covers/{game.Id}"
            })
            .ToListAsync();

        return Ok(games);
    }
}
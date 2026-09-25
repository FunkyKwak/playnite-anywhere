using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayniteAnywhere.Backend.Data;
using PlayniteAnywhere.Backend.Models;
using PlayniteAnywhere.Common.Models;

namespace PlayniteAnywhere.Backend.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly PlayniteAnywhereDbContext db;

    public SyncController(PlayniteAnywhereDbContext db)
    {
        this.db = db;
    }

    [HttpPost("games")]
    public async Task<IActionResult> SyncGames(
        [FromBody] List<SyncGameRequest> games)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var existingGames = await db.Games
            .ToDictionaryAsync(game => game.Id);

        var receivedIds = new HashSet<Guid>();

        foreach (var incomingGame in games)
        {
            receivedIds.Add(incomingGame.Id);

            if (existingGames.TryGetValue(
                incomingGame.Id,
                out var existingGame))
            {
                existingGame.Name = incomingGame.Name;
                existingGame.Favorite = incomingGame.Favorite;
                existingGame.Source = incomingGame.Source;
                existingGame.CompletionStatus =
                    incomingGame.CompletionStatus;
            }
            else
            {
                db.Games.Add(new Game
                {
                    Id = incomingGame.Id,
                    Name = incomingGame.Name,
                    Favorite = incomingGame.Favorite,
                    Source = incomingGame.Source,
                    CompletionStatus =
                        incomingGame.CompletionStatus
                });
            }
        }

        var gamesToDelete = existingGames.Values
            .Where(game => !receivedIds.Contains(game.Id))
            .ToList();

        db.Games.RemoveRange(gamesToDelete);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            received = games.Count,
            deleted = gamesToDelete.Count,
            total = await db.Games.CountAsync()
        });
    }

    

    [HttpPost("covers/{id:guid}")]
    public async Task<IActionResult> SyncCover(
        Guid id,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Fichier manquant.");
        }

        var game = await db.Games.FindAsync(id);

        if (game == null)
        {
            return NotFound("Jeu introuvable.");
        }

        var coversDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "covers"
        );

        Directory.CreateDirectory(coversDirectory);

        var extension = Path.GetExtension(file.FileName);

        var filePath = Path.Combine(
            coversDirectory,
            $"{id}{extension}"
        );

        await using var stream = System.IO.File.Create(filePath);

        await file.CopyToAsync(stream);

        return Ok(new
        {
            id,
            size = file.Length
        });
    }

}
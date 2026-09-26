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
    private readonly ILogger<SyncController> logger;

    public SyncController(
        PlayniteAnywhereDbContext db,
        ILogger<SyncController> logger)
    {
        this.db = db;
        this.logger = logger;
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
        var coversToSync = new List<Guid>();
        var coversToRemove = new List<Guid>();
        
        logger.LogInformation("Début synchronisation : {Count} jeux reçus.", games.Count);

        foreach (var incomingGame in games)
        {
            receivedIds.Add(incomingGame.Id);

            if (existingGames.TryGetValue(
                incomingGame.Id,
                out var existingGame))
            {
                var coverChanged =
                    incomingGame.CoverSize != null &&
                        (existingGame.CoverSize != incomingGame.CoverSize ||
                        existingGame.CoverLastWriteTimeUtcTicks != incomingGame.CoverLastWriteTimeUtcTicks);
                var coverRemoved = incomingGame.CoverSize == null && existingGame.CoverSize != null;

                if (coverChanged)
                {
                    logger.LogInformation($"Cover modifiée ! {incomingGame.Name}");
                    coversToSync.Add(incomingGame.Id);
                }
                if (coverRemoved)
                {
                    logger.LogInformation($"Cover supprimée ! {incomingGame.Name}");
                    coversToRemove.Add(incomingGame.Id);   
                }

                existingGame.Name = incomingGame.Name;
                existingGame.Favorite = incomingGame.Favorite;
                existingGame.Source = incomingGame.Source;
                existingGame.CompletionStatus = incomingGame.CompletionStatus;
                existingGame.CoverSize = incomingGame.CoverSize;
                existingGame.CoverLastWriteTimeUtcTicks = incomingGame.CoverLastWriteTimeUtcTicks;
            }
            else
            {
                db.Games.Add(new Game
                {
                    Id = incomingGame.Id,
                    Name = incomingGame.Name,
                    Favorite = incomingGame.Favorite,
                    Source = incomingGame.Source,
                    CompletionStatus = incomingGame.CompletionStatus
                });

                if (incomingGame.CoverSize.HasValue)
                {
                    coversToSync.Add(incomingGame.Id);
                }
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
            total = await db.Games.CountAsync(),
            coversToSync,
            coversToRemove
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

        // Supprime toute ancienne version de la cover
        var existingFiles = Directory.GetFiles(
            coversDirectory,
            $"{id}.*"
        );
        foreach (var existingFile in existingFiles)
        {
            System.IO.File.Delete(existingFile);
            logger.LogInformation(
                "Ancienne cover supprimée : {FilePath}",
                existingFile
            );
        }

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


    [HttpDelete("covers/{id:guid}")]
    public async Task<IActionResult> RemoveCover(Guid id)
    {
        var coversDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "covers"
        );

        if (!Directory.Exists(coversDirectory))
        {
            return Ok(new
            {
                id,
                removed = false
            });
        }

        var files = Directory.GetFiles(
            coversDirectory,
            $"{id}.*"
        );

        foreach (var filePath in files)
        {
            System.IO.File.Delete(filePath);

            logger.LogInformation(
                "Cover supprimée : {FilePath}",
                filePath
            );
        }

        return Ok(new
        {
            id,
            removed = files.Length > 0
        });
    }
}
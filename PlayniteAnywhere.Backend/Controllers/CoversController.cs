using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayniteAnywhere.Backend.Data;

namespace PlayniteAnywhere.Backend.Controllers;

[ApiController]
[Route("api/covers")]
public class CoversController : ControllerBase
{
    private readonly PlayniteAnywhereDbContext db;

    public CoversController(PlayniteAnywhereDbContext db)
    {
        this.db = db;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCover(Guid id)
    {
        var gameExists = await db.Games
            .AsNoTracking()
            .AnyAsync(game => game.Id == id);

        if (!gameExists)
        {
            return NotFound();
        }

        var coversDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "covers"
        );

        var files = Directory.GetFiles(
            coversDirectory,
            $"{id}.*"
        );

        var filePath = files.FirstOrDefault();

        if (filePath == null)
        {
            return NotFound();
        }

        var contentType = GetContentType(
            Path.GetExtension(filePath)
        );

        return PhysicalFile(
            filePath,
            contentType
        );
    }

    private static string GetContentType(string extension)
    {
        switch (extension.ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";

            case ".png":
                return "image/png";

            case ".webp":
                return "image/webp";

            case ".gif":
                return "image/gif";

            case ".bmp":
                return "image/bmp";

            default:
                return "application/octet-stream";
        }
    }
}
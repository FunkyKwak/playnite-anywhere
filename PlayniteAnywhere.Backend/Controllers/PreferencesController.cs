using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayniteAnywhere.Backend.Data;
using PlayniteAnywhere.Backend.Models;
using System.Text.Json;

namespace PlayniteAnywhere.Backend.Controllers;

[ApiController]
[Route("api/preferences")]
public class PreferencesController : ControllerBase
{
    private readonly PlayniteAnywhereDbContext db;

    public PreferencesController(PlayniteAnywhereDbContext db)
    {
        this.db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetPreferences()
    {
        var preferences = await db.Preferences
            .FirstOrDefaultAsync();

        if (preferences == null)
        {
            preferences = new Preferences();

            db.Preferences.Add(preferences);

            await db.SaveChangesAsync();
        }

        var collapsedGroups =
            JsonSerializer.Deserialize<Dictionary<string, bool>>(
                preferences.CollapsedGroups
            ) ?? new Dictionary<string, bool>();

        return Ok(new
        {
            preferences.Id,
            preferences.GroupBy,
            CollapsedGroups = collapsedGroups
        });
    }

    [HttpPost]
    public async Task<IActionResult> SavePreferences(
        [FromBody] Preferences preferences)
    {
        var existing = await db.Preferences
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            existing = new Preferences();

            db.Preferences.Add(existing);
        }

        existing.GroupBy = preferences.GroupBy;

        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("groups")]
    public async Task<IActionResult> SaveGroupState(
        [FromBody] GroupStateRequest request)
    {
        var preferences = await db.Preferences
            .FirstOrDefaultAsync();

        if (preferences == null)
        {
            preferences = new Preferences();

            db.Preferences.Add(preferences);
        }

        var collapsedGroups =
            JsonSerializer.Deserialize<Dictionary<string, bool>>(
                preferences.CollapsedGroups
            ) ?? new Dictionary<string, bool>();

        collapsedGroups[request.Name] = request.Collapsed;

        preferences.CollapsedGroups =
            JsonSerializer.Serialize(collapsedGroups);

        await db.SaveChangesAsync();

        return Ok();
    }
}

public class GroupStateRequest
{
    public string Name { get; set; } = string.Empty;

    public bool Collapsed { get; set; }
}
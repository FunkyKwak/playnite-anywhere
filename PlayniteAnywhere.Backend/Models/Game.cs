namespace PlayniteAnywhere.Backend.Models;

public class Game
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Favorite { get; set; }
    public string? Source { get; set; }
    public string? CompletionStatus { get; set; }
    public string? Platforms { get; set; }
    public long? CoverSize { get; set; }
    public long? CoverLastWriteTimeUtcTicks { get; set; }
}
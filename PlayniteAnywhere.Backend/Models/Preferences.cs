namespace PlayniteAnywhere.Backend.Models;

public class Preferences
{
    public int Id { get; set; }

    public string GroupBy { get; set; } = "None";

    public string CollapsedGroups { get; set; } = "{}";
}
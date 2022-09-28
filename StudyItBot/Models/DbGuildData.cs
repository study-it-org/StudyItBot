namespace StudyItBot.Models;

public record DbGuildData
{
    public static string GetKey(ulong id) => $"guild:{id}";
    public ulong? ModChannel { get; set; } = null;
}
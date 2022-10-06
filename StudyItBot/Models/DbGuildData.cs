namespace StudyItBot.Models;

public record DbGuildData
{
    public static string GetKey(ulong id) => $"guild:{id}";
    public ulong? ModChannel { get; set; } = null;
    public ulong? WelcomeChannel { get; set; } = null;
    public ulong? StudentRole { get; set; } = null;
    public ulong? NormalRole { get; set; } = null;
}
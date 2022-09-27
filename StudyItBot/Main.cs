using Discord;
using Discord.WebSocket;
using NLog;

namespace StudyItBot;

public class Program
{
    private readonly DiscordSocketClient _discordSocketClient = new();
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private async Task MainAsync()
    {
        var token = Environment.GetEnvironmentVariable("token");
        if (token == null)
        {
            _logger.Fatal("No token provided in the \"token\" environment variable");
            return;
        }

        await _discordSocketClient.LoginAsync(TokenType.Bot, "");
        await Task.Delay(-1);
    }
}
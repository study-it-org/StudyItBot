using System.ComponentModel.Design;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using StudyItBot.Interactions;

namespace StudyItBot;

public class Program
{
    private DiscordSocketClient _discordSocketClient = null!;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private IServiceProvider _services = null!;

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

        var clientConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose
        };

        _discordSocketClient = new DiscordSocketClient(clientConfig);
        var interactionServiceConfig = new InteractionServiceConfig
        {
            InteractionCustomIdDelimiters = new[] { '.' },
            DefaultRunMode = RunMode.Async,
            AutoServiceScopes = false,
            LogLevel = LogSeverity.Verbose,
        };

        var interactionService = new InteractionService(_discordSocketClient, interactionServiceConfig);
        interactionService.Log += OnLog;

        _services = new ServiceCollection()
            .AddSingleton(clientConfig)
            .AddSingleton(_discordSocketClient)
            .AddSingleton(interactionServiceConfig)
            .AddSingleton(interactionService).BuildServiceProvider();
        _discordSocketClient.Log += OnLog;

        await _discordSocketClient.LoginAsync(TokenType.Bot, token);
        await _discordSocketClient.StartAsync();

        _discordSocketClient.Ready += OnReady;


        await Task.Delay(-1);
    }

    private async Task OnReady()
    {
        var cmdHandler = new CommandHandler(_discordSocketClient, _services);
        await cmdHandler.InstallCommandsAsync();
    }

    private static Task OnLog(LogMessage arg)
    {
        var logger = LogManager.GetLogger(arg.Source);
        if (arg.Message != null)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    logger.Fatal(arg.Message);
                    break;
                case LogSeverity.Error:
                    logger.Error(arg.Message);
                    break;
                case LogSeverity.Warning:
                    logger.Warn(arg.Message);
                    break;
                case LogSeverity.Info:
                    logger.Info(arg.Message);
                    break;
                case LogSeverity.Verbose:
                    logger.Debug(arg.Message);
                    break;
                case LogSeverity.Debug:
                    logger.Trace(arg.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (arg.Exception != null)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    logger.Fatal(arg.Exception);
                    break;
                case LogSeverity.Error:
                    logger.Error(arg.Exception);
                    break;
                case LogSeverity.Warning:
                    logger.Warn(arg.Exception);
                    break;
                case LogSeverity.Info:
                    logger.Info(arg.Exception);
                    break;
                case LogSeverity.Verbose:
                    logger.Debug(arg.Exception);
                    break;
                case LogSeverity.Debug:
                    logger.Trace(arg.Exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return Task.CompletedTask;
    }
}
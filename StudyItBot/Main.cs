using System.ComponentModel.Design;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using StudyItBot.Interactions;
using StudyItBot.Models;
using StudyItBot.Services;
using SurrealDB.Configuration;
using SurrealDB.Driver.Rpc;
using SurrealDB.Models;

namespace StudyItBot;

public class Program
{
    private DiscordSocketClient _discordSocketClient = null!;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private IServiceProvider _services = null!;
    public static bool IsReady = false;

    public static void Main(string[] args)
    {
        LogManager.Setup().SetupExtensions(builder => builder.RegisterAssembly(Assembly.GetEntryAssembly()));
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private async Task MainAsync()
    {
        var token = Environment.GetEnvironmentVariable("bot_token");
        if (token == null)
        {
            _logger.Fatal("No token provided in the \"bot_token\" environment variable");
            return;
        }

        var emailUsername = Environment.GetEnvironmentVariable("email_username");
        if (emailUsername == null)
        {
            _logger.Fatal("No token provided in the \"email_username\" environment variable");
            return;
        }

        var emailPassword = Environment.GetEnvironmentVariable("email_password");
        if (emailPassword == null)
        {
            _logger.Fatal("No token provided in the \"email_password\" environment variable");
            return;
        }

        var emailServer = Environment.GetEnvironmentVariable("email_server");
        if (emailServer == null)
        {
            _logger.Fatal("No token provided in the \"email_server\" environment variable");
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

        var cfg = Config.Create()
            .WithEndpoint("127.0.0.1:8000")
            .WithDatabase("test")
            .WithNamespace("test")
            .WithBasicAuth("root", "root")
            .WithRpc(true).Build();

        DatabaseRpc db = new(cfg);
        await db.Open();
        _logger.Info("Successfully connected to Database.");


        var dbService = new DatabaseService(db);

        _services = new ServiceCollection()
            .AddSingleton(clientConfig)
            .AddSingleton(_discordSocketClient)
            .AddSingleton(new SwotService())
            .AddSingleton(new SmtpService(emailServer, emailUsername, emailPassword))
            .AddSingleton(dbService)
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
        IsReady = true;
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
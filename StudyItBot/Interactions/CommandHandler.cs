using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace StudyItBot.Interactions;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly InteractionService _interactionService;

    public CommandHandler(DiscordSocketClient client, IServiceProvider services)
    {
        _client = client;
        _services = services;
        _interactionService = (services.GetService(typeof(InteractionService)) as InteractionService)!;
        _interactionService.SlashCommandExecuted += PostSlashCommandExecution;

    }


    private Task PostSlashCommandExecution(SlashCommandInfo info, IInteractionContext ctx, IResult res)
    {
        //TODO: Track Statistics.
        return Task.CompletedTask;
    }

    public async Task InstallCommandsAsync()
    {
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
#if DEBUG
        await _interactionService.RegisterCommandsToGuildAsync(262984015995207681);
#else
            await _interactionService.RegisterCommandsGloballyAsync();
#endif
    }
}
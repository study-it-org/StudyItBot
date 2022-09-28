using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using StudyItBot.Models;
using StudyItBot.Services;

namespace StudyItBot.Interactions;

[UsedImplicitly]
[Group("settings", "Settings for StudyItBot")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class SettingsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DatabaseService _databaseService;

    public SettingsCommand(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [UsedImplicitly]
    [SlashCommand("modlog", "Sets the Mod-log Channel for this server.")]
    public async Task SetModChannel(SocketTextChannel textChannel)
    {
        await DeferAsync(true);
        try
        {
            await textChannel.SendMessageAsync("This channel will now be used to log moderation information.");
        }
        catch (HttpException e)
        {
            await FollowupAsync($"Failed to Send message to {textChannel.Mention} ({e.DiscordCode}).");
            return;
        }

        var guildSettings = await _databaseService.SelectGuildSettingsAsync(Context.Guild.Id) ?? new DbGuildData();
        guildSettings.ModChannel = textChannel.Id;
        await _databaseService.ChangeGuildSettingsAsync(Context.Guild.Id, guildSettings);
        await FollowupAsync($"Successfully set {textChannel.Mention} as Logging Channel.");
    }
}
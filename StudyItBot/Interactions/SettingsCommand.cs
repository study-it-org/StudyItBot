using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using StudyItBot.Models;
using StudyItBot.Services;
using StudyItBot.Utilities;

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

    [UsedImplicitly]
    [SlashCommand("welcome", "Sets the Welcome Channel for this server.")]
    public async Task SetWelcomeChannel(SocketTextChannel textChannel)
    {
        await DeferAsync(true);
        try
        {
            var msg = await textChannel.SendMessageAsync(
                $" {Context.User.Mention} This channel will now be used to ping new Users.");
            msg.DeleteAndForget(2000);
        }
        catch (HttpException e)
        {
            await FollowupAsync($"Failed to Send message to {textChannel.Mention} ({e.DiscordCode}).");
            return;
        }

        var guildSettings = await _databaseService.SelectGuildSettingsAsync(Context.Guild.Id) ?? new DbGuildData();
        guildSettings.WelcomeChannel = textChannel.Id;
        await _databaseService.ChangeGuildSettingsAsync(Context.Guild.Id, guildSettings);
        await FollowupAsync($"Successfully set {textChannel.Mention} as Welcome Channel.");
        await textChannel.SendMessageAsync(
            "To start the verification process type /signup or click the button below!",
            components: new ComponentBuilder().WithButton("Signup", "signup_button")
                .Build());
    }

    [UsedImplicitly]
    [SlashCommand("studentrole", "Sets the Welcome Channel for this server.")]
    public async Task SetStudentRole(SocketRole role)
    {
        await DeferAsync(true);

        var guildSettings = await _databaseService.SelectGuildSettingsAsync(Context.Guild.Id) ?? new DbGuildData();
        guildSettings.StudentRole = role.Id;
        await _databaseService.ChangeGuildSettingsAsync(Context.Guild.Id, guildSettings);
        await FollowupAsync($"Successfully set {role.Mention} as Student Role.");
    }
    
    [UsedImplicitly]
    [SlashCommand("normalrole", "Sets the Welcome Channel for this server.")]
    public async Task SetNormalRole(SocketRole role)
    {
        await DeferAsync(true);

        var guildSettings = await _databaseService.SelectGuildSettingsAsync(Context.Guild.Id) ?? new DbGuildData();
        guildSettings.NormalRole = role.Id;
        await _databaseService.ChangeGuildSettingsAsync(Context.Guild.Id, guildSettings);
        await FollowupAsync($"Successfully set {role.Mention} as Normal Role.");
    }
}
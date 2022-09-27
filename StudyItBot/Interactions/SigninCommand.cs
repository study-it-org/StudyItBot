using Discord.Interactions;

namespace StudyItBot.Interactions;

public class SigninCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("echo", "Echo an input")]
    public async Task Echo(string input)
    {
        await RespondAsync(input);
    }
}
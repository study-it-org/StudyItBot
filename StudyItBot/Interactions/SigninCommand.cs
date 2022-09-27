using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace StudyItBot.Interactions;

public class SignUpModal : IModal
{
    public string Title => "Signin Form";

    [InputLabel("University E-Mail")]
    [ModalTextInput("e_mail", TextInputStyle.Short, "mamu1025@h-ka.de")]
    public string Email { get; set; } = "";
}

public class SigninCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;

    public SigninCommand(DiscordSocketClient client)
    {
        _client = client;
    }

    [SlashCommand("test", "Echo an input")]
    public async Task test()
    {
        await RespondWithModalAsync<SignUpModal>("signup_modal");
    }

    [ModalInteraction("signup_modal")]
    public async Task ModalResponse(SignUpModal modal)
    {
        //TODO: Send Verification E-Mail.
        //await RespondAsync($"E-Mail: {modal.Email}");
    }
}
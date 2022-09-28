using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using NLog;
using StudyItBot.Models;
using StudyItBot.Services;

namespace StudyItBot.Interactions;

[UsedImplicitly]
public class SignUpModal : IModal
{
    public string Title => "Signin Form";

    [InputLabel("University E-Mail")]
    [ModalTextInput("e_mail", TextInputStyle.Short, "mamu1025@h-ka.de", maxLength: 75)]
    public string Email { get; set; } = "";

    [InputLabel("Firstname")]
    [ModalTextInput("first_name", TextInputStyle.Short, "Max", maxLength: 45)]
    public string FirstName { get; set; } = "";

    [InputLabel("Lastname")]
    [ModalTextInput("last_name", TextInputStyle.Short, "(Optional)", maxLength: 45)]
    [RequiredInput(false)]
    public string LastName { get; set; } = "";
}

[UsedImplicitly]
public class SigninCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DatabaseService _dbService;
    private readonly SwotService _swotService;
    private readonly SmtpService _smtpService;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public SigninCommand(DatabaseService dbService, SwotService swotService, SmtpService smtpService)
    {
        _dbService = dbService;
        _swotService = swotService;
        _smtpService = smtpService;
    }

    [UsedImplicitly]
    [SlashCommand("signup", "Starts the Verification process.")]
    [EnabledInDm(false)]
    public async Task SignupAsync()
    {
        await RespondWithModalAsync<SignUpModal>("signup_modal");
    }

    [UsedImplicitly]
    [ModalInteraction("signup_modal")]
    public async Task ModalResponse(SignUpModal modal)
    {
        await DeferAsync();
        try
        {
            await ((SocketGuildUser)Context.User).ModifyAsync(properties =>
            {
                properties.Nickname = $"{modal.FirstName} {modal.LastName}".Trim();
            });
        }
        catch (Exception e)
        {
            _logger.Warn($"Failed to change username for {Context.User.Username}");
        }

        if (!await _swotService.VerifyUniversityEmailAsync(modal.Email))
        {
            //TODO: The Entered E-Mail doesn't match with a valid university E-Mail.
            await FollowupAsync(text: "E-Mail is not a student E-Mail", ephemeral: true);
            return;
        }


        var user = await _dbService.SelectUserAsync(Context.User.Id);
        if (user != null)
        {
            //TODO: The User Already exists. Send message or ask for reverification of E-Mail.
            if (user.Verified)
            {
                await FollowupAsync(text: "You are already verified on this Discord.", ephemeral: true);
                return;
            }

            await FollowupAsync(
                text: "You are existent in the Database. Do you want to re-verify your e-mail?",
                ephemeral: true
            );
            return;
        }

        //TODO: The User doesn't exist yet. Create it. And send verification E-Mail
        user = await _dbService.CreateUserAsync(Context.User.Id, new DbUserData
        {
            Firstname = modal.FirstName,
        });

        if (user == null)
        {
            //TODO: Something went wrong.
            return;
        }

        await _smtpService.SendVerificationMailAsync(modal.Email, user.VerificationToken);
    }
}
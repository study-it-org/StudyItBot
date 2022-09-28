using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;

namespace StudyItBot.Services;

public class SmtpService
{
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly string _emailServer;
    private readonly string _emailUsername;
    private readonly string _emailPassword;
    private readonly SmtpClient _smtpClient;
    private bool _shouldReconnect = true;
    private bool _connected = false;

    public SmtpService(string emailServer, string emailUsername, string emailPassword)
    {
        _emailServer = emailServer;
        _emailUsername = emailUsername;
        _emailPassword = emailPassword;

        _smtpClient = new SmtpClient();

        _smtpClient.Disconnected += OnDisconnect;
        _smtpClient.Connected += OnConnection;
        _smtpClient.MessageSent += OnMessageSent;
        _smtpClient.Authenticated += OnAuthentication;
        ConnectAsync().GetAwaiter().GetResult();
    }

    public async Task<bool> SendVerificationMailAsync(string email, string verificationCode)
    {
        var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress("StudyItBot", "noreply-StudyIt@online.de"));
        mailMessage.To.Add(new MailboxAddress(email, email));
        mailMessage.Subject = "Please confirm your E-Mail address to access StudyIt";
        mailMessage.Body = new TextPart("plain")
        {
            Text = $"Hello your verification code is: {verificationCode}"
        };

        if (_connected)
        {
            try
            {
                await _smtpClient.SendAsync(mailMessage);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        _logger.Warn("Tryed to send E-Mail without the SMTP-Client being connected to the Server.");
        return false;
    }

    public async Task ConnectAsync()
    {
        _shouldReconnect = true;
        await _smtpClient.ConnectAsync(_emailServer, 465, true);
        await _smtpClient.AuthenticateAsync(_emailUsername, _emailPassword);
    }

    public async Task DisconnectAsync()
    {
        _shouldReconnect = false;
        await _smtpClient.DisconnectAsync(true);
    }

    private void OnAuthentication(object? sender, AuthenticatedEventArgs e)
    {
        _logger.Debug($"Successfully authenticated to SMTP-Server Msg: {e.Message}");
    }

    private void OnMessageSent(object? sender, MessageSentEventArgs e)
    {
        _logger.Debug($"Successfully send E-Mail to {e.Message.To.Mailboxes.First().Address}\n{e.Message}");
    }

    private void OnConnection(object? sender, ConnectedEventArgs e)
    {
        _logger.Info($"Successfully connected to SMTP Server {e.Host}:{e.Port}");
        _connected = true;
    }

    private void OnDisconnect(object? sender, DisconnectedEventArgs e)
    {
        _logger.Warn($"Disconnected from SMTP Server {e.Host}:{e.Port}");
        _connected = false;
        if (_shouldReconnect)
        {
            ConnectAsync().GetAwaiter().GetResult();
        }
    }
}
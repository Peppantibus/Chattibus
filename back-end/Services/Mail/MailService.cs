using Chat.Models.Dto.Auth;
using MimeKit;

namespace Chat.Services.Mail;

public class MailService : IMailService
{
    private readonly IMailTemplateService _templateService;
    private readonly IConfiguration _config;

    public MailService(IMailTemplateService templateService, IConfiguration config)
    {
        _templateService = templateService;
        _config = config;
    }

    public async Task SendVerifyEmail(string email, string username, Guid token)
    {
        string verifyUrl = $"{_config["AppUrls:FrontEnd"]}/verify-email?token={token}";

        var parameters = new Dictionary<string, string>
        {
            { "username", username },
            { "verifyUrl", verifyUrl }
        };

        var html = await _templateService.RenderTemplateAsync("VerifyEmail.html", parameters);

        var mail = new MailDto
        {
            From = _config["MailService:AppMail"]!,
            EmailTo = email,
            Subject = "Conferma la tua registrazione",
            Body = html,
            IsHtml = true
        };

        await Send(mail);
    }

    public async Task SendResetPasswordEmail(string email, string username, Guid token)
    {
        string resetUrl = $"{_config["AppUrls:FrontEnd"]}/reset-password?token={token}";

        var parameters = new Dictionary<string, string>
        {
            { "username", username },
            { "resetUrl", resetUrl }
        };

        var html = await _templateService.RenderTemplateAsync("ResetPassword.html", parameters);

        var mail = new MailDto
        {
            From = _config["MailService:AppMail"]!,
            EmailTo = email,
            Subject = "Recupera Password",
            Body = html,
            IsHtml = true
        };

        await Send(mail);
    }

    private async Task Send(MailDto mail)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Chattibus", mail.From));
        message.To.Add(new MailboxAddress("", mail.EmailTo));

        if (mail.EmailCC != null)
        {
            message.Cc.AddRange(
                mail.EmailCC
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => new MailboxAddress("", e))
            );
        }

        message.Subject = mail.Subject;

        var builder = new BodyBuilder();

        if (mail.IsHtml)
        {
            builder.HtmlBody = mail.Body;
            builder.TextBody = "Il tuo client email non supporta HTML.";
        }
        else
        {
            builder.TextBody = mail.Body;
        }

        // Corpo finale MIME
        message.Body = builder.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();

        //per produzione aggiungere authenticate e modificare il false per l'ssl
        await client.ConnectAsync(_config["MailService:Host"], int.Parse(_config["MailService:Port"]!), false);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

using Chat.Models.Dto.Auth;
using MimeKit;

namespace Chat.Services.Mail;

public class MailService : IMailService
{
    private readonly IConfiguration _config;

    public MailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(MailDto mail)
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

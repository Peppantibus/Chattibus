using Chat.Models.Dto.Auth;

namespace Chat.Services.Mail
{
    public interface IMailService
    {
        public Task SendAsync(MailDto mail);
    }
}

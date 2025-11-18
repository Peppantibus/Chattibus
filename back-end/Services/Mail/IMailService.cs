using Chat.Models.Dto.Auth;

namespace Chat.Services.Mail
{
    public interface IMailService
    {
        public Task SendVerifyEmail(string email, string username, Guid token);
        public Task SendResetPasswordEmail(string email, string username, Guid token);
    }
}

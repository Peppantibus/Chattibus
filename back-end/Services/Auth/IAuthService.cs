using Chat.Models.Dto.Auth;
using Chat.Models.Entity;

namespace Chat.Services.AuthService;

public interface IAuthService
{
    public Task<AuthResponseDto> Login(string username, string password);
    public Task AddUser(User user);
    public Task<bool> VerifyMail(Guid token);
    public Task<bool> ResetPasswordRedirect(Guid token);
    public Task<string> RecoveryPassword(string email);
    public Task<bool> ResetPassword(ResetPasswordDto body);
}

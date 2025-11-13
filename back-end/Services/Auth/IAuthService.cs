using Chat.Models.Dto;
using Chat.Models.Entity;

namespace Chat.Services.AuthService;

public interface IAuthService
{
    public Task<AuthResponseDto> Login(string username, string password);
    public Task AddUser(User user);

    public Task<string> RefreshToken(string token);
}

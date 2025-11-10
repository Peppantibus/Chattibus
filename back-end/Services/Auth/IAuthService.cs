using Chat.Models.Entity;

namespace Chat.Services.AuthService;

public interface IAuthService
{
    public Task<string> Login(string username, string password);
    public Task AddUser(User user);
}

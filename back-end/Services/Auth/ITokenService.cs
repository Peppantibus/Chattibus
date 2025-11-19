using Chat.Models.Dto.Auth;
using Chat.Models.Entity;

namespace Chat.Services.Auth;

public interface ITokenService
{
    public Task<RefreshTokenDto> RefreshToken(string token);
    public Task<RefreshToken> CreateRefreshToken(User user);
    public AccessTokenResult GenerateAccessToken(User user);
}

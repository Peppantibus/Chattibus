using Chat.Models.Entity;

namespace Chat.Models.Dto.Auth;

public class RefreshTokenDto
{
    public string NewRefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public AccessTokenResult AccessToken { get; set; } = default!;
    public UserDto User { get; set; } = default!;
}

namespace Chat.Models.Dto.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    //durate in secondi
    public int AccessExpiresIn { get; set; }
    public int RefreshExpiresIn { get; set; }

    public UserDto User { get; set; } = default!;

}

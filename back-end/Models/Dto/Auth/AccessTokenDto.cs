namespace Chat.Models.Dto.Auth;

public class AccessTokenResult
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
}
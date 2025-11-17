namespace Chat.Models.Dto.Auth;

public class ResetPasswordDto
{
    public Guid Token { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

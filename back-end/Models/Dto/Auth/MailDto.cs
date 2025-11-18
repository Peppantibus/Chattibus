namespace Chat.Models.Dto.Auth;

public class MailDto
{
    public string EmailTo { get; set; } = default!;
    public List<string>? EmailCC { get; set; } = default!;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
}

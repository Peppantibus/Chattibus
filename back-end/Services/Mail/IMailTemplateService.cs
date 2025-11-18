namespace Chat.Services.Mail;

public interface IMailTemplateService
{
    public Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> variables);
}

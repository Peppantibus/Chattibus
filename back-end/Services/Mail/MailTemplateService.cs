namespace Chat.Services.Mail;

public class MailTemplateService : IMailTemplateService
{
    private readonly IWebHostEnvironment _env;

    public MailTemplateService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> variables)
    {
        // Percorso ai template
        var path = Path.Combine(_env.ContentRootPath, "MailTemplates", templateName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Template {templateName} non trovato", path);

        string template = await File.ReadAllTextAsync(path);

        // Replace manuali
        foreach (var kvp in variables)
        {
            template = template.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        return template;
    }
}

using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Utilities;

public class JwtSettings
{
    //chiave segreta del token
    public string Key { get; set; } = string.Empty;
    //controllo in più per dire accetta solo i token per quest'app
    public string Issuer { get; set; } = string.Empty;
    //ulteriore controllo accetta solo i token destinati a questo "pubblico"
    public string Audience { get; set; } = string.Empty;
    //tempo di durata del token in ore
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeMinutes { get; set; }

}

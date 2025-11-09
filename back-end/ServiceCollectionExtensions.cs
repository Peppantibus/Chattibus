using System.Text;
using Chat.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        //mappatura dei campi del mio secret json e classe di config
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        if (string.IsNullOrEmpty(jwtSettings?.Key))
            throw new Exception("JWT key mancante o vuota nella configurazione.");

        //inizializzo token
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Controlla se esiste un token nella query string (es. /ws?token=abc123)
                    var accessToken = context.Request.Query["token"];
                    var path = context.HttpContext.Request.Path;

                    // Lo consideriamo valido solo per le richieste WebSocket
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });



        return services;
    }
}

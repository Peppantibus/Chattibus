using System.Text;
using Chat.Data;
using Chat.Services;
using Chat.Services.Auth;
using Chat.Services.AuthService;
using Chat.Services.FriendService;
using Chat.Services.Mail;
using Chat.Services.MessageService;
using Chat.Services.MessagService;
using Chat.Services.Redis;
using Chat.Services.Security;
using Chat.Services.UserService;
using Chat.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;


public static class ServiceCollectionExtensions
{
    //configurazione JWT
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<ChatDbContext>(options =>
            options.UseSqlite(config.GetConnectionString("Default")));

        // Dependency Injection
        services.AddSingleton<WebSocketServices>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<IMailTemplateService, MailTemplateService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRateLimitService, RateLimitService>();
        //redis
        services.AddScoped<IRedisService, RedisService>();

        // Configurazioni di sicurezza
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        services.Configure<SecuritySettings>(config.GetSection("SecuritySettings"));
        //mappatura dei campi del mio secret json e classe di config
        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();

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
                    // se esiste un token nella query string (es. /ws?token=abc123)
                    var accessToken = context.Request.Query["token"];
                    var path = context.HttpContext.Request.Path;

                    //valido solo per le richieste WebSocket
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        //redis config
        string redisUrl = config.GetSection("Redis")["Url"]!;
        var mux = ConnectionMultiplexer.Connect(redisUrl);
        services.AddSingleton<IConnectionMultiplexer>(mux);


        // CORS
        var allowedOrigins = config.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Chat API",
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Inserisci il token JWT nel formato: Bearer {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        // Routing
        services.AddRouting(o =>
        {
            o.LowercaseUrls = true;
            o.LowercaseQueryStrings = true;
        });

        // Controller & accessor
        services.AddControllers();
        services.AddHttpContextAccessor();

        return services;
    }

    public static IApplicationBuilder UseDependencies(this IApplicationBuilder app)
    {
        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseWebSockets();

        return app;
    }

}

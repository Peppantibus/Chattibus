using System.Security.Cryptography;
using Chat.Data;
using Chat.Enum;
using Chat.Models.Dto;
using Chat.Models.Dto.Auth;
using Chat.Models.Entity;
using Chat.Services.Auth;
using Chat.Services.Mail;
using Chat.Services.Security;
using Chat.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Chat.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly ChatDbContext _dbContext;
    private readonly string _pepper;
    private readonly IMailService _mailService;
    private readonly IMailTemplateService _templateService;
    private readonly ITokenService _tokenService;
    private readonly IRateLimitService _rateLimitService;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ChatDbContext dbContext,  IOptions<SecuritySettings> securitySettings, IMailService mailService, ITokenService tokenService, IRateLimitService rateLimitService, IMailTemplateService templateService, IConfiguration config, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _pepper = securitySettings.Value.Pepper;
        _mailService = mailService;
        _tokenService = tokenService;
        _rateLimitService = rateLimitService;
        _templateService = templateService;
        _config = config;
        _logger = logger;
    }

    public async Task<RefreshTokenDto> Login(string username, string password)
    {
        _logger.LogInformation("Tentativo login per utente {username}", username);
        //verifico che l utente non sia bloccato
        bool isBlocked = await _rateLimitService.IsBlocked(RateLimitRequestType.Login,username);

        if (isBlocked)
        {
            _logger.LogWarning("Login bloccato per utente {username} (rate limit)", username);
            throw new UnauthorizedAccessException("utente bloccato");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
        {
            _logger.LogWarning("Login fallito: utente {username} non trovato", username);
            throw new InvalidOperationException($"Credenziali non valide");
        }
        if (!user.EmailVerified)
        {
            _logger.LogWarning("Login fallito: email non verificata per utente {username}", username);
            throw new InvalidOperationException("devi verificare prima la mail");
        }
        var salt = user.Salt;

        var storedHash = Convert.FromBase64String(user.Password);
        var testHashed = HashPassword(password, Convert.FromBase64String(salt));

        bool isValid = CryptographicOperations.FixedTimeEquals(storedHash, testHashed);

        if (!isValid) {
            _logger.LogWarning("Login fallito: password errata per utente {username}", username);
            await _rateLimitService.RegisterAttempted(RateLimitRequestType.Login, username);
            throw new InvalidOperationException("Credenziali non valide");
        }

        var accesstokenResponse = _tokenService.GenerateAccessToken(user);
        //creo record di refreshtoken per associare utente
        var refreshToken = await _tokenService.CreateRefreshToken(user);

        //cancello da redis l istanza di tentativi dopo che l utente ha fatto login
        await _rateLimitService.Reset(RateLimitRequestType.Login,username);

        _logger.LogInformation("Login riuscito per utente {username}", username);

        return new RefreshTokenDto
        {
            NewRefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            AccessToken = accesstokenResponse,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                LastName = user.LastName,
            }
        };


    }

    public async Task AddUser(User user)
    {
        bool isBlocked = await _rateLimitService.IsBlocked(RateLimitRequestType.Register, user.Email);

        if (isBlocked)
        {
            _logger.LogWarning("Registrazione bloccata per email {email}", user.Email);
            throw new UnauthorizedAccessException("utente bloccato");
        }

        var exists = await _dbContext.Users
            .AnyAsync(x =>
                x.Username.ToLower() == user.Username.ToLower() ||
                x.Email.ToLower() == user.Email.ToLower());

        if (exists)
        {
            // Tentativo fallito → incremento rate limit REGISTER
            await _rateLimitService.RegisterAttempted(RateLimitRequestType.Register, user.Email);
            _logger.LogWarning("Tentativo di registrazione con email/username già usata: {email}", user.Email);
            throw new InvalidOperationException("utente già esistente, riprova con un o altro username o email");
        }

        //calcolo salt e password, calcolata con password + salt hashata con PBKF 
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hashedPw = HashPassword(user.Password, salt);

        //riassegno password e salt prima di salvare
        user.Password = Convert.ToBase64String(hashedPw);
        user.Salt = Convert.ToBase64String(salt);

        _dbContext.Users.Add(user);

        Guid emailToken = Guid.NewGuid();

        var emailVerified = new EmailVerifiedToken
        {
            UserId = user.Id,
            Token = emailToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
        };

        _dbContext.EmailVerifiedTokens.Add(emailVerified);
        await _dbContext.SaveChangesAsync();

        await SendAuthEmail(RateLimitRequestType.VerifyEmail, user.Email, user.Username, emailToken, "VerifyEmail.html", "Verifica email", "/verify-email?token=");
        _logger.LogInformation("Registrazione completata per utente {email}", user.Email);
    }


    public async Task<string> RecoveryPassword(string email)
    {
        _logger.LogInformation("Richiesta reset password per email {email}", email);

        var existingEntry = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (existingEntry == null)
        {
            return "Se l'email è registrata, ti abbiamo inviato un link per il reset.";
        }

        Guid passwordToken = Guid.NewGuid();

        var entryPassword = new PasswordResetToken
        {
            UserId = existingEntry.Id,
            Token = passwordToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
        };

        _dbContext.PasswordResetTokens.Add(entryPassword);
        await _dbContext.SaveChangesAsync();

        await SendAuthEmail(RateLimitRequestType.ResetPassword,email, existingEntry.Username, passwordToken, "ResetPassword.html", "Recupero Password", "/reset-password?token=");

        return "Se l'email è registrata, ti abbiamo inviato un link per il reset.";
    }

    public async Task<bool> ResetPasswordRedirect(Guid token)
    {
        var entry = await _dbContext.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);

        if (entry == null)
        {
            _logger.LogWarning("ResetPassword: il token non esiste");
            return false;
        }

        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("ResetPassword: token scaduto");
            return false;
        }

        return true;
    }

    public async Task<bool> ResetPassword(ResetPasswordDto body)
    {
        if (body.Password != body.ConfirmPassword)
        {
            throw new InvalidOperationException("password e confirm password devono essere uguali");
        }

        var entry = await _dbContext.PasswordResetTokens.FirstOrDefaultAsync(x => x.Token == body.Token);
        //validation
        if (entry == null) return false;

        if(entry.ExpiresAt < DateTime.UtcNow) return false;

        var user = await _dbContext.Users.FindAsync(entry.UserId);

        if (user == null)
        {
            throw new InvalidOperationException("errore durante il recupero");
        }

        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hashedPw = HashPassword(body.Password, salt);

        user.Password = Convert.ToBase64String(hashedPw);
        user.Salt = Convert.ToBase64String(salt);
        user.PasswordUpdatedAt = DateTime.UtcNow;

        //cancello il token
        _dbContext.PasswordResetTokens.Remove(entry);

        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Password resettata per utente id {id}", user.Id);

        return true;
    }

    public async Task<bool> VerifyMail(Guid token)
    {
        var entry = await _dbContext.EmailVerifiedTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);

        if (entry == null)
        {
            _logger.LogWarning("VerifyMail: il token non esiste");
            return false;
        }

        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("VerifyMail: token scaduto");
            return false;
        }
            

        entry.User.EmailVerified = true;
        _dbContext.EmailVerifiedTokens.Remove(entry);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Email verificata con successo per utente {email}", entry.User.Email);

        return true;
    }

    private byte[] HashPassword(string password, byte[] salt, int iterations = 300000)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password + _pepper,  // la password in chiaro + il pepper (simile al salt, ma mai in chiaro nel database) per una maggiore protezione da attacchi
            salt,                // il salt generato casualmente
            iterations,          // quante volte iterare
            HashAlgorithmName.SHA256 // algoritmo interno di hashing
        );

        return pbkdf2.GetBytes(32); // lunghezza in byte del risultato (es. 256 bit)
    }

    private async Task SendAuthEmail(
     RateLimitRequestType type,
     string email,
     string username,
     Guid token,
     string templateName,
     string subject,
     string urlPath)
    {
        _logger.LogDebug("Preparazione invio email {type} a {email}", type, email);

        // 1) Hard block
        if (await _rateLimitService.IsBlocked(type, email))
        {
            _logger.LogWarning("Block RATE LIMIT {type} per email {email}", type, email);
            throw new InvalidOperationException("utente bloccato");
        }

        // 2) Cooldown
        if (await _rateLimitService.IsInCooldown(type, email))
        {
            _logger.LogWarning("Cooldown attivo per email {email} (tipo {type})", email, type);
            throw new InvalidOperationException("utente in cooldown");
        }
        // 3) Hard attempt
        bool attemptLimitReached = await _rateLimitService.RegisterAttempted(type, email);
        if (attemptLimitReached)
        {
            _logger.LogWarning("Tentativi eccessivi per {type} email {email}. Utente bloccato.", type, email);
            throw new InvalidOperationException("troppi tentativi, utente bloccato temporaneamente");
        }

        // 4) URL
        string baseUrl = _config["AppUrls:FrontEnd"]!;
        string url = $"{baseUrl}{urlPath}{token}";

        // 5) Template parameters
        var parameters = new Dictionary<string, string>
        {
            { "username", username },
            { "url", url }   
        };

        // 6) Render template
        var html = await _templateService.RenderTemplateAsync(templateName, parameters);

        // 7) Send email
        var mail = new MailDto
        {
            From = _config["MailService:AppMail"]!,
            EmailTo = email,
            Subject = subject,
            Body = html,
            IsHtml = true
        };

        await _mailService.SendAsync(mail);
        _logger.LogInformation("Email {type} inviata a {email}", type, email);

        // 8) Start cooldown
        await _rateLimitService.StartCooldown(type, email, TimeSpan.FromSeconds(60));
    }


}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Chat.Data;
using Chat.Models.Dto;
using Chat.Models.Dto.Auth;
using Chat.Models.Entity;
using Chat.Services.AuthService;
using Chat.Services.Mail;
using Chat.Services.UserService;
using Chat.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly ChatDbContext _dbContext;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly string _pepper;
    private readonly ICurrentUserService _currentUser;
    private readonly IMailService _mailService;

    public AuthService(ChatDbContext dbContext, IOptions<JwtSettings> jwtSettings, IOptions<SecuritySettings> securitySettings, ICurrentUserService currentUserService, IMailService mailService)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings;
        _pepper = securitySettings.Value.Pepper;
        _currentUser = currentUserService;
        _mailService = mailService;
    }

    public async Task<AuthResponseDto> Login(string username, string password)
    {
        var query = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (query == null)
        {
            throw new InvalidOperationException($"Credenziali non valide");
        }
        if (!query.EmailVerified)
        {
            throw new InvalidOperationException("devi verificare prima la mail");
        }
        var salt = query.Salt;

        var storedHash = Convert.FromBase64String(query.Password);
        var testHashed = HashPassword(password, Convert.FromBase64String(salt));

        bool isValid = CryptographicOperations.FixedTimeEquals(storedHash, testHashed);

        if (!isValid) {
            throw new InvalidOperationException("Credenziali non valide");
        }

        return new AuthResponseDto
        {
            AccessToken = GenerateJWT(query),
            RefreshToken = GenerateJWT(query, true),
            AccessExpiresIn = _jwtSettings.Value.AccessTokenLifetimeHours * 3600,
            RefreshExpiresIn = _jwtSettings.Value.RefreshTokenLifetimeHours * 3600,
            User = new UserDto
            {
                Id = query.Id,
                Username = username,
                Name = query.Name,
                LastName = query.LastName,
            }
        };
    }

    public async Task AddUser(User user)
    {
        var exists = await _dbContext.Users
            .AnyAsync(x =>
                x.Username.ToLower() == user.Username.ToLower() ||
                x.Email.ToLower() == user.Email.ToLower());

        if (exists)
        {
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

        await _mailService.SendVerifyEmail(user.Email, user.Username, emailToken);
    }

    //implementare refresh token
    public async Task<string> RefreshToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenValidationParameter = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Value.Issuer,
            ValidAudience = _jwtSettings.Value.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.Key))
        };
        //jwt utente
        var jwt = handler.ValidateToken(token, tokenValidationParameter, out SecurityToken validatedToken);

        var userId = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var username = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        var typeClaim = jwt.Claims.FirstOrDefault(c => c.Type == "type")?.Value;

        if (typeClaim != "refresh")
            throw new SecurityTokenException("Token non valido per refresh");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId!));

        if (user == null)
            throw new UnauthorizedAccessException("Utente non trovato o rimosso");

        //rigenero jwt con expiredinhour di 10 giorni 
        return GenerateJWT(user, true);

    }
    public async Task<string> RecoveryPassword(string email)
    {
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

        await _mailService.SendResetPasswordEmail(email, existingEntry.Username, passwordToken);

        return "Se l'email è registrata, ti abbiamo inviato un link per il reset.";
    }

    public async Task<bool> ResetPasswordRedirect(Guid token)
    {
        var entry = await _dbContext.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);

        if (entry == null)
            return false;

        if (entry.ExpiresAt < DateTime.UtcNow)
            return false;

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

        return true;
    }

    public async Task<bool> VerifyMail(Guid token)
    {
        var entry = await _dbContext.EmailVerifiedTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);

        if (entry == null)
            return false;

        if (entry.ExpiresAt < DateTime.UtcNow)
            return false;

        entry.User.EmailVerified = true;
        _dbContext.EmailVerifiedTokens.Remove(entry);

        await _dbContext.SaveChangesAsync();

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

    private string GenerateJWT(User user, bool isRefresh = false)
    {
        // 1️ Chiave di firma
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2️ Claims (dati del token) il FE decryptando il jwt riceverà i dati scritti nel claims
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim("email", user.Email),
            !isRefresh ? new Claim("type", "access") : new Claim("type", "refresh")
        };

        int expiresInHour = !isRefresh ? _jwtSettings.Value.AccessTokenLifetimeHours : _jwtSettings.Value.RefreshTokenLifetimeHours;

        // 3️ Creazione della configurazione del token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Value.Issuer,
            audience: _jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiresInHour),
            signingCredentials: creds
        );

        // 4️ Creazione token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}

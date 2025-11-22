using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Chat.Data;
using Chat.Models.Dto;
using Chat.Models.Dto.Auth;
using Chat.Models.Entity;
using Chat.Utilities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Services.Auth;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;
    private readonly ILogger<TokenService> _logger;
    private readonly ChatDbContext _dbContext;

    public TokenService (IOptions<JwtSettings> jwtSettings, ILogger<TokenService> logger, ChatDbContext dbContext)
    {
        _jwt = jwtSettings.Value;
        _logger = logger;
        _dbContext = dbContext;
    }

    //implementare refresh token
    public async Task<RefreshTokenDto> RefreshToken(string token)
    {
        var result = await ValidateRefreshToken(token);
        var userId = result.UserId;
        var queryUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);

        if (queryUser == null)
        {
            throw new InvalidOperationException("nessun utente trovato");
        }

        var accessToken = GenerateAccessToken(queryUser);

        return new RefreshTokenDto
        {
            NewRefreshToken = result.Token,
            RefreshTokenExpiresAt = result.ExpiresAt,
            AccessToken = accessToken,
            User = new UserDto
            {
                Id = queryUser.Id,
                Username = queryUser.Username,
                Name = queryUser.Name,
                LastName = queryUser.LastName,
            }
        };

    }

    public string GenerateRefreshToken() 
    { 
        var randomNumber = RandomNumberGenerator.GetBytes(64); 
        return Convert.ToBase64String(randomNumber); 
    }

    public async Task<RefreshToken> CreateRefreshToken(User user)
    {
        var token = GenerateRefreshToken();

        var entity = new RefreshToken
        {
            UserId = user.Id,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RevokedAt = null,
            ReplacedByToken = null
        };

        _dbContext.RefreshTokens.Add(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public AccessTokenResult GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim("email", user.Email),
            new Claim("type", "access"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenLifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new AccessTokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresInSeconds = _jwt.AccessTokenLifetimeMinutes * 60
        };
    }

    private async Task<RefreshToken> ValidateRefreshToken(string token)
    {
        var existingEntry = await _dbContext.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == token);
        
        if (existingEntry == null)
        {
            throw new InvalidOperationException("token inesistente");
        }

        if (existingEntry.RevokedAt != null)
        {
            throw new InvalidOperationException("token revocato prima");
        }

        if (existingEntry.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("token scaduto");
        }

        if (existingEntry.ReplacedByToken != null)
        {
            var tokenCompromessi = _dbContext.RefreshTokens.Where(x => x.UserId == existingEntry.UserId);
            _dbContext.RemoveRange(tokenCompromessi);
            await _dbContext.SaveChangesAsync();
            _logger.LogWarning("refresh token reuse rilevato: sessione invalidata");
            throw new InvalidOperationException("refresh token reuse rilevato: sessione invalidata");
        }

        //qua la validazione dovrebbe essere corretta quindi genero nuovo token per rotation
        return await UpdateRefreshToken(existingEntry);
    }

    private async Task<RefreshToken> UpdateRefreshToken(RefreshToken oldEntity)
    {
        string newToken = GenerateRefreshToken();

        if (oldEntity == null) { throw new InvalidOperationException("token non trovato"); }
            
        oldEntity.RevokedAt = DateTime.UtcNow;
        oldEntity.ReplacedByToken = newToken;

        var newEntity = new RefreshToken
        {
            UserId = oldEntity.UserId,
            Token = newToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _dbContext.Add(newEntity);
        await _dbContext.SaveChangesAsync();

        return newEntity;
    }
}

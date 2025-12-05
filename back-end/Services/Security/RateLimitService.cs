using Chat.Enum;
using Chat.Services.Redis;
using Chat.Utilities;
using StackExchange.Redis;

namespace Chat.Services.Security;

public class RateLimitService : IRateLimitService
{
    private readonly IRedisService _redisService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Dictionary<RateLimitRequestType, RateLimitConfiguration> _config = BuildConfig();

    public RateLimitService(IRedisService redisService, IHttpContextAccessor contextAccessor)
    {
        _redisService = redisService;
        _contextAccessor = contextAccessor;
    }

    public async Task<bool> IsBlocked(RateLimitRequestType type, string identifier)
    {
        var ip = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        string ipLockKey = $"rl:lock:{type}:ip:{ip}";
        string userLockKey = $"rl:lock:{type}:{identifier}";

        // controlla se esiste un blocco temporaneo
        var ipBlocked = await _redisService.GetValue(ipLockKey) != null;
        var userBlocked = await _redisService.GetValue(userLockKey) != null;

        return ipBlocked || userBlocked;
    }

    public async Task<bool> RegisterAttempted(RateLimitRequestType type, string idenfier)
    {
        _config.TryGetValue(type, out var configuration);

        if (configuration == null) {
            throw new InvalidOperationException("enum non registrato");
        }

        var ip = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        string ipAttemptKey = $"rl:attempt:{type}:ip:{ip}";
        string identifierAttemptKey = $"rl:attempt:{type}:{idenfier}";

        //incremento di 1 per ogni tentativo
        var ipAttempts = await _redisService.Increment(ipAttemptKey, 1);
        var identifierAttempts = await _redisService.Increment(identifierAttemptKey, 1);

        //verifico tutti i tentativi nell arco di 15 minuti
        var expireIp = await _redisService.Expire(ipAttemptKey, configuration.AttemptWindow);
        var identifierUsername = await _redisService.Expire(identifierAttemptKey, configuration.AttemptWindow);

        if (ipAttempts > configuration.MaxIpAttempts)
        {
            //blocco ip per X minuti in base alla configurazione
            await _redisService.SetValue($"rl:lock:{type}:ip:{ip}", "1", configuration.LockDuration);
            return true;
        }

        if (identifierAttempts > configuration.MaxUserAttempts)
        {
            //blocco utente per X minuti in base alla configurazione
            await _redisService.SetValue($"rl:lock:{type}:{idenfier}", "1", configuration.LockDuration);
            return true;
        }

        return false;
    }

    public async Task Reset(RateLimitRequestType type, string identifier)
    {
        var ip = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        await _redisService.Remove($"rl:attempt:{type}:ip:{ip}");
        await _redisService.Remove($"rl:attempt:{type}:{identifier}");
    }

    public async Task<bool> IsInCooldown(RateLimitRequestType type, string identifier)
    {
        string key = $"rl:cooldown:{type}:{identifier}";
        return await _redisService.GetValue(key) != null;
    }

    public async Task StartCooldown(RateLimitRequestType type, string identifier, TimeSpan duration)
    {
        string key = $"rl:cooldown:{type}:{identifier}";
        await _redisService.SetValue(key, "1", duration);
    }

    private static Dictionary<RateLimitRequestType, RateLimitConfiguration> BuildConfig()
    {
        return new()
    {
        {
            RateLimitRequestType.Login,
            new RateLimitConfiguration
            {
                MaxUserAttempts = 5,
                MaxIpAttempts = 20,
                AttemptWindow = TimeSpan.FromMinutes(15),
                LockDuration = TimeSpan.FromMinutes(5)
            }
        },
        {
            RateLimitRequestType.Register,
            new RateLimitConfiguration
            {
                MaxUserAttempts = 3,
                MaxIpAttempts = 10,
                AttemptWindow = TimeSpan.FromMinutes(30),
                LockDuration = TimeSpan.FromMinutes(10)
            }
        },
        {
            RateLimitRequestType.VerifyEmail,
            new RateLimitConfiguration
            {
                MaxUserAttempts = 5,
                MaxIpAttempts = 15,
                AttemptWindow = TimeSpan.FromHours(1),
                LockDuration = TimeSpan.FromMinutes(15)
            }
        },
        {
            RateLimitRequestType.ResetPassword,
            new RateLimitConfiguration
            {
                MaxUserAttempts = 3,
                MaxIpAttempts = 10,
                AttemptWindow = TimeSpan.FromMinutes(30),
                LockDuration = TimeSpan.FromMinutes(15)
            }
        }
    };
    }


}

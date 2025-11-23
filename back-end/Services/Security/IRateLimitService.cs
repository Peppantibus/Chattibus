using Chat.Enum;

namespace Chat.Services.Security;

public interface IRateLimitService
{
    public Task<bool> IsBlocked(RateLimitRequestType type, string identifier);
    public Task<bool> RegisterAttempted(RateLimitRequestType type, string identifier);
    public Task Reset(RateLimitRequestType type, string identifier);
    public Task<bool> IsInCooldown(RateLimitRequestType type, string identifier);
    public Task StartCooldown(RateLimitRequestType type, string identifier, TimeSpan duration);
}

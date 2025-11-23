using Chat.Enum;

namespace Chat.Utilities;

/// <summary>
/// configurazione per usare il rate limit usando redis per piu funzioni es. register, login, verifyemail ecc..
/// </summary>
public class RateLimitConfiguration
{
    public int MaxUserAttempts { get; set; }
    public int MaxIpAttempts { get; set; }
    public TimeSpan AttemptWindow { get; set; }
    public TimeSpan LockDuration { get; set; }

}

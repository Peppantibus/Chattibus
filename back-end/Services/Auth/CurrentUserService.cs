using System.Security.Claims;

namespace Chat.Services.CurrentUser;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string? Username { get; }
}
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim.Value) : throw new UnauthorizedAccessException("User not authenticated.");
        }
    }

    public string? Username =>
        _httpContextAccessor.HttpContext?.User?.Identity?.Name;
}

using System.Net.WebSockets;

namespace Chat.Utilities;

public sealed class ClientConnection
{
    public Guid ConnectionId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = default!;
    public WebSocket WebSocket { get; init; } = default!;
    public DateTime ConnectedAt { get; init; } = DateTime.UtcNow;
}

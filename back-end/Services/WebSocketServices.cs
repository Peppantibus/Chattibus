using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Chat.Models.Dto;
using Chat.Models.Entity;
using Chat.Services.MessagService;
using Chat.Services.UserService;
using Chat.Utilities;

namespace Chat.Services;

public class WebSocketServices
{
    private readonly ILogger<WebSocketServices> _logger;
    private readonly IServiceProvider _serviceProvider;
    //il primo guid è il mio user id, il secondo è il connection id e poi la classe per gestire la connection
    private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ClientConnection>> ConnectedUsers = new();

    public WebSocketServices(IServiceProvider serviceProvider, ILogger<WebSocketServices> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleConnection(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        using var scope = context.RequestServices.CreateScope();
        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

        var userId = currentUser.UserId; // <-- preso dai claims
        var username = currentUser.Username;

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await RegisterClientAsync(userId, username!, webSocket);
    }

    public async Task RegisterClientAsync(Guid userId, string username, WebSocket webSocket)
    {
        var connection = new ClientConnection
        {
            ConnectionId = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            WebSocket = webSocket,
        };

        /*
        * Se per userId non esiste ancora un bucket/connections, lo crea e te lo ritorna.
        * Se esiste già (magari l’utente ha un’altra tab aperta), ti ritorna quello esistente.
        * È thread-safe: due connessioni in parallelo per lo stesso utente finiscono sul medesimo bucket.
        */
        var bucket = ConnectedUsers.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, ClientConnection>());

        //ogni bucket un utente
        //Ora hai il tuo secchio di connessioni per questo utente.
        //In questo momento contiene una connessione, ma può contenerne più di una se l’utente apre più tab o dispositivi.
        bucket[connection.ConnectionId] = connection;

        await HandleMessagesAsync(connection);

    }

    public async Task HandleMessagesAsync(ClientConnection connection)
    {
        var webSocket = connection.WebSocket;
        var buffer = new byte[1024 * 4];
        try
        {


            while (webSocket.State == WebSocketState.Open)
            {
                //ottengo dal FE il messaggio
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                // esci se il client chiude la connessione
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    break;
                }
                //lo parso in JSON
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //lo converto in oggetto
                var message = JsonSerializer.Deserialize<IncomingMessageDto>(json);
                //creo l'entità per salvare il mio message nel db
                var itemToAdd = new Message
                {
                    Content = message.Content,
                    SenderId = connection.UserId,
                    ReceiverId = message.ToUserId,
                    IsRead = false,
                };

                //qui devo salvare passandomi il service
                using var scope = _serviceProvider.CreateScope();
                var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

                await messageService.AddMessage(itemToAdd);

                //se il destinatario è online invia subito il messaggio
                // se il destinatario è online, invia subito il messaggio
                if (ConnectedUsers.TryGetValue(message.ToUserId, out var bucket))
                {
                    foreach (var conn in bucket.Values.ToList())
                    {
                        if (conn.WebSocket.State == WebSocketState.Open)
                        {
                            await conn.WebSocket.SendAsync(
                                Encoding.UTF8.GetBytes(json),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None
                            );
                        }
                    }
                }

                // ✅ invia anche al mittente (così la UI si aggiorna subito)
                if (ConnectedUsers.TryGetValue(connection.UserId, out var senderBucket))
                {
                    foreach (var conn in senderBucket.Values.ToList())
                    {
                        if (conn.WebSocket.State == WebSocketState.Open)
                        {
                            await conn.WebSocket.SendAsync(
                                Encoding.UTF8.GetBytes(json),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None
                            );
                        }
                    }
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'HandleMessagesAsync per utente {UserId} ({Username})", connection.UserId, connection.Username);
        }
        finally
        {
            if (ConnectedUsers.TryGetValue(connection.UserId, out var bucket))
            {
                bucket.TryRemove(connection.ConnectionId, out _);
                if (bucket.IsEmpty)
                    ConnectedUsers.TryRemove(connection.UserId, out _);
            }

            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                _logger.LogInformation("Connessione chiusa per utente {User} ({ConnectionId})", connection.Username, connection.ConnectionId);
            }
            catch (Exception closeEx)
            {
                _logger.LogWarning(closeEx, "Socket già chiuso o errore durante la chiusura per utente {User}", connection.Username);
            }
        }



    }

}

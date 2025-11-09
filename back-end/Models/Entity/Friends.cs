using Chat.Models.Entity;
/// <summary>
/// tabella "ponte" per gestire il concetto di amicizia tra due utenti basandosi sulla tabella users, e sulla logica di friend request ---> se friend request confermata allora 
/// </summary>
public class Friend
{
    public int Id { get; set; }
 
    public Guid UserId { get; set; }         // utente principale
    public User User { get; set; } = default!;

    public Guid FriendId { get; set; }       // amico dell’utente
    public User FriendUser { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

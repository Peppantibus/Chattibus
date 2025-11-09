namespace Chat.Models.Entity
{
    /// <summary>
    /// ulteriore tabella ponte per gestire le richieste tra utenti
    /// una volta accettata la richiesta creo il record nella tabella Friends
    /// </summary>
    public class FriendRequest
    {
        public int Id { get; set; }
        public Guid SenderId { get; set; }
        public User Sender { get; set; } = default!;
        public Guid ReceiverId { get; set; }
        public User Receiver { get; set; } = default!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}

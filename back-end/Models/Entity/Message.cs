namespace Chat.Models.Entity;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public Guid ReceiverId { get; set; }
    public bool IsRead { get; set; }

    // Navigation properties
    public User Sender { get; set; } = default!;
    public User Receiver { get; set; } = default!;
}

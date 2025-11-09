namespace Chat.Models.Dto;

public class FriendDto
{
    public int Id { get; set; }
    public Guid FriendId { get; set; }       // amico dell’utente
    public string FriendUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

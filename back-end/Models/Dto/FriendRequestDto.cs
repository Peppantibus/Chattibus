namespace Chat.Models.Dto;

public class FriendRequestDto
{
    public int Id { get; set; }
    public string SenderUsername { get; set; } = default!;
    public string ReceiverUsername { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}


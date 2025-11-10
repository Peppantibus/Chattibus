using Chat.Models.Entity;

namespace Chat.Models.Dto;

public class FriendRequestDto
{
    public int Id { get; set; }
    public string SenderUsername { get; set; } = default!;
    public string ReceiverUsername { get; set; } = default!;
    public DateTime SentAt { get; set; }

    public static FriendRequestDto FromEntity(FriendRequest friendRequest)
    {
        return new FriendRequestDto
        {
            Id = friendRequest.Id,
            SenderUsername = friendRequest.Sender.Username,
            ReceiverUsername = friendRequest.Receiver.Username,
            SentAt = friendRequest.SentAt,
        };
    }
}


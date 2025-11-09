using Chat.Models.Entity;

namespace Chat.Models.Dto;

public class MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderUsername { get; set; } = string.Empty;
    public string ReceivedUsername {  get; set; } = string.Empty;   
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsMine { get; set; }
    public bool IsRead { get; set; }


    public static MessageDto FromEntity(Message message, Guid currentUserId)
    {
        return new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            SenderUsername = message.Sender.Username,
            ReceivedUsername = message.Receiver.Username,
            SentAt = message.SentAt,
            IsMine = message.SenderId == currentUserId,
            IsRead = message.IsRead
        };
    }
}

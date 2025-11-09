using Chat.Models.Dto;
using Chat.Models.Entity;

namespace Chat.Services.MessagService;

public interface IMessageService
{
    public Task<List<MessageDto>> GetAllMessages();
    public Task AddMessage(Message message);
}

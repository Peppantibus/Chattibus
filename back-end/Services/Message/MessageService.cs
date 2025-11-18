using Chat.Data;
using Chat.Models.Dto;
using Chat.Models.Entity;
using Chat.Services.MessagService;
using Chat.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace Chat.Services.MessageService;

public class MessageService : IMessageService
{
    private readonly ChatDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public MessageService(ChatDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<List<MessageDto>> GetAllMessages() 
    { 
        var query = await _dbContext.Messages.AsNoTracking().Include(m => m.Sender).Include(m => m.Receiver).ToListAsync();

        var result = query
        .Select(x => MessageDto.FromEntity(x, _currentUser.UserId))
        .ToList();

        return result;
    }
         
    public async Task AddMessage(Message message)
    {
        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();
    }


}

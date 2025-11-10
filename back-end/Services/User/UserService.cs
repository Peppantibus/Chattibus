using Chat.Data;
using Chat.Models.Dto;
using Chat.Models.Entity;
using Chat.Services.CurrentUser;
using Chat.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Chat.Services.UserService;

public class UserService : IUserService
{
    private readonly ChatDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public UserService (ChatDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<List<UserDto>> GetUsers(string username)
    {
        if (_currentUser.Username == username)
        {
            throw new InvalidOperationException("non puoi cercare te stesso");
        }

        return await _dbContext.Users.AsNoTracking()
             .Where(x => x.Username.ToLower().Contains(username.ToLower()) && x.Id != _currentUser.UserId)
             .Select(u => SimpleMapper.Map<User, UserDto>(u))
             .Take(10)
             .ToListAsync();
    }
}

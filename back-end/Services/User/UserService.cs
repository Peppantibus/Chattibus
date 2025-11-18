using Chat.Data;
using Chat.Models.Dto;
using Chat.Models.Entity;
using Chat.Services.UserService;
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

    public async Task<List<UserDto>> GetUsers(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return new List<UserDto>();
        }
        //prendo l'utente corrente tramite autenticazione jwt
        var currentUser = await _dbContext.Users
           .Include(u => u.Friends)
           .Include(u => u.FriendOf)
           .Include(u => u.SentRequests)
           .Include(u => u.ReceivedRequests)
           .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId);

        if (currentUser == null)
        {
            throw new InvalidOperationException("utente non esiste");
        }

        //esclusione anche di utenti a cui è stata già inviata una request
        var allFriendsRequestsId = currentUser.SentRequests
             .Select(r => r.ReceiverId)
             .Concat(currentUser.ReceivedRequests.Select(r => r.SenderId))
             .ToHashSet();

        //verifico che l'utente abbia amici che ha aggiunto lui e da cui è stato aggiunto
        //logica Friends --> aggiunti da me
        //logica FriendsOf --> aggiunto da X
        var allFriendIds = currentUser.Friends
            .Select(f => f.FriendId)
            .Concat(currentUser.FriendOf.Select(f => f.UserId))
            .ToHashSet();

        //qui prendo tutti gli utenti filtrati per username e dico prendi gli utenti che contengono l'username passato, non sono me stesso e non fanno già parte dei miei amici
        return await _dbContext.Users.AsNoTracking()
            .Where(u =>
                u.Username.ToLower().Contains(username.ToLower()) &&
                u.Id != _currentUser.UserId &&
                !allFriendIds.Contains(u.Id) &&
                !allFriendsRequestsId.Contains(u.Id)) 
            .Select(u => SimpleMapper.Map<User, UserDto>(u))
            .Take(10)
            .ToListAsync();
    }
}

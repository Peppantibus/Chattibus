using Chat.Data;
using Chat.Enum;
using Chat.Models.Dto;
using Chat.Models.Entity;
using Chat.Services.CurrentUser;
using Chat.Utilities;
using Microsoft.EntityFrameworkCore;
namespace Chat.Services.FriendService;

public class FriendService : IFriendService
{
    private readonly ChatDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public FriendService(ICurrentUserService currentUser, ChatDbContext dbContext)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }


    public async Task<List<FriendDto>> GetAllFriends()
    {
        var friends = await _dbContext.Friends
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.FriendUser)
            .Where(f => f.UserId == _currentUser.UserId || f.FriendId == _currentUser.UserId)
            .ToListAsync();

        var result = friends.Select(x =>
            {
                var dto = SimpleMapper.Map<Friend, FriendDto>(x);
                dto.FriendUsername = _currentUser.UserId == x.UserId ? x.FriendUser.Username : x.User.Username;
                dto.FriendId = _currentUser.UserId == x.UserId ? x.FriendUser.Id : x.User.Id;
                return dto;
            }
        ).ToList();

        return result;
    }

    public async Task DeleteFriend(int id)
    {
        var existingEntry = await _dbContext.Friends.FirstOrDefaultAsync(x => x.Id == id);
        if (existingEntry == null)
        {
            throw new InvalidOperationException($"non esiste alcuna richiesta con id:{id}");
        }

        if (existingEntry.UserId != _currentUser.UserId && existingEntry.FriendId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Non puoi eliminare gli amici di altri utenti.");

        _dbContext.Friends.Remove(existingEntry);
        await _dbContext.SaveChangesAsync();
    }

    //friend request

    public async Task<List<FriendRequestDto>> GetFriendRequests(FriendRequestType type)
    {
        var query = await _dbContext.FriendRequests
            .AsNoTracking()
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Where(r => type == FriendRequestType.Sent
                ? r.SenderId == _currentUser.UserId
                : r.ReceiverId == _currentUser.UserId)
            .ToListAsync();

        var result = query
        .Select(x => FriendRequestDto.FromEntity(x))
        .ToList();

        return result;
    }

    public async Task Send(string username)
    {
        var receiver = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (receiver == null)
            throw new InvalidOperationException($"Utente non trovato: {username}");

        if (_currentUser.UserId == receiver.Id)
            throw new InvalidOperationException("Non puoi inviare una richiesta a te stesso.");

        //verifico se esiste già una richiesta tra i due in modo da bloccare l'invio
        bool alreadyExists = await _dbContext.FriendRequests.AnyAsync(fr =>
            (fr.SenderId == _currentUser.UserId && fr.ReceiverId == receiver.Id) ||
            (fr.SenderId == receiver.Id && fr.ReceiverId == _currentUser.UserId));

        if (alreadyExists)
            throw new InvalidOperationException("Esiste già una richiesta pendente tra questi utenti.");

        var friendRequest = new FriendRequest
        {
            SenderId = _currentUser.UserId,
            ReceiverId = receiver.Id
        };

        _dbContext.FriendRequests.Add(friendRequest);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Accept(int id)
    {
        //l'utente ha cliccato il bottone accetta
        //il FE ha l'id perchè glielo passo con la get delle richieste
        //eseguo delete sul db della friendRequest
        //creo il nuovo record nella tabella friends

        var existingEntry = await _dbContext.FriendRequests.FirstOrDefaultAsync(x =>x.Id == id);
        if (existingEntry == null)
        {
            throw new InvalidOperationException($"non esiste alcuna richiesta con id:{id}");
        }
        //verifico che solo il ricevente possa accettare la richiesta
        if (existingEntry.ReceiverId != _currentUser.UserId)
        {
            throw new UnauthorizedAccessException("Non puoi accettare richieste di altri utenti.");
        }
  
        //creo la vera relazione di amicizia tra i due utenti
        Friend friendToAdd = new Friend()
        {
            UserId = existingEntry.SenderId,
            FriendId = existingEntry.ReceiverId,
        };
        //aggiungo all'istanza del db
        _dbContext.Friends.Add(friendToAdd);

        //dopo essermi assicurato la creazione della relazione elimino la richiesta
        _dbContext.FriendRequests.Remove(existingEntry);
        await _dbContext.SaveChangesAsync();
    }   

    public async Task DeleteFriendRequest(int id)
    {
        var existingEntry = await _dbContext.FriendRequests.FirstOrDefaultAsync(x => x.Id == id);

        if (existingEntry == null)
        {
            throw new InvalidOperationException($"non esiste alcuna richiesta con id:{id}");
        }

        if (existingEntry.SenderId != _currentUser.UserId && existingEntry.ReceiverId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Non puoi eliminare richieste di altri utenti.");

        _dbContext.FriendRequests.Remove(existingEntry);
        await _dbContext.SaveChangesAsync();
    }
}

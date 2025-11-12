using Chat.Models.Dto;

namespace Chat.Services.UserService;

public interface IUserService
{
    public Task<List<UserDto>> GetUsers(string? username);
}

using Chat.Enum;
using Chat.Models.Dto;
using Chat.Models.Entity;

namespace Chat.Services.FriendService
{
    public interface IFriendService
    {
        public Task<List<FriendDto>> GetAllFriends();
        public Task DeleteFriend(int id);
        //friend request
        public Task<List<FriendRequest>> GetFriendRequests(FriendRequestType type);
        public Task Send(string username);
        public Task Accept(int id);
        public Task DeleteFriendRequest(int id);
    }
}

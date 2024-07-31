using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;

namespace FlightSearch.Interfaces
{
    public interface IFriendRepository
    {
        Task<UserFriendRequest?> SendFriendRequest(string user1, string user2);
        Task<UserFriendRequest?> CheckFriendRequestExists(string user1, string user2);
        Task<UserFriendRequest?> AcceptOrRejectFriendRequest(string user1, string user2, bool AcceptAndRejectRequest);
        Task<List<OutUserDTO>> SearchUsers(string searchTerm);
    }
}
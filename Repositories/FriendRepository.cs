using FlightSearch.Database;
using FlightSearch.Database.Models;
using FlightSearch.Enums;
using FlightSearch.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightSearch.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly ApplicationDbContext _context;
        public FriendRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserFriendRequest?> SendFriendRequest(string user1, string user2)
        {
            var friendRequestExists = await CheckFriendRequestExists(user1, user2);
            if (friendRequestExists != null)
            {
                return null;
            }
            var friendRequest = new UserFriendRequest{
                UserId1 = user1,
                UserId2 = user2,
                FriendsStatus = FriendsStatus.RequestSent
            };
            await _context.UserFriendRequests.AddAsync(friendRequest);
            await _context.SaveChangesAsync();
            return friendRequest;
        }

        public async Task<UserFriendRequest?> CheckFriendRequestExists(string user1, string user2)
        {
            var checkedFriendRequest = await _context.UserFriendRequests.
                Where(friendRequest => (friendRequest.UserId1 == user1 && friendRequest.UserId2 == user2) || (friendRequest.UserId1 == user2 && friendRequest.UserId2 == user1)).FirstOrDefaultAsync();
            return checkedFriendRequest;
        }

        public async Task<UserFriendRequest?> AcceptOrRejectFriendRequest(string user1, string user2, bool AcceptAndRejectRequest)
        {
            var friendRequest = await CheckFriendRequestExists(user1, user2);
            if (friendRequest == null || friendRequest.FriendsStatus == FriendsStatus.Friends)
            {
                return null;
            }
            friendRequest.FriendsStatus = AcceptAndRejectRequest ? FriendsStatus.Friends : FriendsStatus.ReqiestRejected;
            await _context.SaveChangesAsync();
            return friendRequest;
        }
    }
}
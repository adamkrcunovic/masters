using FlightSearch.Database;
using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;
using FlightSearch.Enums;
using FlightSearch.Helpers;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;
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

        public async Task<List<OutUserDTO>> SearchUsers(string searchTerm)
        {
            var dbUsers = await _context.Users.Include(user => user.Country).ToListAsync();
            dbUsers = dbUsers.Where(user => 
                ((double)LevenshteinHelper.Calculate(searchTerm, user.Name + " " + user.LastName) / (user.Name + " " + user.LastName).Length < 0.15) ||
                ((double)LevenshteinHelper.Calculate(searchTerm, user.LastName + " " + user.Name) / (user.LastName + " " + user.Name).Length < 0.15) ||
                (user.Name.ToLower() + " " + user.LastName.ToLower()).Contains(searchTerm.ToLower()) ||
                (user.LastName.ToLower() + " " + user.Name.ToLower()).Contains(searchTerm.ToLower())).ToList();
            var foundUsers = new List<OutUserDTO>();
            if (dbUsers != null)
            {
                foreach(var dbUser in dbUsers)
                {
                    foundUsers.Add(UserMapper.DbUserToOutUser(dbUser));
                }
            }
            return foundUsers;
        }

        public async Task<User?> getFriend(string userId)
        {
            return await _context.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();
        }
    }
}
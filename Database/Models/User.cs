using Microsoft.AspNetCore.Identity;

namespace FlightSearch.Database.Models
{
    public class User : IdentityUser
    {
        public List<UserFriendRequest> SentFriendRequests { get; set; } = new List<UserFriendRequest>();
        public List<UserFriendRequest> ReceivedFriendRequests { get; set; } = new List<UserFriendRequest>();
    }
}
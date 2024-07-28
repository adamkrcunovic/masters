using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FlightSearch.Database.Models
{
    public class User : IdentityUser
    {
        public List<UserFriendRequest> SentFriendRequests { get; set; } = new List<UserFriendRequest>();
        public List<UserFriendRequest> ReceivedFriendRequests { get; set; } = new List<UserFriendRequest>();
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public DateOnly Birthday { get; set; }
        public string Preferences { get; set; } = string.Empty;
        [Required]
        public int? CountryId { get; set; }
        public Country? Country { get; set; }
    }
}
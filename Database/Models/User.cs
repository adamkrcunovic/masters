using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FlightSearch.Database.Models
{
    public class User : IdentityUser
    {
        public List<UserFriendRequest> SentFriendRequests { get; set; } = new List<UserFriendRequest>();
        public List<UserFriendRequest> ReceivedFriendRequests { get; set; } = new List<UserFriendRequest>();
        public List<ItineraryMember> ReceivedItineraryRequests { get; set; } = new List<ItineraryMember>();
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public DateOnly Birthday { get; set; }
        public string Preferences { get; set; } = string.Empty; //preferences separated by ; and with - for those with scale
        [Required]
        public int? CountryId { get; set; }
        public Country? Country { get; set; }
        public List<Itinerary> Itineraries { get; set; } = new List<Itinerary>();
        public string DeviceIds { get; set; } = string.Empty; //separated by ;
    }
}
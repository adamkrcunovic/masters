using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FlightSearch.Enums;

namespace FlightSearch.Database.Models
{
    public class UserFriendRequest
    {
        public int Id { get; set; }
        public string? UserId1 { get; set; }
        public User? User1 { get; set; }
        public string? UserId2 { get; set; }
        public User? User2 { get; set; }
        public FriendsStatus FriendsStatus { get; set; } 
    }
}
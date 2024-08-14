using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;

namespace FlightSearch.Mappers
{
    public static class UserMapper
    {
        public static OutUserDTO DbUserToOutUser(this User user)
        {
            Console.WriteLine(user.Name);
            var friendsFromSentRequests = user.SentFriendRequests.Where(friendRequest => friendRequest.FriendsStatus == Enums.FriendsStatus.Friends).Select(friendRequest => friendRequest.User2).ToList();
            var friendsFromReceivedRequests = user.ReceivedFriendRequests.Where(friendRequest => friendRequest.FriendsStatus == Enums.FriendsStatus.Friends).Select(friendRequest => friendRequest.User1).ToList();
            var friendsFinal = friendsFromSentRequests.Concat(friendsFromReceivedRequests).Select(user => user.DbUserToOutUserWithoutRequests()).ToList();
            var pendingUsers = user.SentFriendRequests.Where(friendRequest => friendRequest.FriendsStatus != Enums.FriendsStatus.Friends).Select(friendRequest => friendRequest.User2.DbUserToOutUserWithoutRequests()).ToList();
            var requests = user.ReceivedFriendRequests.Where(friendRequest => friendRequest.FriendsStatus == Enums.FriendsStatus.RequestSent).Select(friendRequest => friendRequest.User1.DbUserToOutUserWithoutRequests()).ToList();
            Console.WriteLine(pendingUsers.Count);
            Console.WriteLine(requests.Count);
            return new OutUserDTO{
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Birthday = user.Birthday,
                Country = user.Country != null ? user.Country.CountryName : "",
                Preferences = user.Preferences,
                Friends = friendsFinal,
                Pending = pendingUsers,
                Requests = requests
            };
        }

        public static OutUserDTO DbUserToOutUserWithoutRequests(this User user)
        {
            return new OutUserDTO{
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Birthday = user.Birthday,
                Country = user.Country != null ? user.Country.CountryName : "",
                Preferences = user.Preferences
            };
        }

        public static User InRegisterUserToDbUser(this InRegisterDTO inRegisterDTO)
        {
            return new User {
                Email = inRegisterDTO.Email,
                UserName = inRegisterDTO.Email.ToUpper(),
                Name = inRegisterDTO.Name,
                LastName = inRegisterDTO.LastName,
                Birthday = inRegisterDTO.Birthday,
                Preferences = inRegisterDTO.Preferences,
                CountryId = inRegisterDTO.CountryId,
                DeviceIds = inRegisterDTO.DeviceId + ";"
            };
        }
    }
}
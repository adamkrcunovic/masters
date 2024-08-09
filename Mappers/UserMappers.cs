using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;

namespace FlightSearch.Mappers
{
    public static class UserMapper
    {
        public static OutUserDTO DbUserToOutUser(this User user)
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
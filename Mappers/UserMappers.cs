using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;

namespace FlightSearch.Mappers
{
    public static class UserMapper
    {
        public static OutUserDTO DbUserToOutUser(this User user)
        {
            return new OutUserDTO{
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName
            };
        }
    }
}
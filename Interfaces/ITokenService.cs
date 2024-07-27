using FlightSearch.Database.Models;

namespace FlightSearch.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
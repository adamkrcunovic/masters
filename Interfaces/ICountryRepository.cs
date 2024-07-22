using FlightSearch.Database.Models;

namespace FlightSearch.Interfaces
{
    public interface ICountryRepository
    {
        Task<List<Country>> GetAllCountries();
        Task<Country?> GetCountryById(int id);
        Task<Country> CreateCountry(string countryName);
        Task<Country?> UpdateCountry(int id, string countryName);
        Task<Country?> DeleteCountry(int id);
    }
}
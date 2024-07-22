using FlightSearch.Database;
using FlightSearch.Database.Models;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FlightSearch.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly ApplicationDbContext _context;

        public CountryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Country> CreateCountry(string countryName)
        {
            var country = countryName.ToCountryFromCountryName();
            await _context.Countries.AddAsync(country);
            await _context.SaveChangesAsync();
            return country;
        }

        public async Task<Country?> DeleteCountry(int id)
        {
            var country = await GetCountryById(id);
            if (country == null)
            {
                return null;
            }
            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();
            return country;
        }

        public Task<List<Country>> GetAllCountries()
        {
            return _context.Countries.Include(country => country.PublicHolidays).ToListAsync();
        }

        public  async Task<Country?> GetCountryById(int id)
        {
            return await _context.Countries.Include(country => country.PublicHolidays).FirstOrDefaultAsync(country => country.Id == id);
        }

        public async Task<Country?> UpdateCountry(int id, string countryName)
        {
            var country = await GetCountryById(id);
            if (country == null)
            {
                return null;
            }
            country.CountryName = countryName;
            await _context.SaveChangesAsync();
            return country;
        }
    }
}
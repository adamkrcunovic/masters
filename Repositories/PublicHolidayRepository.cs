using FlightSearch.Database;
using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;

namespace FlightSearch.Repositories
{
    public class PublicHolidayRepository : IPublicHolidayRepository
    {
        private readonly ApplicationDbContext _context;

        public PublicHolidayRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PublicHoliday> CreatePublicHoliday(InPublicHolidayDTO inPublicHolidayDTO)
        {
            var publicHoliday = inPublicHolidayDTO.ToPublicHolidayFromInDTO();
            await _context.PublicHolidays.AddAsync(publicHoliday);
            await _context.SaveChangesAsync();
            return publicHoliday;
        }
    }
}
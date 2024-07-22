using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;

namespace FlightSearch.Interfaces
{
    public interface IPublicHolidayRepository
    {
        Task<PublicHoliday> CreatePublicHoliday(InPublicHolidayDTO inPublicHoliday);
    }
}
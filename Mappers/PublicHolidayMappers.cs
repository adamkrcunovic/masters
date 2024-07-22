using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;

namespace FlightSearch.Mappers
{
    public static class PublicHolidayMapper
    {
        public static PublicHoliday ToPublicHolidayFromInDTO(this InPublicHolidayDTO inPublicHolidayDTO)
        {
            return new PublicHoliday
            {
                CountryId = inPublicHolidayDTO.CountryId,
                HolidayDate = inPublicHolidayDTO.HolidayDate
            };
        }
    }
}
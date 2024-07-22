using FlightSearch.Database.Models;
using FlightSearch.DTOs.OutModels;

namespace FlightSearch.Mappers
{
    public static class CountryMapper
    {
        public static OutCountryDTO ToOutCountryDTO(this Country country)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly todayYearAfter = today.AddYears(1);
            List<DateOnly> publicHolidayList = country.PublicHolidays.Select(publicHoliday => publicHoliday.HolidayDate).Where(publicHoliday => publicHoliday.CompareTo(today) > 0 && publicHoliday.CompareTo(todayYearAfter) < 0).ToList();
            publicHolidayList.Sort((x, y) => x.CompareTo(y));
            return new OutCountryDTO
            {
                Id = country.Id,
                CountryName = country.CountryName,
                PublicHolidays = publicHolidayList
            };
        }

        public static Country ToCountryFromCountryName(this string countryName)
        {
            return new Country
            {
                CountryName = countryName
            };
        }
    }
}
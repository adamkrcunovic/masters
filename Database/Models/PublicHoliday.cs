using System.ComponentModel.DataAnnotations;

namespace FlightSearch.Database.Models
{
    public class PublicHoliday
    {
        public int Id { get; set; }
        public DateOnly HolidayDate { get; set; }
        public int? CountryId { get; set; }
        public Country? Country { get; set; }
    }
}
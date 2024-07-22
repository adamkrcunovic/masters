using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FlightSearch.Database.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public List<PublicHoliday> PublicHolidays { get; set; } = new List<PublicHoliday>();
    }
}
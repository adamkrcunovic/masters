using System.ComponentModel.DataAnnotations;

namespace FlightSearch.DTOs.InModels
{
    public class InEditPersonalDataDTO
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateOnly? Birthday { get; set; }
        public int? CountryId { get; set; }
        public string? Preferences { get; set; }
    }
}
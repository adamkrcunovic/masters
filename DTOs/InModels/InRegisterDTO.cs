using System.ComponentModel.DataAnnotations;

namespace FlightSearch.DTOs.InModels
{
    public class InRegisterDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public DateOnly Birthday { get; set; }
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        [Required]
        public int CountryId;
        [Required]
        public string Preferences { get; set; } = string.Empty;

    }
}
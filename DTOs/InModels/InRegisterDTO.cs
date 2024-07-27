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

    }
}
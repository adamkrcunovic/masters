using System.ComponentModel.DataAnnotations;
using FlightSearch.Enums;

namespace FlightSearch.DTOs.InModels
{
    public class InFlightSearchDTO
    {
        [Required]
        public FlightSearchType FlightSearchType { get; set; }
        public DateOnly? DepartureDay { get; set; }
        public DateOnly? ReturnDay { get; set; }
        [Required]
        [Range(1, 9)]
        public int Adults { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? TripDuration { get; set; }
        [Required]
        public string FromAirport { get; set; } = string.Empty;
        public string? MultiCity1 { get; set; }
        public string? MultiCity2 { get; set; }
        [Required]
        public string ToAirport { get; set; } = string.Empty;
        public bool? MixMultiCity { get; set; }
        public bool FlyTheNightBefore { get; set; } = false;
        public string? IncludedAirlineCodes { get; set; }
    
        public bool MulticityRightfullyDefined() {
            bool MultiCity1Defined = MultiCity1 != null && MultiCity1.Length > 0;
            bool MultiCity2Defined = MultiCity2 != null && MultiCity2.Length > 0;
            return MultiCity1Defined == MultiCity2Defined;
        }

        public bool MulticityDefined() {
            return MultiCity1 != null && MultiCity1.Length > 0 && MultiCity2 != null && MultiCity2.Length > 0;
        }
        
    }
}
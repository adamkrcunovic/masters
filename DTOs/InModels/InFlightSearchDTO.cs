using FlightSearch.Enums;

namespace FlightSearch.DTOs.InModels
{
    public class InFlightSearchDTO
    {
        public FlightSearchType FlightSearchType { get; set; }
        public DateOnly? DepartureDay { get; set; }
        public DateOnly? ReturnDay { get; set; }
        public int Adults { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? TripDuration { get; set; }
        public string FromAirport { get; set; } = string.Empty;
        public string? MultiCity1 { get; set; } = string.Empty;
        public string? MultiCity2 { get; set; } = string.Empty;
        public string ToAirport { get; set; } = string.Empty;
        public bool OneWay { get; set; } = false;
        public bool? MixMultiCity { get; set; }
        public bool FlyTheNightBefore { get; set; } = false;
    }
}
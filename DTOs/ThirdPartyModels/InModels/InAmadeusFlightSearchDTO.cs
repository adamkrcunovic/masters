using System.ComponentModel.DataAnnotations;

namespace FlightSearch.DTOs.ThirdPartyModels.InModels
{
    public class InAmadeusFlightSearchDTO
    {
        public string OriginLocationCode { get; set; } = string.Empty;
        public string DestinationLocationCode { get; set; } = string.Empty;
        public DateOnly DepartureDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public int Adults { get; set; }
        public string CurrencyCode { get; set; } = "EUR";
        public int Max { get; set; } = 250;
        public string? IncludedAirlineCodes { get; set; }
    }
}
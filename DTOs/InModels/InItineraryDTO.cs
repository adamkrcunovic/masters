using FlightSearch.DTOs.OutModels;
using FlightSearch.Enums;

namespace FlightSearch.DTOs.InModels
{
    public class InItineraryDTO
    {
        public string ItineraryName { get; set; } = string.Empty;
        public int Adults { get; set; }
        public OutFlightDealDTO OutFlightDealDTO { get; set; } = new OutFlightDealDTO();
        public string? ChatGPTGeneratedText { get; set; }
        public PriceChangeNotificationType PriceChangeNotificationType { get; set; }
        public int? Percentage { get; set; }
        public int? Amount { get; set; }
    }
}
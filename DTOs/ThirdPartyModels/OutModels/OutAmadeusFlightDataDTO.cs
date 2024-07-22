namespace FlightSearch.DTOs.ThirdPartyModels.OutModels
{
    public class OutAmadeusFlightDataDTO
    {
        public List<OutAmadeusFlightItineraryDTO> Itineraries { get; set; } = new List<OutAmadeusFlightItineraryDTO>();
        public OutAmadeusFlightPriceDTO Price { get; set; } = new OutAmadeusFlightPriceDTO();
    }
}
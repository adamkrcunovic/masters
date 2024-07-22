namespace FlightSearch.DTOs.ThirdPartyModels.OutModels
{
    public class OutAmadeusFlightItineraryDTO
    {
        public string Duration { get; set; } = string.Empty;
        public List<OutAmadeusFlightSegmentDTO> Segments { get; set; } = new List<OutAmadeusFlightSegmentDTO>();
    }
}
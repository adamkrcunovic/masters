namespace FlightSearch.DTOs.ThirdPartyModels.OutModels
{
    public class OutAmadeusFlightSegmentNestedDTO
    {
        public string IataCode { get; set; } = string.Empty;
        public string? Terminal { get; set; }
        public DateTime At { get; set; }
    }
}
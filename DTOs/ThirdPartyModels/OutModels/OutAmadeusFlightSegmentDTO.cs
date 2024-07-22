namespace FlightSearch.DTOs.ThirdPartyModels.OutModels
{
    public class OutAmadeusFlightSegmentDTO
    {
        public OutAmadeusFlightSegmentNestedDTO Departure { get; set; } = new OutAmadeusFlightSegmentNestedDTO();
        public OutAmadeusFlightSegmentNestedDTO Arrival { get; set; } = new OutAmadeusFlightSegmentNestedDTO();
        public string CarrierCode { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }
}
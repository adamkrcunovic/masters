namespace FlightSearch.DTOs.OutModels
{
    public class OutFlightSegmentDTO
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;

        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string FlightCode { get; set; } = string.Empty;

    }
}
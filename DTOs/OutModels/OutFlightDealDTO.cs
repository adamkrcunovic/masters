namespace FlightSearch.DTOs.OutModels
{
    public class OutFlightDealDTO
    {
        public string ToDuration { get; set; } = string.Empty;
        public List<OutFlightSegmentDTO> ToSegments { get; set; } = new List<OutFlightSegmentDTO>();
        public List<String> LayoverToDuration { get; set; } = new List<String>();
        public string? FromDuration { get; set; }
        public List<OutFlightSegmentDTO>? FromSegments { get; set; }
        public List<String>? LayoverFromDuration { get; set; }
        public Double TotalPrice { get; set; }
    }
}
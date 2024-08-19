using FlightSearch.DTOs.InModels;

namespace FlightSearch.DTOs.OutModels
{
    public class OutTripDTO
    {
        public int Id { get; set; }
        public string ItineraryName { get; set; } = string.Empty;
        public int Adults { get; set; }
        public string? TotalStayDuration { get; set; }
        public string ToDuration { get; set; } = string.Empty;
        public List<OutFlightSegmentDTO> ToSegments { get; set; } = new List<OutFlightSegmentDTO>();
        public List<string> LayoverToDuration { get; set; } = new List<string>();
        public string? FromDuration { get; set; }
        public List<OutFlightSegmentDTO> FromSegments { get; set; } = new List<OutFlightSegmentDTO>();
        public List<string> LayoverFromDuration { get; set; } = new List<string>();
        public List<string> CityVisit { get; set; } = new List<string>();
        public Double TotalPrice { get; set; }
        public Double CurrentPrice { get; set; }
        public string? ChatGPTGeneratedText { get; set; }
        public List<OutUserDTO> InvitedMembers { get; set; } = new List<OutUserDTO>();
        public List<OutCommentDTO> Comments { get; set; } = new List<OutCommentDTO>();
        public OutUserDTO Creator { get; set; } = new OutUserDTO();
    }
}
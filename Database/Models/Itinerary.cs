using FlightSearch.DTOs.OutModels;
using FlightSearch.Enums;

namespace FlightSearch.Database.Models
{
    public class Itinerary
    {
        public int Id { get; set; }
        public int Adults { get; set; }
        public string? TotalStayDuration { get; set; }
        public string ToDuration { get; set; } = string.Empty;
        public List<FlightSegment> Segments { get; set; } = new List<FlightSegment>();  //combined segments
        public int ToSegmentsLength { get; set; } //segments are combined so this is to separate them
        public string? LayoverToDuration { get; set; } //separated by ;
        public string? CityVisit { get; set; } //separated by ;
        public string? FromDuration { get; set; }
        public string? LayoverFromDuration { get; set; } //separated by ;
        public Double TotalPrice { get; set; }
        public string? ChatGPTGeneratedText { get; set; }
        public PriceChangeNotificationType PriceChangeNotificationType { get; set; }
        public int? Percentage { get; set; }
        public int? Amount { get; set; }
        public string? UserId { get; set; } //creator
        public User? User { get; set; }
        public List<ItineraryMember>? InvitedMembers { get; set; } //invited people
    }
}
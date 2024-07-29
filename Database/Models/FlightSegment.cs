namespace FlightSearch.Database.Models
{
    public class FlightSegment
    {
        public int Id { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string FlightCode { get; set; } = string.Empty;
        public int? ItineraryId { get; set; }
        public Itinerary? Itinerary { get; set; }

    }
}
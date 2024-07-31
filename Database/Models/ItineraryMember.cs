namespace FlightSearch.Database.Models
{
    public class ItineraryMember
    {
        public int Id { get; set; }
        public int? ItineraryId { get; set; }
        public Itinerary? Itinerary { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}
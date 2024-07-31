namespace FlightSearch.Database.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }
        public int? ItineraryId { get; set; }
        public Itinerary? Itinerary { get; set; }
    }
}
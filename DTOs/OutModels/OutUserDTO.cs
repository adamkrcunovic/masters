namespace FlightSearch.DTOs.InModels
{
    public class OutUserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateOnly Birthday { get; set; }
        public string Preferences { get; set; } = string.Empty;
    }
}
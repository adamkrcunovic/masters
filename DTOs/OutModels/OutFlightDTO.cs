namespace FlightSearch.DTOs.OutModels
{
    public class OutFlightDTO
    {
        public List<OutFlightDealDTO?> CheapestFlights { get; set; } = new List<OutFlightDealDTO?>();
        public List<OutFlightDealDTO?> FastestFlights { get; set; } = new List<OutFlightDealDTO?>();
        public List<OutFlightDealDTO?> LongestStayFlights { get; set; } = new List<OutFlightDealDTO?>();
        public List<OutFlightDealDTO?> CityVisit { get; set; } = new List<OutFlightDealDTO?>();
    }
}
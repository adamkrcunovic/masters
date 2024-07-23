namespace FlightSearch.DTOs.OutModels
{
    public class OutFlightDTO
    {
        List<OutFlightDealDTO> CheapestFlights { get; set; } = new List<OutFlightDealDTO>();
        List<OutFlightDealDTO> FastestFlights { get; set; } = new List<OutFlightDealDTO>();
        List<OutFlightDealDTO> LongestStayFlights { get; set; } = new List<OutFlightDealDTO>();
    }
}
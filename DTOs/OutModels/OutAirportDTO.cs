using FlightSearch.DTOs.InModels;

namespace FlightSearch.DTOs.OutModels
{
    public class OutAirportDTO
    {
        public string Name { get; set; } = string.Empty;
        public string IataCode { get; set; } = string.Empty;
    }
}
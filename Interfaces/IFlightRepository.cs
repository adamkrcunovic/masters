using FlightSearch.DTOs.OutModels;
using FlightSearch.DTOs.ThirdPartyModels.InModels;

namespace FlightSearch.Interfaces
{
    public interface IFlightRepository
    {
        Task<List<OutFlightDealDTO?>> GetFlightData(List<InAmadeusFlightSearchDTO> inAmadeusFlightSearchDTOs, Boolean multiCity, Boolean flyTheNightBefore);
        Task<List<OutAirportDTO>> GetAirports(string searchTerm);
    }
}
using FlightSearch.Database.Models;
using FlightSearch.DTOs.ThirdPartyModels.InModels;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;

namespace FlightSearch.Interfaces
{
    public interface IFlightRepository
    {
        Task<Object?> GetFlightData(List<InAmadeusFlightSearchDTO> inAmadeusFlightSearchDTOs);
    }
}
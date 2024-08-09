using FlightSearch.Database.Models;
using FlightSearch.DTOs.OutModels;

namespace FlightSearch.Interfaces
{
    public interface ITripRepository
    {
        public Task<string> GetChatGPTData(string inputText);
        Task<Itinerary> SaveItinerary(Itinerary itinerary);
        Task<bool> InviteUserToTrip(int itineraryId, string myUserId, string user);
        Task<bool> AddComment(int itineraryId, string myUserId, string comment);
        Task<List<OutTripDTO>> GetTrips(string myUserId);
    }
}
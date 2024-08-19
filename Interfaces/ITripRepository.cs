using FlightSearch.Database.Models;
using FlightSearch.DTOs.OutModels;

namespace FlightSearch.Interfaces
{
    public interface ITripRepository
    {
        public Task<string> GetChatGPTData(string inputText);
        Task<Itinerary> SaveItinerary(Itinerary itinerary);
        Task<List<String>?> InviteUserToTrip(int itineraryId, string myUserId, string user);
        Task<List<String>?> AddComment(int itineraryId, string myUserId, string comment);
        Task<List<OutTripDTO>> GetTrips(string myUserId);
    }
}
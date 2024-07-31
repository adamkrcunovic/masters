using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlightSearch.Constants;
using FlightSearch.Database;
using FlightSearch.Database.Models;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Enums;
using FlightSearch.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FlightSearch.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public TripRepository(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<string> GetChatGPTData(string inputText)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, ApplicationConstants.OpenAIApiAddress);
            var body = "{\"model\":\"gpt-4o\",\"messages\":[{\"role\":\"user\",\"content\":\"" + inputText + "\"}]}";
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApplicationConstants.OpenAIApiToken);
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var returnData = JsonSerializer.Deserialize<OutOpenAIChatDTO>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            return returnData != null ? returnData.Choices[0].Message.Content : "";
        }

        public async Task<Itinerary> SaveItinerary(Itinerary itinerary)
        {
            await _context.Itinenaries.AddAsync(itinerary);
            await _context.SaveChangesAsync();
            return itinerary;
        }

        public async Task<bool> InviteUserToTrip(int itineraryId, string myUserId, string user)
        {
            var friendship = await _context.UserFriendRequests.Where(userRequest => 
                userRequest.FriendsStatus == FriendsStatus.Friends &&
                ((userRequest.UserId1 == myUserId && userRequest.UserId2 == user) ||
                (userRequest.UserId1 == user && userRequest.UserId2 == myUserId))).FirstOrDefaultAsync();
            if (friendship == null)
            {
                return false;
            }
            var foundItinerary = await _context.Itinenaries.Where(itinerary => itineraryId == itinerary.Id).FirstOrDefaultAsync();
            if (foundItinerary == null || foundItinerary.UserId != myUserId)
            {
                return false;
            }
            var foundMemberItinerary = await _context.ItineraryMembers.Where(memberItinerary => memberItinerary.ItineraryId == itineraryId && memberItinerary.UserId == user).FirstOrDefaultAsync();
            if (foundMemberItinerary != null)
            {
                return false;
            }
            await _context.ItineraryMembers.AddAsync(new ItineraryMember{
                ItineraryId = itineraryId,
                UserId = user
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddComment(int itineraryId, string myUserId, string comment)
        {
            var foundItinerary = await _context.Itinenaries.Include(itinerary => itinerary.InvitedMembers).Where(itinerary => itineraryId == itinerary.Id).FirstOrDefaultAsync();
            if (foundItinerary == null)
            {
                return false;
            }
            var creatorId = foundItinerary.UserId;
            var invitedMembers = (foundItinerary.InvitedMembers??new List<ItineraryMember>()).Select(invitedMember => invitedMember.UserId).ToList();
            if (myUserId != creatorId && !invitedMembers.Contains(myUserId))
            {
                return false;
            }
            await _context.Comments.AddAsync(new Comment{
                CommentText = comment,
                DateCreated = DateTime.UtcNow,
                UserId = myUserId,
                ItineraryId = itineraryId
            });
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
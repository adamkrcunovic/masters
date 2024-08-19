using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlightSearch.Constants;
using FlightSearch.Database;
using FlightSearch.Database.Models;
using FlightSearch.DTOs.OutModels;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Enums;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<String>?> InviteUserToTrip(int itineraryId, string myUserId, string user)
        {
            var friendship = await _context.UserFriendRequests.Where(userRequest => 
                userRequest.FriendsStatus == FriendsStatus.Friends &&
                ((userRequest.UserId1 == myUserId && userRequest.UserId2 == user) ||
                (userRequest.UserId1 == user && userRequest.UserId2 == myUserId))).FirstOrDefaultAsync();
            if (friendship == null)
            {
                return null;
            }
            var foundItinerary = await _context.Itinenaries.Where(itinerary => itineraryId == itinerary.Id).FirstOrDefaultAsync();
            if (foundItinerary == null || foundItinerary.UserId != myUserId)
            {
                return null;
            }
            var foundMemberItinerary = await _context.ItineraryMembers.Where(memberItinerary => memberItinerary.ItineraryId == itineraryId && memberItinerary.UserId == user).FirstOrDefaultAsync();
            if (foundMemberItinerary != null)
            {
                return null;
            }
            await _context.ItineraryMembers.AddAsync(new ItineraryMember{
                ItineraryId = itineraryId,
                UserId = user
            });
            await _context.SaveChangesAsync();
            var friend = await _context.Users.Where(userDb => userDb.Id == user).FirstOrDefaultAsync();
            var devicesIds = friend.DeviceIds.Split(";").Where(id => id.Length > 0).ToList();
            return devicesIds;
        }

        public async Task<List<String>?> AddComment(int itineraryId, string myUserId, string comment)
        {
            var devicesToSendNotification = new List<string>();
            var foundItinerary = await _context.Itinenaries.Include(itinerary => itinerary.InvitedMembers).ThenInclude(itineraryMember => itineraryMember.User).Include(itinerary => itinerary.User).Where(itinerary => itineraryId == itinerary.Id).FirstOrDefaultAsync();
            if (foundItinerary == null)
            {
                return null;
            }
            var creatorId = foundItinerary.UserId;
            var invitedMembersUsers = (foundItinerary.InvitedMembers??new List<ItineraryMember>()).ToList();
            var invitedMembers = invitedMembersUsers.Select(invitedMember => invitedMember.UserId).ToList();
            if (myUserId != creatorId && !invitedMembers.Contains(myUserId))
            {
                return null;
            }
            else {
                Console.WriteLine("Dosao ovde" + invitedMembersUsers.Count);
                if (myUserId != creatorId) devicesToSendNotification = devicesToSendNotification.Concat(foundItinerary.User.DeviceIds.Split(";").ToList()).ToList();
                foreach (var invitedMemberUser in invitedMembersUsers) {
                    if (invitedMemberUser.User.Id != myUserId) {
                        devicesToSendNotification = devicesToSendNotification.Concat(invitedMemberUser.User.DeviceIds.Split(";").Where(id => id.Length > 0).ToList()).ToList();
                        Console.WriteLine("KOMENTARIIII" + invitedMemberUser.User.DeviceIds);
                    }
                }
            }
            await _context.Comments.AddAsync(new Comment{
                CommentText = comment,
                DateCreated = DateTime.UtcNow,
                UserId = myUserId,
                ItineraryId = itineraryId
            });
            await _context.SaveChangesAsync();
            return devicesToSendNotification;
        }

        public async Task<List<OutTripDTO>> GetTrips(string myUserId)
        {
            var myTrips = await _context.Itinenaries
                .Include(itinerary => itinerary.Comments).ThenInclude(comment => comment.User)
                .Include(itinerary => itinerary.User).ThenInclude(user => user.Country)
                .Include(itinerary => itinerary.Segments)
                .Include(itinerary => itinerary.InvitedMembers).ThenInclude(invitedMember => invitedMember.User).ThenInclude(user => user.Country)
                .Where(itinerary => itinerary.UserId == myUserId)
                .Select(myTrip => myTrip.ToOutTripFromDbTrip(false)).ToListAsync();
            var myInvitedTrips = await _context.ItineraryMembers
                .Where(itineraryMember => itineraryMember.UserId == myUserId)
                .Include(itineraryMember => itineraryMember.Itinerary)
                .ThenInclude(itinerary => itinerary.Comments)
                .ThenInclude(comment => comment.User)
                .Include(itineraryMember => itineraryMember.Itinerary)
                .ThenInclude(itinerary => itinerary.User).ThenInclude(user => user.Country)
                .Include(itineraryMember => itineraryMember.Itinerary)
                .ThenInclude(itinerary => itinerary.Segments)
                .Include(itineraryMember => itineraryMember.Itinerary)
                .ThenInclude(itinerary => itinerary.InvitedMembers).ThenInclude(invitedMember => invitedMember.User).ThenInclude(user => user.Country)
                .Where(itinerary => itinerary.UserId == myUserId)
                .Select(itineraryMember => itineraryMember.Itinerary.ToOutTripFromDbTrip(false)).ToListAsync();
            return myTrips.Concat(myInvitedTrips).OrderBy(trip => trip.ToSegments[0].Departure).ToList();
        }
    }
}
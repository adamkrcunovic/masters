using FlightSearch.DTOs.InModels;
using FlightSearch.Helpers;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightSearch.Controllers
{
    [ApiController]
    [Route("api/trip")]
    public class TripController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;

        public TripController(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateTrip([FromBody]InTripDTO inTripDTO)
        {
            var chatGPTText = TripMapper.TripToChatGPTText(inTripDTO);
            var generatedTripText = await _tripRepository.GetChatGPTData(chatGPTText);
            return Ok(generatedTripText);
        }

        [HttpPost("saveItinerary")]
        [Authorize]
        public async Task<IActionResult> SaveItinerary([FromBody] InItineraryDTO inItineraryDTO)
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var itinerary = inItineraryDTO.InItineraryToDbItinerary(userId);
            await _tripRepository.SaveItinerary(itinerary);
            return Ok();
        }

        [HttpPost("inviteUserToTrip/{itineraryId}/{user}")]
        [Authorize]
        public async Task<IActionResult> InviteUserToTrip([FromRoute] int itineraryId, [FromRoute] string user)
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var commentAddedUserDevices = await _tripRepository.InviteUserToTrip(itineraryId, userId, user);
            if (commentAddedUserDevices != null)
            {
                return Ok(commentAddedUserDevices);
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("addComment/{itineraryId}/{comment}")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromRoute] int itineraryId, [FromRoute] string comment)
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var commentAddedUserDevices = await _tripRepository.AddComment(itineraryId, userId, comment);
            if (commentAddedUserDevices != null)
            {
                return Ok(commentAddedUserDevices);
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet("getTrips")]
        [Authorize]
        public async Task<IActionResult> GetTrips()
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var myItineraries = await _tripRepository.GetTrips(userId);
            return Ok(myItineraries);
        }
    }
}
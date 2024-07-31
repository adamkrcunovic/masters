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
            var userAdded = await _tripRepository.InviteUserToTrip(itineraryId, userId, user);
            if (userAdded)
            {
                return Ok();
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
            var commentAdded = await _tripRepository.AddComment(itineraryId, userId, comment);
            if (commentAdded)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
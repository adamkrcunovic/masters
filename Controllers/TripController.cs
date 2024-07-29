using FlightSearch.DTOs.InModels;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;
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
    }
}
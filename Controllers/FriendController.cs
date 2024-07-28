using FlightSearch.Helpers;
using FlightSearch.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightSearch.Controllers
{
    [Route("api/friend")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly IFriendRepository _friendRepository;

        public FriendController(IFriendRepository friendRepository)
        {
            _friendRepository = friendRepository;
        }

        [HttpPost("sendRequest/{friendId}")]
        [Authorize]
        public async Task<IActionResult> SendFriendRequest([FromRoute] string friendId)
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var friendRequest = await _friendRepository.SendFriendRequest(userId, friendId);
            if (friendRequest == null)
            {
                return BadRequest("Request already exists");
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("updateRequest/{friendId}/{acceptAndRejectRequest}")]
        [Authorize]
        public async Task<IActionResult> AcceptOrRejectFriendRequest([FromRoute] string friendId, [FromRoute] bool acceptAndRejectRequest)
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var friendRequest = await _friendRepository.AcceptOrRejectFriendRequest(userId, friendId, acceptAndRejectRequest);
            if (friendRequest == null)
            {
                return BadRequest("Request not foud, or already friends"); 
            }
            else
            {
                return Ok();
            }
        }
    }
}
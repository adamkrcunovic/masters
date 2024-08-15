using FlightSearch.Helpers;
using FlightSearch.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
                var friend = await _friendRepository.getFriend(friendId);
                var devicesIds = friend.DeviceIds.Split(";").Where(id => !id.IsNullOrEmpty()).ToList();
                return Ok(devicesIds);
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
                var friend = await _friendRepository.getFriend(friendId);
                var devicesIds = friend.DeviceIds.Split(";").Where(id => !id.IsNullOrEmpty()).ToList();
                return Ok(acceptAndRejectRequest ? devicesIds : new List<string>());
            }
        }

        [HttpGet("search/{searchTerm}")]
        [Authorize]
        public async Task<IActionResult> SearchUsers([FromRoute] string searchTerm)
        {
            var users = await _friendRepository.SearchUsers(searchTerm);
            if (users.IsNullOrEmpty())
            {
                return NotFound();
            }
            return Ok(users);
        }
    }
}
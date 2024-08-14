using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;
using FlightSearch.Helpers;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FlightSearch.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;
        public AccountController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] InRegisterDTO inRegisterDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = inRegisterDTO.InRegisterUserToDbUser();
                var createdUser = await _userManager.CreateAsync(user, inRegisterDTO.Password);
                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        var token = new OutRegisterDTO{
                            Token = _tokenService.CreateToken(user)
                        };
                        return Ok(token.Token);
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return StatusCode(500, createdUser.Errors);
                }
            } catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(InLoginDTO inLoginDTO)
        {
            if(!ModelState.IsValid)
            {
                return (BadRequest(ModelState));
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(user => user.UserName == inLoginDTO.Email.ToUpper());
            if (user == null)
            {
                return NotFound("User not found");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, inLoginDTO.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Incorrect password");
            }
            var userDeviceIds = user.DeviceIds;
            var listOfDeviceIds = userDeviceIds.Split(";").ToList();
            listOfDeviceIds.RemoveAt(listOfDeviceIds.Count - 1);
            if (!listOfDeviceIds.Contains(inLoginDTO.DeviceId))
            {
                listOfDeviceIds.Add(inLoginDTO.DeviceId);
            }
            var newDeviceIds = "";
            foreach(var newDeviceId in listOfDeviceIds)
            {
                newDeviceIds += newDeviceId + ";";
            }
            user.DeviceIds = newDeviceIds;
            await _userManager.UpdateAsync(user);
            var token = new OutRegisterDTO{
                Token = _tokenService.CreateToken(user)
            };
            return Ok(token.Token);
        }

        [HttpPut("editPersonalData")]
        [Authorize]
        public async Task<IActionResult> EditPersonalData([FromBody] InEditPersonalDataDTO inEditPersonalDataDTO)
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var foundUser = await _userManager.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();
            if (foundUser != null)
            {
                if (!inEditPersonalDataDTO.Name.IsNullOrEmpty())
                {
                    foundUser.Name = inEditPersonalDataDTO.Name??"";
                }
                if (!inEditPersonalDataDTO.LastName.IsNullOrEmpty())
                {
                    foundUser.LastName = inEditPersonalDataDTO.LastName??"";
                }
                if (inEditPersonalDataDTO.Birthday != null)
                {
                    foundUser.Birthday = inEditPersonalDataDTO.Birthday??new DateOnly();
                }
                if (inEditPersonalDataDTO.CountryId != null)
                {
                    foundUser.CountryId = inEditPersonalDataDTO.CountryId;
                }
                if (!inEditPersonalDataDTO.Preferences.IsNullOrEmpty())
                {
                    foundUser.Preferences = inEditPersonalDataDTO.Preferences??"";
                }
                await _userManager.UpdateAsync(foundUser);
                return Ok();
            }
            return NotFound("User not found");
        }

        [HttpGet("getUserData")]
        [Authorize]
        public async Task<IActionResult> GetUserData()
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var foundUser = await _userManager.Users.Include(user => user.Country).Include(user => user.SentFriendRequests).ThenInclude(sentFriendRequest => sentFriendRequest.User2).Include(user => user.ReceivedFriendRequests).ThenInclude(receivedFriendRequest => receivedFriendRequest.User1).Where(user => user.Id == userId).FirstOrDefaultAsync();
            if (foundUser != null)
            {
                return Ok(foundUser.DbUserToOutUser());
            }
            return NotFound();
        }

        [HttpPut("logout/{deviceId}")]
        [Authorize]
        public async Task<IActionResult> Logout([FromRoute] string deviceId)
        {
            var userId = await TokenHelper.GetUserIdFromHttpContext(HttpContext);
            var foundUser = await _userManager.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();
            if (foundUser != null)
            {
                var userDeviceIds = foundUser.DeviceIds;
                var listOfDeviceIds = userDeviceIds.Split(";").ToList();
                listOfDeviceIds.RemoveAt(listOfDeviceIds.Count - 1);
                if (listOfDeviceIds.Contains(deviceId))
                {
                    listOfDeviceIds.Remove(deviceId);
                }
                var newDeviceIds = "";
                foreach(var newDeviceId in listOfDeviceIds)
                {
                    newDeviceIds += newDeviceId + ";";
                }
                foundUser.DeviceIds = newDeviceIds;
                await _userManager.UpdateAsync(foundUser);
                return Ok();
            }
            return NotFound("User not found");
        }
    }
} 
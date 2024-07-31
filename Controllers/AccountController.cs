using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;
using FlightSearch.Helpers;
using FlightSearch.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                var user = new User{
                    Email = inRegisterDTO.Email,
                    UserName = inRegisterDTO.Email.ToUpper(),
                    DeviceIds = inRegisterDTO.DeviceId + ";"
                };
                var createdUser = await _userManager.CreateAsync(user, inRegisterDTO.Password);
                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        return Ok(new OutRegisterDTO{
                            Token = _tokenService.CreateToken(user)
                        });
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
            return Ok(new OutRegisterDTO{
                Token = _tokenService.CreateToken(user)
            });
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
            }
            return Ok();
        }
    }
} 
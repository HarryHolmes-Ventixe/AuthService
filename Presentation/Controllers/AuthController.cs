using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;

namespace ApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, IConfiguration configuration, UserManager<UserEntity> userManager) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<UserEntity> _userManager = userManager;

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.CreateUserAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            return StatusCode(500, result.Error);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authService.SignInAsync(model);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        return Unauthorized(new { error = "could not find user." });
                    }

                    var token = _authService.GenerateJwtToken(user);

                    return Ok(new {token});
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("signout")]
        public new async Task<IActionResult> SignOut()
        {
            Response.Cookies.Delete("jwt");
            await _authService.SignOutAsync();
            return Ok();
        }
    }
}

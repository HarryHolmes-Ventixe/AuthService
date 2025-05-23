using Data.Entities;
using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

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
            var result = await _authService.SignInAsync(model);
            if (result.Succeeded)
            {
                return Ok();
            }
            return Unauthorized();
        }
    }
}

using ApiApp.Entities;
using ApiApp.Models;
using ApiApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = model.Password,
                UserName = model.Email
            };

            var result = await _authService.CreateUserAsync(user);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.SignIn(model);
            if (result.Succeeded)
            {
                return Ok();
            }
            return Unauthorized();
        }
    }
}

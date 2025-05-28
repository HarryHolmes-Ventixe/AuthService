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

        // This method includes the creation of the JTW Token. 
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
                    var userEntity = await _userManager.FindByEmailAsync(model.Email);
                    if (userEntity == null)
                    {
                        return StatusCode(500, new { error = "User not found after sign-in" });
                    }

                    var user = new User
                    {
                        Id = userEntity.Id,
                        FirstName = userEntity.FirstName,
                        LastName = userEntity.LastName,
                        Email = userEntity.Email!
                    };

                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, model.Email),
                        new Claim("firstName", user.FirstName),
                        new Claim("lastName", user.LastName)
                    };

                    var jwtKey = _configuration["Jwt:Key"];
                    var jwtIssuer = _configuration["Jwt:Issuer"];
                    var jwtAudience = _configuration["Jwt:Audience"];

                    if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                    {
                        return StatusCode(500, new { error = "JWT configuration is missing" });
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: jwtIssuer,
                        audience: jwtAudience,
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(60),
                        signingCredentials: creds
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    Response.Cookies.Append("jwt", tokenString, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                    });
                    return Ok(new {token = tokenString, user });
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging set up
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

        // GitHub copilot suggested to add in this method to help with the identifying of the JWT token
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var firstName = User.FindFirst("firstName")?.Value;
            var lastName = User.FindFirst("lastName")?.Value;
            var email = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;


            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            return Ok(new { firstName, lastName, email});
        }

    }
}

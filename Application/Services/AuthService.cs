using Data.Entities;
using Application.Models;
using Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services;

public class AuthService(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, IAuthRepository authRepository, IConfiguration configuration) : IAuthService
{
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IConfiguration _configuration = configuration;

    #region CRUD

    public async Task<UserResult> CreateUserAsync(UserRequest request)
    {
        try
        {
            var userEntity = new UserEntity
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email
            };

            var identityResult = await _userManager.CreateAsync(userEntity, request.Password);
            return identityResult.Succeeded
                ? new UserResult
                {
                    Success = true
                }
                : new UserResult
                {
                    Success = false,
                    Error = string.Join(";", identityResult.Errors.Select(e => e.Description))
                };
        }
        catch (Exception ex)
        {
            return new UserResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<UserResult<IEnumerable<User>>> GetAllUsersAsync()
    {
        var result = await _authRepository.GetAllAsync();
        var users = result.Result?.Select(x => new User
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email!
        });

        return new UserResult<IEnumerable<User>>
        {
            Success = true,
            Result = users
        };
    }

    public async Task<UserResult<User?>> GetUserAsync(string email)
    {
        var result = await _authRepository.GetAsync(x => x.Email.ToUpper() == email.ToUpper());
        if (result.Success && result.Result != null)
        {
            var user = new User
            {
                Id = result.Result.Id,
                FirstName = result.Result.FirstName,
                LastName = result.Result.LastName,
                Email = result.Result.Email!
            };

            return new UserResult<User?>
            {
                Success = true,
                Result = user
            };
        }
        return new UserResult<User?>
        {
            Success = false,
            Error = "User not found"
        };

    }

    #endregion

    #region User operations

    public async Task<SignInResult> SignInAsync(SignInModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return result;
        }
        return SignInResult.Failed;
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public string GenerateJwtToken(UserEntity user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key");
        var issuers = jwtSection.GetSection("Issuer").Get<string[]>();
        var audiences = jwtSection.GetSection("Audience").Get<string[]>();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("firstName", user.FirstName ?? string.Empty),
            new("lastName", user.LastName ?? string.Empty),
            new("email", user.Email ?? string.Empty)
        };

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var signingKey = new SymmetricSecurityKey(keyBytes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = issuers?.FirstOrDefault(),
            Audience = audiences?.FirstOrDefault(),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


    #endregion
}

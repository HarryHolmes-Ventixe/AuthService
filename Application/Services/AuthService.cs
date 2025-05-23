using Data.Entities;
using Application.Models;
using Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class AuthService(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, IAuthRepository authRepository) : IAuthService
{
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly IAuthRepository _authRepository = authRepository;

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

    public async Task<UserResult<User?>> GetUserAsync(string eventId)
    {
        var result = await _authRepository.GetAsync(x => x.Id == eventId);
        if (result.Success && result.Result != null)
        {
            var currentEvent = new User
            {
                Id = result.Result.Id,
                FirstName = result.Result.FirstName,
                LastName = result.Result.LastName,
                Email = result.Result.Email!
            };

            return new UserResult<User?>
            {
                Success = true,
                Result = currentEvent
            };
        }
        return new UserResult<User?>
        {
            Success = false,
            Error = "Event not found"
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

    #endregion
}

using ApiApp.Entities;
using ApiApp.Models;
using ApiApp.Repositories;
using Microsoft.AspNetCore.Identity;

namespace ApiApp.Services;

public class AuthService(SignInManager<User> signInManager, UserManager<User> userManager, IAuthRepository authRepository) : IAuthService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly IAuthRepository _authRepository = authRepository;

    #region CRUD

    public async Task<IdentityResult> CreateUserAsync(User user)
    {
        var exists = await _authRepository.ExistsAsync(user.Email);

        if (exists)
        {
            throw new Exception("User already exists");
        }

        var result = await _userManager.CreateAsync(user, user.PasswordHash);
        return result;
    }

    public async Task<User?> GetUserAsync(string email)
    {
        var user = await _authRepository.GetAsync(email);
        return user;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        var exists = await _authRepository.ExistsAsync(email);
        return exists;
    }

    #endregion

    #region User operations

    public async Task<SignInResult> SignIn(SignInModel model)
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

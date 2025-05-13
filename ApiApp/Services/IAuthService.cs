using ApiApp.Entities;
using ApiApp.Models;
using Microsoft.AspNetCore.Identity;

namespace ApiApp.Services
{
    public interface IAuthService
    {
        Task<IdentityResult> CreateUserAsync(User user);
        Task<User?> GetUserAsync(string email);
        Task<SignInResult> SignIn(SignInModel model);
        Task SignOutAsync();
        Task<bool> UserExistsAsync(string email);
    }
}
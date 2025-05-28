using Application.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public interface IAuthService
    {
        Task<UserResult> CreateUserAsync(UserRequest request);
        Task<UserResult<IEnumerable<User>>> GetAllUsersAsync();
        Task<UserResult<User?>> GetUserAsync(string email);
        Task<SignInResult> SignInAsync(SignInModel model);
        Task SignOutAsync();
        string GenerateJwtToken(UserEntity user);
    }
}
using ApiApp.Entities;

namespace ApiApp.Repositories
{
    public interface IAuthRepository
    {
        Task<bool> CreateAsync(User user);
        Task<IEnumerable<User>?> GetAllAsync();
        Task<User?> GetAsync(string email);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string email);
    }
}
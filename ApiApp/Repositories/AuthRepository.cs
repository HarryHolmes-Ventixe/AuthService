using ApiApp.Data;
using ApiApp.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ApiApp.Repositories;

public class AuthRepository(DataContext context, DbSet<User> dbSet) : IAuthRepository
{
    protected readonly DataContext _context = context;
    protected readonly DbSet<User> _dbSet = dbSet;

    public virtual async Task<bool> CreateAsync(User user)
    {
        try
        {
            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
            return false;
        }
    }

    public virtual async Task<IEnumerable<User>?> GetAllAsync()
    {
        var userEntities = await _dbSet.ToListAsync();
        return userEntities;
    }

    public virtual async Task<User?> GetAsync(string email)
    {
        var userEntity = await _dbSet.FindAsync(email);
        return userEntity;
    }

    public virtual async Task<bool> UpdateAsync(User user)
    {
        try
        {
            _dbSet.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
            return false;
        }
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var userEntity = await _dbSet.FindAsync(id);
            if (userEntity == null)
            {
                return false;
            }
            _dbSet.Remove(userEntity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
            return false;
        }
    }

    public virtual async Task<bool> ExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(e => e.Email == email);
    }
}

using Data.Data;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected BaseRepository(DataContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    #region CRUD Operations

    public virtual async Task<RepositoryResult> AddAsync(TEntity entity)
    {
        try
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();

            return new RepositoryResult
            {
                Success = true
            };
        }
        catch
        {
            return new RepositoryResult
            {
                Success = false,
                Error = "An error occurred while adding the entity."
            };
        }
    }

    public virtual async Task<RepositoryResult<IEnumerable<TEntity>>> GetAllAsync()
    {
        try
        {
            var entities = await _dbSet.ToListAsync();
            return new RepositoryResult<IEnumerable<TEntity>>
            {
                Success = true,
                Result = entities
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<IEnumerable<TEntity>>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public virtual async Task<RepositoryResult<TEntity?>> GetAsync(Expression<Func<TEntity, bool>> expression)
    {
        try
        {
            var entity = await _dbSet.FirstOrDefaultAsync(expression);
            if (entity == null)
            {
                throw new Exception("Entity not found.");
            }

            return new RepositoryResult<TEntity?>
            {
                Success = true,
                Result = entity
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<TEntity?>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public virtual async Task<RepositoryResult> UpdateAsync(TEntity entity)
    {
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();

            return new RepositoryResult
            {
                Success = true
            };
        }
        catch
        {
            return new RepositoryResult
            {
                Success = false,
                Error = "An error occurred while adding the entity."
            };
        }
    }

    public virtual async Task<RepositoryResult> DeleteAsync(TEntity entity)
    {
        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();

            return new RepositoryResult
            {
                Success = true
            };
        }
        catch
        {
            return new RepositoryResult
            {
                Success = false,
                Error = "An error occurred while adding the entity."
            };
        }
    }

    public virtual async Task<RepositoryResult> ExistsAsync(Expression<Func<TEntity, bool>> expression)
    {
        var result = await _dbSet.AnyAsync(expression);
        return result
            ? new RepositoryResult
            {
                Success = true
            }
            : new RepositoryResult
            {
                Success = false,
                Error = "Not found."
            };
    }

    #endregion
}

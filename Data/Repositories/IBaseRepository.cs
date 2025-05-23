using Data.Models;
using System.Linq.Expressions;

namespace Data.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<RepositoryResult> AddAsync(TEntity entity);
        Task<RepositoryResult> DeleteAsync(TEntity entity);
        Task<RepositoryResult> ExistsAsync(Expression<Func<TEntity, bool>> expression);
        Task<RepositoryResult<IEnumerable<TEntity>>> GetAllAsync();
        Task<RepositoryResult<TEntity?>> GetAsync(Expression<Func<TEntity, bool>> expression);
        Task<RepositoryResult> UpdateAsync(TEntity entity);
    }
}
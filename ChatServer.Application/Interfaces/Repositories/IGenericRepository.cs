using System.Linq.Expressions;

namespace ChatServer.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T>
    {
        Task<T?> GetByIdAsync(Guid guid, int intId);
        Task<T?> GetByIdAsync(object id);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(T entity);
        Task<T> CreateAsync(T entity);
        Task<T?> PatchAsync<TDto>(object id, TDto dto);
        Task<T?> GetFirstOrDefaultByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string[]? includeProperties = null);
    }
}

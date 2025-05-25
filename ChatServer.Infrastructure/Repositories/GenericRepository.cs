using System.Linq.Expressions;
using ChatServer.Application.Interfaces.Repositories;
using ChatServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }
        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
            return entity;
        }

        public async Task<T> DeleteAsync(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);

            _dbSet.Remove(entity);
            await Task.CompletedTask;
            return entity;
        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string[]? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            if (orderBy != null)
            {
                return [.. orderBy(query)];
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async Task<T?> GetByIdAsync(Guid guid, int intId)
        {
            return await _dbSet.FindAsync(intId, guid);
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> GetFirstOrDefaultByConditionAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<T?> PatchAsync<TDto>(object id, TDto dto)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return null;

            foreach (var prop in typeof(TDto).GetProperties())
            {
                var value = prop.GetValue(dto);
                if (value != null)
                {
                    var entityProp = typeof(T).GetProperty(prop.Name);
                    if (entityProp != null && entityProp.CanWrite)
                    {
                        entityProp.SetValue(entity, value);
                    }
                }
            }
            return entity;
        }
    }
}

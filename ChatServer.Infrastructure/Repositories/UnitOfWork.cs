using ChatServer.Application.Interfaces.Repositories;
using ChatServer.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T>(_context);
                _repositories.Add(type, repositoryInstance);
            }
            return (IGenericRepository<T>)_repositories[type];
        }

        public TCustomRepo CustomRepository<TCustomRepo>() where TCustomRepo : class
        {
            var type = typeof(TCustomRepo);
            if (!_repositories.ContainsKey(type))
            {
                var repository = _serviceProvider.GetRequiredService<TCustomRepo>();
                _repositories[type] = repository;
            }
            return (TCustomRepo)_repositories[type];
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

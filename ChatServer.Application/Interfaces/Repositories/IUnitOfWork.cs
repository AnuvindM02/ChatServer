namespace ChatServer.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> GetRepository<T>() where T : class;
        TCustomRepo CustomRepository<TCustomRepo>() where TCustomRepo : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

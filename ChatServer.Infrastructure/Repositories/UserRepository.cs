using ChatServer.Application.Interfaces.Repositories;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Persistence;

namespace ChatServer.Infrastructure.Repositories
{
    public class UserRepository: GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext _context) : base(_context)
        {
        }
    }
}

using AutoMapper;
using ChatServer.Application.Commands;
using ChatServer.Application.Interfaces.Repositories;
using ChatServer.Domain.Entities;
using MediatR;

namespace ChatServer.Application.Handlers
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public CreateUserHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.CustomRepository<IUserRepository>();
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            User user = _mapper.Map<User>(request);
            await _userRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return user.AuthUserId;
        }
    }
}

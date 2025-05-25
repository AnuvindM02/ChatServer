using MediatR;
namespace ChatServer.Application.Commands;
public class CreateUserCommand : IRequest<int>
{
    public int UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string? MiddleName { get; }
    public string? LastName { get; }

    public CreateUserCommand(int userId, string email, string firstName, string? middleName, string? lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
    }
}


namespace ChatServer.Application.RabbitMq.Models;
public sealed record NewUserMessage(int UserId, string Email, string FirstName, string? MiddleName, string? LastName);

using AutoMapper;
using ChatServer.Application.Commands;
using ChatServer.Application.RabbitMq.Models;
using ChatServer.Domain.Entities;
using MediatR;

namespace ChatServer.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<NewUserMessage, CreateUserCommand>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<CreateUserCommand, User>()
                .ForMember(dest => dest.AuthUserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Username,
                opt => opt.MapFrom(
                    src => $"{src.FirstName}" +
                $"{(src.MiddleName != "" ? (" " + src.MiddleName) : "")}" +
                $"{(src.LastName != "" ? (" " + src.LastName) : "")}")
                );

        }
    }
}

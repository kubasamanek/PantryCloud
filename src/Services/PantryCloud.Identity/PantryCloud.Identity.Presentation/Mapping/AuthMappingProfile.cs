using AutoMapper;
using PantryCloud.Identity.Application.Commands;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Presentation.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<RegisterRequestDto, RegisterCommand>()
            .ConstructUsing(dto => new RegisterCommand(dto));
        
        CreateMap<RefreshTokenRequestDto, RefreshTokenCommand>()
            .ConstructUsing(dto => new RefreshTokenCommand(dto));
        
        CreateMap<LoginRequestDto, LoginCommand>()
            .ConstructUsing(dto => new LoginCommand(dto));
    }
}
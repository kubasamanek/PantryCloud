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
        
        CreateMap<ForgotPasswordRequestDto, ForgotPasswordCommand>()
            .ConstructUsing(dto => new ForgotPasswordCommand(dto));
        
        CreateMap<ResetPasswordRequestDto, ResetPasswordCommand>()
            .ConstructUsing(dto => new ResetPasswordCommand(dto));

        CreateMap<VerifyEmailRequestDto, VerifyEmailCommand>()
            .ConstructUsing(dto => new VerifyEmailCommand(dto));
    }
}
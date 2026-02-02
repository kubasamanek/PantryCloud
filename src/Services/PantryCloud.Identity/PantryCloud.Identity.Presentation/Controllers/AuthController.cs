using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.Identity.Application.Commands;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.SharedKernel.Controllers;

namespace PantryCloud.Identity.Presentation.Controllers;

[Route("api/auth")]
public class AuthController(IMediator mediator, IMapper mapper) : ApiControllerBase(mediator, mapper)
{

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var deviceName = Request.Headers["X-Device-Name"].FirstOrDefault() ?? request.DeviceName;
        var requestWithDevice = request with { DeviceName = deviceName };
        var command = new LoginCommand(requestWithDevice);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }
  
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var deviceName = Request.Headers["X-Device-Name"].FirstOrDefault() ?? request.DeviceName;
        var requestWithDevice = request with { DeviceName = deviceName };
        var command = new RefreshTokenCommand(requestWithDevice);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }
    
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var command = Mapper.Map<RegisterCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }
    
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var command = Mapper.Map<ForgotPasswordCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }
    
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var command = Mapper.Map<ResetPasswordCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<VerifyEmailCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, StatusCodes.Status200OK);
    }

}
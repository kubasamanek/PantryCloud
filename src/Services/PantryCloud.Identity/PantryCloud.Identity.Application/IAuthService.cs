using ErrorOr;
using MediatR;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.Identity.Application;

public interface IAuthService
{
    Task<ErrorOr<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<ResetPasswordResponseDto>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<VerifyEmailResponseDto>> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<ListSessionsResponseDto>> ListSessionsAsync(CancellationToken cancellationToken = default);
    Task<ErrorOr<Unit>> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<ErrorOr<Unit>> RevokeAllOtherSessionsAsync(CancellationToken cancellationToken = default);
    Task<ErrorOr<Unit>> LogoutAsync(LogoutRequestDto request, CancellationToken cancellationToken = default);
}
using PantryCloud.Identity.Core.Entities;

namespace PantryCloud.Identity.Application;

public interface ITokenProvider
{
    string CreateAccessToken(ApplicationUser user, Guid? sessionId = null);
    string CreateRefreshToken();
    string CreatePasswordResetToken();
    string CreateVerifyEmailToken();
}
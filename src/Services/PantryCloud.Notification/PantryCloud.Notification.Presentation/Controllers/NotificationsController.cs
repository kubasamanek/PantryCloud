using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;

namespace PantryCloud.Notification.Presentation.Controllers;

/// <summary>
/// Serves GET /me for current user's notification history.
/// Gateway forwards /api/notification/me to this app as /me.
/// </summary>
[ApiController]
[Route("")]
[Authorize]
public class NotificationsController(IUserNotificationRepository repository) : ControllerBase
{
    /// <summary>
    /// Gets recent notifications for the current user (for "what I missed" when offline).
    /// </summary>
    /// <param name="limit">Max number to return (default 50, max 100).</param>
    /// <param name="since">Optional UTC datetime to return only notifications after this time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int limit = 50,
        [FromQuery] DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();
        var effectiveLimit = Math.Clamp(limit, 1, 100);
        var list = await repository.GetByUserIdAsync(userId.Value, effectiveLimit, since, cancellationToken);
        return Ok(list);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

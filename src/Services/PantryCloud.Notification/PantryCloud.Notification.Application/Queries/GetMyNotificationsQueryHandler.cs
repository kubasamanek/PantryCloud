using ErrorOr;
using MediatR;
using PantryCloud.Notification.Core.Dtos;

namespace PantryCloud.Notification.Application.Queries;

public class GetMyNotificationsQueryHandler(IUserNotificationRepository repository)
    : IRequestHandler<GetMyNotificationsQuery, ErrorOr<IReadOnlyList<NotificationDto>>>
{
    public async Task<ErrorOr<IReadOnlyList<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await repository.GetByUserIdAsync(request.UserId, request.Limit, request.Since, cancellationToken);
        return notifications.ToList();
    }
}

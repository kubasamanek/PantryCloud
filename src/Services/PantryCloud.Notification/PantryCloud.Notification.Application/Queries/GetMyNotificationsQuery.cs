using ErrorOr;
using MediatR;
using PantryCloud.Notification.Core.Dtos;

namespace PantryCloud.Notification.Application.Queries;

public record GetMyNotificationsQuery(Guid UserId, int Limit, DateTime? Since) : IRequest<ErrorOr<IReadOnlyList<NotificationDto>>>;

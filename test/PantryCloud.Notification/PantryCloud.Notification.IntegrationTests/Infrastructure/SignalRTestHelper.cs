using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR.Client;
using PantryCloud.Notification.Core.Dtos;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure;

public static class SignalRTestHelper
{
    public static void OnReceiveNotification(HubConnection connection, ConcurrentBag<NotificationDto> received)
    {
        connection.On("ReceiveNotification", [typeof(NotificationDto)], (args, _) =>
        {
            if (args is [NotificationDto n])
                received.Add(n);
            return Task.FromResult<object?>(null);
        }, state: null!);
    }
}

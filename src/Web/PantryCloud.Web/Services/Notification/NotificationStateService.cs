namespace PantryCloud.Web.Services.Notification;

public class NotificationStateService : INotificationStateService
{
    private readonly List<NotificationMessage> _items = [];
    private readonly HashSet<Guid> _readIds = [];
    private readonly Lock _lock = new();

    public event Action? StateChanged;

    public int UnreadCount
    {
        get
        {
            lock (_lock)
            {
                return _items.Count(m => !_readIds.Contains(m.Id));
            }
        }
    }

    public NotificationStateService(INotificationHubClient hubClient)
    {
        hubClient.OnNotification += OnHubNotification;
    }

    private void OnHubNotification(NotificationMessage msg)
    {
        Add(msg);
    }

    public IReadOnlyList<NotificationMessage> GetListForPanel()
    {
        lock (_lock)
        {
            return _items
                .OrderByDescending(m => !_readIds.Contains(m.Id))
                .ThenByDescending(m => m.CreatedAt)
                .ToList();
        }
    }

    public void Add(NotificationMessage message)
    {
        lock (_lock)
        {
            if (_items.Any(m => m.Id == message.Id)) return;
            _items.Insert(0, message);
        }
        StateChanged?.Invoke();
    }

    public void AddRange(IEnumerable<NotificationMessage> messages)
    {
        lock (_lock)
        {
            foreach (var msg in messages)
            {
                if (_items.Any(m => m.Id == msg.Id)) continue;
                _items.Add(msg);
            }
            _items.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
        }
        StateChanged?.Invoke();
    }

    public void MarkAsRead(Guid id)
    {
        lock (_lock)
        {
            _readIds.Add(id);
        }
        StateChanged?.Invoke();
    }

    public void MarkAllAsRead()
    {
        lock (_lock)
        {
            foreach (var m in _items)
                _readIds.Add(m.Id);
        }
        StateChanged?.Invoke();
    }

    public bool IsRead(Guid id)
    {
        lock (_lock)
        {
            return _readIds.Contains(id);
        }
    }
}

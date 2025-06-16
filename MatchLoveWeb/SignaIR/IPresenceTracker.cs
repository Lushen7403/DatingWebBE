public interface IPresenceTracker
{
    Task UserConnected(int userId, string connectionId);
    Task UserDisconnected(int userId, string connectionId);
    bool IsOnline(int userId);
}

public class PresenceTracker : IPresenceTracker
{
    // key: userId, value: set of connectionIds
    private readonly Dictionary<int, HashSet<string>> _onlineUsers = new();

    public Task UserConnected(int userId, string connectionId)
    {
        lock (_onlineUsers)
        {
            if (!_onlineUsers.TryGetValue(userId, out var conns))
            {
                conns = new HashSet<string>();
                _onlineUsers[userId] = conns;
            }
            conns.Add(connectionId);
        }
        return Task.CompletedTask;
    }

    public Task UserDisconnected(int userId, string connectionId)
    {
        lock (_onlineUsers)
        {
            if (_onlineUsers.TryGetValue(userId, out var conns))
            {
                conns.Remove(connectionId);
                if (conns.Count == 0)
                    _onlineUsers.Remove(userId);
            }
        }
        return Task.CompletedTask;
    }

    public bool IsOnline(int userId)
    {
        lock (_onlineUsers)
        {
            return _onlineUsers.ContainsKey(userId);
        }
    }
}

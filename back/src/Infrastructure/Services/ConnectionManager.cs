using System.Collections.Concurrent;

namespace Infrastructure.Services;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<int, HashSet<string>> _userConnections = new();
    private readonly ConcurrentDictionary<string, int> _connectionUser = new();

    public void AddConnection(int userId, string connectionId)
    {
        _connectionUser[connectionId] = userId;
        _userConnections.AddOrUpdate(
            userId,
            _ => new HashSet<string> { connectionId },
            (_, set) =>
            {
                lock (set) { set.Add(connectionId); }
                return set;
            });
    }

    public void RemoveConnection(string connectionId)
    {
        if (!_connectionUser.TryRemove(connectionId, out var userId)) return;

        if (_userConnections.TryGetValue(userId, out var set))
        {
            lock (set) { set.Remove(connectionId); }
            if (set.Count == 0)
                _userConnections.TryRemove(userId, out _);
        }
    }

    public List<string> GetConnections(int userId)
    {
        return _userConnections.TryGetValue(userId, out var set)
            ? [.. set]
            : [];
    }

    public bool IsUserOnline(int userId)
    {
        return _userConnections.TryGetValue(userId, out var set) && set.Count > 0;
    }

    public int? GetUserId(string connectionId)
    {
        return _connectionUser.TryGetValue(connectionId, out var userId) ? userId : null;
    }
}

using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public sealed class InMemorySubscriptionStore
{
    private readonly object _sync = new();
    private readonly List<Subscription> _subscriptions = [];

    public bool TryAdd(string? url, out Subscription? added)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            added = null;
            return false;
        }

        var subscription = new Subscription(url);

        lock (_sync)
        {
            _subscriptions.Add(subscription);
            added = subscription;
        }

        return true;
    }

    public IReadOnlyList<Subscription> GetAll()
    {
        lock (_sync)
        {
            return _subscriptions.ToArray();
        }
    }
}
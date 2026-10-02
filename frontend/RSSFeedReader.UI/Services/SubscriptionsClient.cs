using System.Net.Http.Json;
using RSSFeedReader.UI.Models;

namespace RSSFeedReader.UI.Services;

public sealed class SubscriptionsClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<Subscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<Subscription[]>("subscriptions", cancellationToken) ?? [];
    }

    public async Task<Subscription> AddAsync(string url, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("subscriptions", new Subscription(url), cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Subscription>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty subscription response.");
    }
}
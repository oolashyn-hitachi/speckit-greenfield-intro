using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RSSFeedReader.Api.Tests;

public class SubscriptionsApiTests : IDisposable
{
    private const string SubscriptionsPath = "/api/subscriptions";
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public SubscriptionsApiTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_returns_an_empty_array_at_start()
    {
        using var response = await _client.GetAsync(SubscriptionsPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var subscriptions = await response.Content.ReadFromJsonAsync<SubscriptionResponse[]>();
        Assert.NotNull(subscriptions);
        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task Post_then_get_returns_the_submitted_subscription()
    {
        const string url = "https://example.com/feed.xml";

        using var postResponse = await PostSubscriptionAsync(url);

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        var created = await postResponse.Content.ReadFromJsonAsync<SubscriptionResponse>();
        Assert.NotNull(created);
        Assert.Equal(url, created.Url);

        using var getResponse = await _client.GetAsync(SubscriptionsPath);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var subscriptions = await getResponse.Content.ReadFromJsonAsync<SubscriptionResponse[]>();
        Assert.NotNull(subscriptions);
        Assert.Equal(new[] { url }, subscriptions.Select(subscription => subscription.Url));
    }

    [Fact]
    public async Task Get_preserves_append_order()
    {
        var submittedUrls = new[]
        {
            "https://example.com/first",
            "https://example.com/second",
            "https://example.com/third"
        };

        foreach (var url in submittedUrls)
        {
            using var response = await PostSubscriptionAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var getResponse = await _client.GetAsync(SubscriptionsPath);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var subscriptions = await getResponse.Content.ReadFromJsonAsync<SubscriptionResponse[]>();
        Assert.NotNull(subscriptions);
        Assert.Equal(submittedUrls, subscriptions.Select(subscription => subscription.Url));
    }

    [Fact]
    public async Task Get_preserves_duplicate_submissions()
    {
        const string url = "https://example.com/repeated";

        for (var index = 0; index < 2; index++)
        {
            using var response = await PostSubscriptionAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var getResponse = await _client.GetAsync(SubscriptionsPath);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var subscriptions = await getResponse.Content.ReadFromJsonAsync<SubscriptionResponse[]>();
        Assert.NotNull(subscriptions);
        Assert.Equal(new[] { url, url }, subscriptions.Select(subscription => subscription.Url));
    }

    [Fact]
    public async Task Post_preserves_an_arbitrary_non_url_string_unchanged()
    {
        const string submittedValue = "  this is not a URL\t";

        using var postResponse = await PostSubscriptionAsync(submittedValue);

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        var created = await postResponse.Content.ReadFromJsonAsync<SubscriptionResponse>();
        Assert.NotNull(created);
        Assert.Equal(submittedValue, created.Url);

        using var getResponse = await _client.GetAsync(SubscriptionsPath);
        var subscriptions = await getResponse.Content.ReadFromJsonAsync<SubscriptionResponse[]>();
        Assert.NotNull(subscriptions);
        Assert.Equal(new[] { submittedValue }, subscriptions.Select(subscription => subscription.Url));
    }

    [Fact]
    public async Task Post_returns_bad_request_when_url_is_missing()
    {
        using var content = JsonContent.Create(new { });
        using var response = await _client.PostAsync(SubscriptionsPath, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_returns_bad_request_when_url_is_null()
    {
        using var response = await PostSubscriptionAsync(null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    public async Task Post_returns_bad_request_when_url_is_empty_or_whitespace(string url)
    {
        using var response = await PostSubscriptionAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_returns_bad_request_for_malformed_json()
    {
        using var content = new StringContent("{ invalid", Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(SubscriptionsPath, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private Task<HttpResponseMessage> PostSubscriptionAsync(string? url) =>
        _client.PostAsJsonAsync(SubscriptionsPath, new { url });

    private sealed record SubscriptionResponse(string Url);
}
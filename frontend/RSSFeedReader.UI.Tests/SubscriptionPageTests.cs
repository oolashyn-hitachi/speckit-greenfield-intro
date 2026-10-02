using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RSSFeedReader.UI.Pages;
using RSSFeedReader.UI.Services;

namespace RSSFeedReader.UI.Tests;

public class SubscriptionPageTests
{
    [Fact]
    public void EmptyListShowsEmptyState()
    {
        var handler = new InMemorySubscriptionsHandler();
        using var context = CreateContext(handler);

        var page = context.RenderComponent<Subscriptions>();

        Assert.Contains("no subscriptions", page.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(page.FindAll("li"));
    }

    [Fact]
    public async Task WhitespaceSubmissionShowsPromptWithoutPosting()
    {
        var handler = new InMemorySubscriptionsHandler();
        using var context = CreateContext(handler);
        var page = context.RenderComponent<Subscriptions>();

        page.Find("input").Change("   ");
        await page.Find("form").SubmitAsync();

        Assert.Equal(0, handler.PostCount);
        Assert.Contains("enter", page.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("URL", page.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(page.FindAll("li"));
    }

    [Fact]
    public async Task SuccessfulAddAppearsImmediately()
    {
        var handler = new InMemorySubscriptionsHandler();
        using var context = CreateContext(handler);
        var page = context.RenderComponent<Subscriptions>();

        await AddValue(page, "https://feeds.example.test/first.xml");

        Assert.Equal(1, handler.PostCount);
        Assert.Equal("https://feeds.example.test/first.xml", Assert.Single(page.FindAll("li")).TextContent);
    }

    [Fact]
    public async Task AddingValueRetainsExistingEntries()
    {
        var handler = new InMemorySubscriptionsHandler(
            initialValues: ["https://feeds.example.test/existing.xml"]);
        using var context = CreateContext(handler);
        var page = context.RenderComponent<Subscriptions>();

        await AddValue(page, "https://feeds.example.test/new.xml");

        Assert.Equal(
            ["https://feeds.example.test/existing.xml", "https://feeds.example.test/new.xml"],
            page.FindAll("li").Select(item => item.TextContent));
    }

    [Fact]
    public async Task DuplicateSubmissionsRenderAsSeparateEntries()
    {
        var handler = new InMemorySubscriptionsHandler();
        using var context = CreateContext(handler);
        var page = context.RenderComponent<Subscriptions>();

        await AddValue(page, "https://feeds.example.test/repeated.xml");
        await AddValue(page, "https://feeds.example.test/repeated.xml");

        Assert.Equal(2, handler.PostCount);
        Assert.Equal(
            ["https://feeds.example.test/repeated.xml", "https://feeds.example.test/repeated.xml"],
            page.FindAll("li").Select(item => item.TextContent));
    }

    [Fact]
    public async Task FailedAddShowsFeedbackAndLeavesListUnchanged()
    {
        var handler = new InMemorySubscriptionsHandler(
            failPosts: true,
            initialValues: ["https://feeds.example.test/existing.xml"]);
        using var context = CreateContext(handler);
        var page = context.RenderComponent<Subscriptions>();

        await AddValue(page, "https://feeds.example.test/rejected.xml");

        Assert.Equal(1, handler.PostCount);
        Assert.Equal("https://feeds.example.test/existing.xml", Assert.Single(page.FindAll("li")).TextContent);
        Assert.True(
            page.Markup.Contains("failed", StringComparison.OrdinalIgnoreCase)
                || page.Markup.Contains("could not", StringComparison.OrdinalIgnoreCase),
            "Expected visible feedback explaining that the add failed.");
    }

    private static TestContext CreateContext(InMemorySubscriptionsHandler handler)
    {
        var context = new TestContext();
        context.Services.AddSingleton(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/api/")
        });
        context.Services.AddScoped(serviceProvider =>
            new SubscriptionsClient(serviceProvider.GetRequiredService<HttpClient>()));
        return context;
    }

    private static async Task AddValue(IRenderedComponent<Subscriptions> page, string value)
    {
        page.Find("input").Change(value);
        await page.Find("form").SubmitAsync();
    }

    private sealed class InMemorySubscriptionsHandler : HttpMessageHandler
    {
        private readonly List<string> subscriptions;
        private readonly bool failPosts;

        public InMemorySubscriptionsHandler(bool failPosts = false, params string[] initialValues)
        {
            this.failPosts = failPosts;
            subscriptions = [.. initialValues];
        }

        public int PostCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.TrimEnd('/') != "/api/subscriptions")
            {
                throw new InvalidOperationException($"Unexpected request URI: {request.RequestUri}");
            }

            if (request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(subscriptions.Select(url => new { url }).ToArray())
                };
            }

            if (request.Method == HttpMethod.Post)
            {
                PostCount++;
                if (failPosts)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
                var url = body.RootElement.GetProperty("url").GetString()!;
                subscriptions.Add(url);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new { url })
                };
            }

            throw new InvalidOperationException($"Unexpected HTTP method: {request.Method}");
        }
    }
}
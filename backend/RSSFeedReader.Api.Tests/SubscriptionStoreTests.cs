using RSSFeedReader.Api.Models;
using RSSFeedReader.Api.Services;

namespace RSSFeedReader.Api.Tests;

public class SubscriptionStoreTests
{
    [Fact]
    public void GetAll_WhenEmpty_ReturnsNoSubscriptions()
    {
        var store = new InMemorySubscriptionStore();

        Assert.Empty(store.GetAll());
    }

    [Fact]
    public void TryAdd_PreservesInsertionOrder()
    {
        var store = new InMemorySubscriptionStore();

        Assert.True(store.TryAdd("first", out _));
        Assert.True(store.TryAdd("second", out _));
        Assert.True(store.TryAdd("third", out _));

        Assert.Equal(new[] { "first", "second", "third" }, store.GetAll().Select(item => item.Url));
    }

    [Fact]
    public void TryAdd_PreservesDuplicates()
    {
        var store = new InMemorySubscriptionStore();

        Assert.True(store.TryAdd("https://example.test/feed", out _));
        Assert.True(store.TryAdd("https://example.test/feed", out _));

        Assert.Equal(
            new[] { "https://example.test/feed", "https://example.test/feed" },
            store.GetAll().Select(item => item.Url));
    }

    [Fact]
    public void TryAdd_PreservesArbitraryNonWhitespaceValuesExactly()
    {
        var store = new InMemorySubscriptionStore();
        var values = new[]
        {
            "  https://example.test/feed  ",
            "text\tinside\nline",
            " \tvalue\r\n"
        };

        foreach (var value in values)
        {
            Assert.True(store.TryAdd(value, out var added));
            Assert.NotNull(added);
            Assert.Equal(value, added.Url);
        }

        Assert.Equal(values, store.GetAll().Select(item => item.Url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void TryAdd_RejectsNullOrWhitespaceWithoutChangingState(string? value)
    {
        var store = new InMemorySubscriptionStore();
        Assert.True(store.TryAdd("existing", out _));
        var before = store.GetAll().Select(item => item.Url).ToArray();

        Assert.False(store.TryAdd(value, out var added));

        Assert.Null(added);
        Assert.Equal(before, store.GetAll().Select(item => item.Url));
    }

    [Fact]
    public void TryAdd_ParallelAddsAreNotLostAndCanBeListedConcurrently()
    {
        const int itemCount = 500;
        var store = new InMemorySubscriptionStore();
        var expected = Enumerable.Range(0, itemCount)
            .Select(index => $"https://example.test/feed/{index}")
            .ToArray();

        Parallel.ForEach(expected, value =>
        {
            Assert.True(store.TryAdd(value, out var added));
            Assert.NotNull(added);
            Assert.Equal(value, added.Url);

            var snapshot = store.GetAll();
            Assert.All(snapshot, item => Assert.False(string.IsNullOrWhiteSpace(item.Url)));
        });

        var actual = store.GetAll().Select(item => item.Url).ToArray();
        Assert.Equal(itemCount, actual.Length);
        Assert.Equal(expected.OrderBy(value => value), actual.OrderBy(value => value));
    }
}
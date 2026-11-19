using Xunit;

namespace IndependentWork7.Tests;

public class CacheTests
{
    [Fact]
    public void AddStoresEntries()
    {
        var cache = new Cache<int, string>(2);
        cache.Add(1, "A");
        cache.Add(2, "B");
        Assert.True(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
    }

    [Fact]
    public void GetReturnsStoredValue()
    {
        var cache = new Cache<int, string>(1);
        cache.Add(7, "A");
        Assert.Equal("A", cache.Get(7));
    }

    [Fact]
    public void GetThrowsForMissingKey()
    {
        var cache = new Cache<int, string>(1);
        Assert.Throws<KeyNotFoundException>(() => cache.Get(7));
    }

    [Fact]
    public void ContainsKeyDistinguishesPresentAndMissingKeys()
    {
        var cache = new Cache<int, string>(1);
        cache.Add(1, "A");
        Assert.True(cache.ContainsKey(1));
        Assert.False(cache.ContainsKey(2));
    }

    [Fact]
    public void CountDoesNotExceedCapacity()
    {
        var cache = new Cache<int, string>(1);
        Assert.Equal(0, cache.Count);
        cache.Add(1, "A");
        cache.Add(2, "B");
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void OverflowEvictsOldestEvenAfterReadingIt()
    {
        var cache = new Cache<int, string>(2);
        cache.Add(1, "A");
        cache.Add(2, "B");
        Assert.Equal("A", cache.Get(1));
        cache.Add(3, "C");
        Assert.False(cache.ContainsKey(1));
        Assert.Equal("B", cache.Get(2));
        Assert.Equal("C", cache.Get(3));
    }

    [Fact]
    public void UpdatingKeyPreservesItsFifoPosition()
    {
        var cache = new Cache<int, string>(2);
        cache.Add(1, "A");
        cache.Add(2, "B");
        cache.Add(1, "New");
        Assert.Equal("New", cache.Get(1));
        Assert.Equal(2, cache.Count);
        cache.Add(3, "C");
        Assert.False(cache.ContainsKey(1));
    }
}

using Argx.Store;

namespace Argx.Tests.Parsing;

public class ArgumentRepositoryTests
{
    [Fact]
    public void Set_ShouldAddKeyValue_WhenNewKey()
    {
        var repo = new ArgumentRepository();

        repo.Set("foo", "bar");

        Assert.True(repo.TryGetValue("foo", out var value));
        Assert.Equal("bar", value);
    }

    [Fact]
    public void Set_ShouldUpdateKeyValue_WhenExistingKey()
    {
        var repo = new ArgumentRepository();

        repo.Set("foo", "bar");
        repo.Set("foo", "baz");

        Assert.True(repo.TryGetValue("foo", out var value));
        Assert.Equal("baz", value);
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseAndOutNull_WhenNotExists()
    {
        var repo = new ArgumentRepository();
        repo.Set("foo", "bar");

        var result = repo.TryGetValue("baz", out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrueAndOutValue_WhenExists()
    {
        var repo = new ArgumentRepository();
        repo.Set("foo", "bar");

        var result = repo.TryGetValue("foo", out var value);

        Assert.True(result);
        Assert.Equal("bar", value);
    }

    [Theory]
    [InlineData(21, "21")]
    [InlineData(false, "False")]
    public void TryGetValue_ShouldOutString_WhenValueNotString(object obj, string expected)
    {
        var repo = new ArgumentRepository();
        repo.Set("foo", obj);

        _ = repo.TryGetValue("foo", out var actual);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("foo", "Foo")]
    [InlineData("Foo", "foo")]
    [InlineData("FOO", "Foo")]
    [InlineData("FOO", "foo")]
    public void TryGetValue_ShouldBeCaseInsensitive(string set, string get)
    {
        var repo = new ArgumentRepository();
        repo.Set(set, "bar");

        var result = repo.TryGetValue(get, out _);

        Assert.True(result);
    }

    [Fact]
    public void TryGetValueT_ShouldReturnTrueAndOutValue_WhenExistsAndValueTypeT()
    {
        var repo = new ArgumentRepository();
        repo.Set("foo", 21);

        var result = repo.TryGetValue<int>("foo", out var value);

        Assert.True(result);
        Assert.Equal(21, value);
    }

    [Fact]
    public void TryGetValueT_ShouldReturnFalseAndOutDefault_WhenNotExists()
    {
        var repo = new ArgumentRepository();

        var result = repo.TryGetValue<int>("foo", out var value);

        Assert.False(result);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGetValueT_ShouldReturnFalseAndOutDefault_WhenExistsButNotTypeT()
    {
        var repo = new ArgumentRepository();
        repo.Set("foo", "bar");

        var result = repo.TryGetValue<int>("foo", out var value);

        Assert.False(result);
        Assert.Equal(0, value);
    }
}
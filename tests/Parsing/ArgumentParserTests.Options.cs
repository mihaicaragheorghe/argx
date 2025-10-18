using Argx.Actions;
using Argx.Errors;
using Argx.Parsing;
using Argx.Store;

using Moq;

namespace Argx.Tests.Parsing;

public partial class ArgumentParserTests
{
    [Fact]
    public void AddFlag_ShouldAddStoreTrueOption_WhenValueIsTrue()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);

        parser.AddFlag("--foo", ["-f"], value: true);
        _ = parser.ParseInternal(["--foo"]);

        repository.Verify(x => x.Set("foo", true));
    }

    [Fact]
    public void AddFlag_ShouldAddStoreFalseOption_WhenValueIsFalse()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);

        parser.AddFlag("--foo", ["-f"], value: false);
        _ = parser.ParseInternal(["--foo"]);

        repository.Verify(x => x.Set("foo", false));
    }

    [Fact]
    public void AddFlag_ShouldThrowInvalidOperationException_WhenNotValidOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddFlag("foo"));
    }

    [Fact]
    public void AddOption_ShouldAddOption_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);

        parser.AddOption("--foo");
        _ = parser.ParseInternal(["--foo", "bar"]);

        repository.Verify(x => x.Set("foo", "bar"));
    }

    [Fact]
    public void AddOption_ShouldThrowInvalidOperationException_WhenNotValidOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddOption("foo"));
    }

    [Fact]
    public void AddOptionT_ShouldThrowInvalidOperationException_WhenNotValidOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddOption<int>("foo"));
    }

    [Fact]
    public void AddOptionT_ShouldAddOption_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);

        parser.AddOption<int>("--foo");
        _ = parser.ParseInternal(["--foo", "69"]);

        repository.Verify(x => x.Set("foo", 69));
    }

    [Fact]
    public void Parse_ShouldStoreOption_WhenValid()
    {
        var parser = new ArgumentParser();
        parser.Add("--input");

        var result = parser.ParseInternal(["--input", "foo.txt"]);

        Assert.Equal("foo.txt", result["input"]);
    }

    [Fact]
    public void Parse_ShouldStoreOptions_WhenValid()
    {
        var parser = new ArgumentParser();
        parser.Add("--foo");
        parser.Add("--bar");

        var result = parser.ParseInternal(["--foo", "bar", "--bar", "baz"]);

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("baz", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreOptionToDest_WhenHasDest()
    {
        var parser = new ArgumentParser();
        parser.Add("--input", dest: "file");

        var result = parser.ParseInternal(["--input", "foo.txt"]);

        Assert.Equal("foo.txt", result["file"]);
    }

    [Fact]
    public void Parse_ShouldStoreOptionT_WhenValid()
    {
        var repo = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repo.Object);
        parser.Add<int>("--foo");

        _ = parser.ParseInternal(["--foo", "21"]);

        repo.Verify(x => x.Set("foo", 21));
    }

    [Fact]
    public void Parse_ShouldThrowArgumentValueException_WhenOptionConversionFails()
    {
        var parser = new ArgumentParser();
        parser.Add<int>("--foo");

        var ex = Assert.Throws<ArgumentValueException>(() => parser.ParseInternal(["--foo", "bar"]));
        Assert.Equal("Error: argument --foo: expected type int. Failed to convert 'bar' to int", ex.Message);
    }

    [Fact]
    public void Parse_ShouldAddToExtras_WhenOptionNotRegistered()
    {
        var parser = new ArgumentParser();
        parser.Add("--foo", ["-f"]);

        var result = parser.ParseInternal(["--foo", "bar", "--baz", "qux"]);

        Assert.Null(result["baz"]);
        Assert.Contains("--baz", result.Extras);
    }

    [Fact]
    public void Parse_ShouldParseFixedTokensLength_WhenArityIsFixed()
    {
        var parser = new ArgumentParser();
        string[] expected = ["bar", "baz", "qux"];
        parser.Add<string[]>("--foo", arity: "3", action: ArgumentActions.Store);

        var result = parser.ParseInternal(["--foo", "bar", "baz", "qux", "--extra", "quux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(expected, actual);
        Assert.Contains("--extra", result.Extras);
        Assert.Contains("quux", result.Extras);
    }

    [Fact]
    public void Parse_ShouldParseNextArgumentToken_WhenExistsAndArityOptional()
    {
        var parser = new ArgumentParser();
        parser.Add<string>("--foo", arity: Arity.Optional, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["--foo", "bar", "baz", "qux"]);

        Assert.True(result.TryGetValue<string>("foo", out var actual));
        Assert.Equal("bar", actual);
    }

    [Fact]
    public void Parse_ShouldUseConstValue_WhenArityOptionalAndNoNextArgumentToken()
    {
        var parser = new ArgumentParser();
        parser.Add<string>("--foo", arity: Arity.Optional, action: ArgumentActions.Store, constValue: "bar");

        var result = parser.ParseInternal(["--foo", "--baz", "qux"]);

        Assert.True(result.TryGetValue<string>("foo", out var actual));
        Assert.Equal("bar", actual);
    }

    [Fact]
    public void Parse_ShouldParseAllFollowingArgumentTokens_WhenArityAny()
    {
        var parser = new ArgumentParser();
        parser.Add<string[]>("--foo", arity: Arity.Any, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["--foo", "bar", "baz", "--qux", "quux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(["bar", "baz"], actual);
    }

    [Fact]
    public void Parse_ShouldUseConst_WhenArityAnyAndNoFollowingArgumentTokens()
    {
        var parser = new ArgumentParser();
        string[] expected = ["barbar"];
        parser.Add<string[]>("--foo", arity: Arity.Any, action: ArgumentActions.Store, constValue: expected);

        var result = parser.ParseInternal(["--foo", "--bar", "baz", "qux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parse_ShouldParseAllFollowingArgumentTokens_WhenArityAtLeastOne()
    {
        var parser = new ArgumentParser();
        string[] expected = ["bar", "baz"];
        parser.Add<string[]>("--foo", arity: Arity.AtLeastOne, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["--foo", "bar", "baz", "--qux", "quux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parse_ShouldThrowInvalidOperationException_WhenArityAtLeastOneAndNoFollowingArgumentTokens()
    {
        var parser = new ArgumentParser();
        parser.Add<string[]>("--foo", arity: Arity.AtLeastOne, action: ArgumentActions.Store);

        var ex = Assert.Throws<ArgumentValueException>(() =>
            parser.ParseInternal(["--foo", "--bar", "baz", "qux", "quux"]));
        Assert.Equal("Error: argument --foo: requires at least one value", ex.Message);
    }

    [Fact]
    public void Parse_ShouldThrowArgumentValueException_WhenPositionalArityExceedsTokensLength()
    {
        var parser = new ArgumentParser();
        parser.Add<string[]>("foo", arity: "3", action: ArgumentActions.Store);

        var ex = Assert.Throws<ArgumentValueException>(() => parser.ParseInternal(["foo", "bar"]));
        Assert.Equal("Error: argument foo: not enough values provided", ex.Message);
    }
}
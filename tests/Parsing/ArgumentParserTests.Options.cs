using Argx.Actions;
using Argx.Errors;
using Argx.Parsing;
using Argx.Store;

using Moq;

namespace Argx.Tests.Parsing;

public partial class ArgumentParserTests
{
    [Fact]
    public void AddFlag_ShouldAddFlagOption_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddFlag("--foo", ["-f"]);
        _ = parser.ParseInternal(["--foo"]);

        repository.Verify(x => x.Set("foo", true));
    }

    [Fact]
    public void AddFlag_ShouldAddFalseFlagOption_WhenValueIsFalse()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddFlag("--foo", ["-f"], value: false);
        _ = parser.ParseInternal(["--foo"]);

        repository.Verify(x => x.Set("foo", false));
    }

    [Fact]
    public void AddFlag_ShouldThrowInvalidOperationException_WhenArgIsNotOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddArgument("--foo"));
    }

    [Fact]
    public void AddOption_ShouldAddOptionalArgument_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddOption("--foo");
        _ = parser.ParseInternal(["--foo", "bar"]);

        repository.Verify(x => x.Set("foo", "bar"));
    }

    [Fact]
    public void AddOption_ShouldThrowInvalidOperationException_WhenArgIsNotOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddOption("foo"));
    }

    [Fact]
    public void AddOptionT_ShouldThrowInvalidOperationException_WhenArgIsOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddOption<int>("foo"));
    }

    [Fact]
    public void AddOptionT_ShouldAddOptionalArgument_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddOption<int>("--foo");
        _ = parser.ParseInternal(["--foo", "69"]);

        repository.Verify(x => x.Set("foo", 69));
    }

    [Fact]
    public void Parse_ShouldStoreArg_WhenValidOption()
    {
        var parser = new ArgumentParser();
        parser.Add("--input");

        var result = parser.ParseInternal(["--input", "foo.txt"]);

        Assert.Equal("foo.txt", result["input"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgs_WhenValidOptions()
    {
        var parser = new ArgumentParser();
        parser.Add("--foo");
        parser.Add("--bar");

        var result = parser.ParseInternal(["--foo", "bar", "--bar", "baz"]);

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("baz", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgToDest_WhenArgHasDest()
    {
        var parser = new ArgumentParser();
        parser.Add("--input", dest: "file");

        var result = parser.ParseInternal(["--input", "foo.txt"]);

        Assert.Equal("foo.txt", result["file"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgT_WhenValidOptionT()
    {
        var repo = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repo.Object);
        parser.Add<int>("--foo");

        _ = parser.ParseInternal(["--foo", "21"]);

        repo.Verify(x => x.Set("foo", 21));
    }

    [Fact]
    public void Parse_ShouldThrowInvalidCastException_WhenOptionConversionFails()
    {
        var parser = new ArgumentParser();
        parser.Add<int>("--foo");

        Assert.Throws<ArgumentValueException>(() => parser.ParseInternal(["--foo", "bar"]));
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
    public void Parse_ShouldSendCorrectTokensToAction_WhenArityIsFixed()
    {
        var parser = new ArgumentParser();
        string[] expected = ["bar", "baz", "qux"];
        parser.Add<string[]>("--foo", arity: "3", action: ArgumentActions.Store);

        var result = parser.ParseInternal(["--foo", "bar", "baz", "qux", "--extra"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(expected, actual);
        Assert.Contains("--extra", result.Extras);
    }

    [Fact]
    public void Parse_ShouldParseValue_WhenArityOptional()
    {
        var parser = new ArgumentParser();
        parser.Add<string>("--foo", arity: Arity.Optional, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["--foo", "bar", "baz", "qux"]);

        Assert.True(result.TryGetValue<string>("foo", out var actual));
        Assert.Equal("bar", actual);
    }

    [Fact]
    public void Parse_ShouldNotParseValue_WhenArityOptionalAndNoValue()
    {
        var parser = new ArgumentParser();
        parser.Add<string>("--foo", arity: Arity.Optional, action: ArgumentActions.Store, constValue: "bar");

        var result = parser.ParseInternal(["--foo", "--bar", "baz"]);

        Assert.True(result.TryGetValue<string>("foo", out var actual));
        Assert.Equal("bar", actual);
    }

    [Fact]
    public void Parse_ShouldParseAllValues_WhenArityAny()
    {
        var parser = new ArgumentParser();
        parser.Add<string[]>("--foo", arity: Arity.Any, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["--foo", "bar", "baz", "--qux", "quux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(["bar", "baz"], actual);
    }

    [Fact]
    public void Parse_ShouldNotParseValues_WhenArityAnyAndNoValues()
    {
        var parser = new ArgumentParser();
        string[] expected = ["barbar"];
        parser.Add<string[]>("--foo", arity: Arity.Any, action: ArgumentActions.Store, constValue: expected);

        var result = parser.ParseInternal(["--foo", "--bar", "baz", "qux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parse_ShouldParseAllValues_WhenArityAtLeastOne()
    {
        var parser = new ArgumentParser();
        string[] expected = ["bar", "baz"];
        parser.Add<string[]>("--foo", arity: Arity.AtLeastOne, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["--foo", "bar", "baz", "--qux", "quux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parse_ShouldThrowInvalidOperationException_WhenArityAtLeastOneAndNoValueProvided()
    {
        var parser = new ArgumentParser();
        parser.Add<string[]>("--foo", arity: Arity.AtLeastOne, action: ArgumentActions.Store);

        Assert.Throws<ArgumentValueException>(() => parser.ParseInternal(["--foo", "--bar", "baz", "qux", "quux"]));
    }
}
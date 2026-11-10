using Argx.Actions;
using Argx.Parsing;

namespace Argx.Tests.Parsing;

public class CollectionRegistrationTests
{
    [Fact]
    public void Parse_ShouldAppendCustomConstants_ForRegisteredCollectionShapes()
    {
        AssertAppendConstants<DayOfWeek[]>();
        AssertAppendConstants<IEnumerable<DayOfWeek>>();
        AssertAppendConstants<IList<DayOfWeek>>();
        AssertAppendConstants<ICollection<DayOfWeek>>();
        AssertAppendConstants<List<DayOfWeek>>();
    }

    private static void AssertAppendConstants<TCollection>()
    {
        var configuration = new ArgumentParserConfiguration();
        configuration.Collections.Register<DayOfWeek>();
        var parser = new ArgumentParser(configuration: configuration);
        parser.AddOption<TCollection>("--monday", action: ArgumentActions.Append,
            arity: Arity.Optional, constValue: DayOfWeek.Monday);

        var result = parser.Parse("--monday", "--monday");

        Assert.Equal([DayOfWeek.Monday, DayOfWeek.Monday],
            Assert.IsAssignableFrom<IEnumerable<DayOfWeek>>(result.GetRequired<TCollection>("monday")));
    }

    [Fact]
    public void Parse_ShouldAppendEnumerableCustomConstants()
    {
        var configuration = new ArgumentParserConfiguration();
        configuration.Collections.Register<DayOfWeek>();
        var parser = new ArgumentParser(configuration: configuration);
        parser.AddOption<DayOfWeek[]>("--days", action: ArgumentActions.Append,
            arity: Arity.Optional, constValue: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday });

        var result = parser.Parse("--days", "--days");

        Assert.Equal([DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Monday, DayOfWeek.Tuesday],
            result.GetRequired<DayOfWeek[]>("days"));
    }

    [Fact]
    public void Parse_ShouldStoreCustomCollectionConstants()
    {
        var configuration = new ArgumentParserConfiguration();
        configuration.Collections.Register<DayOfWeek>();
        var parser = new ArgumentParser(configuration: configuration);
        var days = new List<DayOfWeek> { DayOfWeek.Monday };
        parser.AddOption<List<DayOfWeek>>("--days", arity: Arity.Optional, constValue: days);

        var result = parser.Parse("--days");

        Assert.Same(days, result.GetRequired<List<DayOfWeek>>("days"));
    }

    [Fact]
    public void Parse_ShouldAppendCustomReferenceTypeConstants()
    {
        var configuration = new ArgumentParserConfiguration();
        configuration.Collections.Register<Uri>();
        var parser = new ArgumentParser(configuration: configuration);
        var uri = new Uri("https://example.com");
        parser.AddOption<List<Uri>>("--uri", action: ArgumentActions.Append,
            arity: Arity.Optional, constValue: uri);

        var result = parser.Parse("--uri", "--uri");

        Assert.Equal(new[] { uri, uri }, result.GetRequired<List<Uri>>("uri"));
    }

    [Theory]
    [InlineData(ArgumentActions.Store)]
    [InlineData(ArgumentActions.Append)]
    public void AddOption_ShouldRejectUnregisteredElementTypes(string action)
    {
        var parser = new ArgumentParser();

        var exception = Assert.Throws<ArgumentException>(() =>
            parser.AddOption<DayOfWeek[]>("--days", action: action,
                arity: Arity.Optional, constValue: new[] { DayOfWeek.Monday }));

        Assert.Contains("Collections.Register<TElement>()", exception.Message);
    }

    [Fact]
    public void Register_ShouldNotAffectOtherConfigurations()
    {
        var configuration = new ArgumentParserConfiguration();
        configuration.Collections.Register<DayOfWeek>();
        var configuredParser = new ArgumentParser(configuration: configuration);
        configuredParser.AddOption<DayOfWeek[]>("--days", action: ArgumentActions.Append,
            arity: Arity.Optional, constValue: DayOfWeek.Monday);

        var otherParser = new ArgumentParser();

        Assert.Throws<ArgumentException>(() => otherParser.AddOption<DayOfWeek[]>("--days",
            action: ArgumentActions.Append, arity: Arity.Optional, constValue: DayOfWeek.Monday));
    }

    [Fact]
    public void Parse_ShouldNotEnableTokenConversion_WhenCollectionIsRegistered()
    {
        var configuration = new ArgumentParserConfiguration();
        configuration.Collections.Register<DayOfWeek>();
        var parser = new ArgumentParser(configuration: configuration);
        parser.AddOption<DayOfWeek[]>("--days");

        Assert.Throws<NotSupportedException>(() => parser.Parse("--days", "Monday"));
    }

    [Fact]
    public void Parse_ShouldUseBuiltInFactories_ForStoreAndAppend()
    {
        var parser = new ArgumentParser();
        parser.AddOption<List<int?>>("--values", arity: Arity.AtLeastOne);
        parser.AddOption<int?[]>("--items", action: ArgumentActions.Append);

        var result = parser.Parse("--values", "1", "2", "--items", "3", "--items", "4");

        Assert.Equal(new int?[] { 1, 2 }, result.GetRequired<List<int?>>("values"));
        Assert.Equal([3, 4], result.GetRequired<int?[]>("items"));
    }
}

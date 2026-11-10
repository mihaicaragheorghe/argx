using Argx.Actions;
using Argx.Parsing;

var configuration = new ArgumentParserConfiguration();
configuration.Collections.Register<DayOfWeek>();
configuration.Collections.Register<DayOfWeek?>();
configuration.Collections.Register<Uri>();

var parser = new ArgumentParser(configuration: configuration);
parser.AddOption<int>("--number");
parser.AddOption<int?>("--nullable");
parser.AddOption<int[]>("--values", arity: "+");
parser.AddOption<List<int>>("--items", action: ArgumentActions.Append);
parser.AddFlag("--enabled");
parser.AddOption<DayOfWeek[]>("--monday", action: ArgumentActions.Append,
    arity: "?", constValue: DayOfWeek.Monday);
parser.AddOption<List<DayOfWeek?>>("--tuesday", action: ArgumentActions.Append,
    arity: "?", constValue: DayOfWeek.Tuesday);
var uri = new Uri("https://example.com");
parser.AddOption<ICollection<Uri>>("--uri", action: ArgumentActions.Append,
    arity: "?", constValue: uri);

var result = parser.Parse("--number", "42", "--nullable", "7", "--values", "1", "2", "--items", "3", "--items", "4", "--enabled",
    "--monday", "--monday", "--tuesday", "--tuesday", "--uri", "--uri");

if (result.GetRequired<int>("number") != 42 || result.GetRequired<int?>("nullable") != 7 ||
    !result.GetRequired<int[]>("values").SequenceEqual(new[] { 1, 2 }) ||
    !result.GetRequired<List<int>>("items").SequenceEqual(new[] { 3, 4 }) || !result.GetRequired<bool>("enabled") ||
    !result.GetRequired<DayOfWeek[]>("monday").SequenceEqual(new[] { DayOfWeek.Monday, DayOfWeek.Monday }) ||
    !result.GetRequired<List<DayOfWeek?>>("tuesday").SequenceEqual(new DayOfWeek?[] { DayOfWeek.Tuesday, DayOfWeek.Tuesday }) ||
    !result.GetRequired<ICollection<Uri>>("uri").SequenceEqual(new[] { uri, uri }))
{
    throw new Exception("AOT parsing failed");
}

Console.WriteLine("AOT smoke passed");

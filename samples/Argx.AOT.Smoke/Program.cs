using Argx;
using Argx.Attributes;
using Argx.Extensions;
using Argx.Actions;
using Argx.Binding;
using Argx.Parsing;

var parser = new ArgumentParser();
parser.AddOption<int>("--number");
parser.AddOption<int?>("--nullable");
parser.AddOption<int[]>("--values", arity: "+");
parser.AddOption<List<int>>("--items", action: ArgumentActions.Append);
parser.AddFlag("--enabled");

var result = parser.Parse("--number", "42", "--nullable", "7", "--values", "1", "2", "--items", "3", "--items", "4", "--enabled");

if (result.GetRequired<int>("number") != 42 || result.GetRequired<int?>("nullable") != 7 ||
    !result.GetRequired<int[]>("values").SequenceEqual(new[] { 1, 2 }) ||
    !result.GetRequired<List<int>>("items").SequenceEqual(new[] { 3, 4 }) || !result.GetRequired<bool>("enabled"))
{
    throw new Exception("AOT parsing failed");
}

var unregistered = new ArgumentParser();
unregistered.AddOption<DayOfWeek[]>("--monday", arity: "?", action: ArgumentActions.Append,
    constValue: DayOfWeek.Monday);
try
{
    unregistered.Parse("--monday");
    throw new Exception("AOT accepted an unregistered collection element type");
}
catch (ArgumentException ex) when (ex.Message.Contains("ArgumentCollectionFactories.Register<T>()"))
{
}

ArgumentCollectionFactories.Register<DayOfWeek>();
ArgumentCollectionFactories.Register<DayOfWeek?>();

var constants = new ArgumentParser();
constants.AddOption<DayOfWeek[]>("--monday", arity: "?", action: ArgumentActions.Append,
    constValue: DayOfWeek.Monday);
constants.AddOption<List<DayOfWeek>>("--tuesday", arity: "?", action: ArgumentActions.Append,
    constValue: DayOfWeek.Tuesday);
constants.AddOption<IList<DayOfWeek>>("--wednesday", arity: "?", action: ArgumentActions.Append,
    constValue: DayOfWeek.Wednesday);
constants.AddOption<ICollection<DayOfWeek>>("--thursday", arity: "?", action: ArgumentActions.Append,
    constValue: DayOfWeek.Thursday);
constants.AddOption<IEnumerable<DayOfWeek>>("--friday", arity: "?", action: ArgumentActions.Append,
    constValue: DayOfWeek.Friday);
constants.AddOption<DayOfWeek?[]>("--saturday", arity: "?", action: ArgumentActions.Append,
    constValue: DayOfWeek.Saturday);

var days = constants.Parse("--monday", "--monday", "--tuesday", "--tuesday", "--wednesday",
    "--thursday", "--friday", "--saturday");

if (!days.GetRequired<DayOfWeek[]>("monday").SequenceEqual(new[] { DayOfWeek.Monday, DayOfWeek.Monday }) ||
    !days.GetRequired<List<DayOfWeek>>("tuesday").SequenceEqual(new[] { DayOfWeek.Tuesday, DayOfWeek.Tuesday }) ||
    days.GetRequired<IList<DayOfWeek>>("wednesday").Single() != DayOfWeek.Wednesday ||
    days.GetRequired<ICollection<DayOfWeek>>("thursday").Single() != DayOfWeek.Thursday ||
    days.GetRequired<IEnumerable<DayOfWeek>>("friday").Single() != DayOfWeek.Friday ||
    days.GetRequired<DayOfWeek?[]>("saturday").Single() != DayOfWeek.Saturday)
{
    throw new Exception("AOT registered collection parsing failed");
}

Console.WriteLine("AOT smoke passed");

SmokeBindings.Default.Register();
var bound = result.Get(SmokeBindings.Default.SmokeOptions);
if (bound.Number != 42 || bound.Values.Length != 2 || !bound.Enabled)
    throw new Exception("Generated binding failed");
var registered = result.Get<SmokeOptions>();
if (registered.Number != 42) throw new Exception("Registered binding failed");
var app = new CommandLineApplication(exitOnError: false);
var calls = 0;
app.AddSubcommand("sync", _ => { calls++; });
app.AddSubcommand("async", async _ => { await Task.Yield(); calls++; });
await app.RunAsync(["sync"]);
await app.RunAsync(["async"]);
if (calls != 2) throw new Exception("Subcommand dispatch failed");
var nestedStore = new Argx.Parsing.ArgumentStore();
nestedStore.Set("first-name", "one");
nestedStore.Set("second-name", "two");
nestedStore.Set("alias", 9);
var nested = new Arguments(nestedStore).Get(SmokeBindings.Default.NestedSmoke);
if (nested.First.Name != "one" || nested.Second.Name != "two" || nested.Renamed != 9 || nested.Ignored != 0)
    throw new Exception("Nested/attribute binding failed");
Console.WriteLine("Generated AOT binding passed");

public class SmokeOptions
{
    public int Number { get; set; }
    public int[] Values { get; set; } = [];
    public bool Enabled { get; set; }
}
public class NestedSmoke
{
    public SmokeChild First { get; set; } = null!;
    public SmokeChild Second { get; set; } = null!;
    [Argument("alias")] public int Renamed { get; set; }
    [Ignore] public int Ignored { get; set; }
}
public class SmokeChild { public string Name { get; set; } = ""; }
[ArgxBinding(typeof(NestedSmoke))]
[ArgxBinding(typeof(SmokeOptions))]
internal partial class SmokeBindings : ArgumentBindingContext { }

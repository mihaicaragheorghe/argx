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
    !result.GetRequired<int[]>("values").SequenceEqual(new[] {1, 2}) ||
    !result.GetRequired<List<int>>("items").SequenceEqual(new[] {3, 4}) || !result.GetRequired<bool>("enabled"))
    throw new Exception("AOT parsing failed");
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
if (!days.GetRequired<DayOfWeek[]>("monday").SequenceEqual(new[] {DayOfWeek.Monday, DayOfWeek.Monday}) ||
    !days.GetRequired<List<DayOfWeek>>("tuesday").SequenceEqual(new[] {DayOfWeek.Tuesday, DayOfWeek.Tuesday}) ||
    days.GetRequired<IList<DayOfWeek>>("wednesday").Single() != DayOfWeek.Wednesday ||
    days.GetRequired<ICollection<DayOfWeek>>("thursday").Single() != DayOfWeek.Thursday ||
    days.GetRequired<IEnumerable<DayOfWeek>>("friday").Single() != DayOfWeek.Friday ||
    days.GetRequired<DayOfWeek?[]>("saturday").Single() != DayOfWeek.Saturday)
    throw new Exception("AOT registered collection parsing failed");
Console.WriteLine("AOT smoke passed");

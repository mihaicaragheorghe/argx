using Argx.Actions;
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

Console.WriteLine("AOT smoke passed");

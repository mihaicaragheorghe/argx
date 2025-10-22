using Argx;
using Argx.Actions;
using Argx.Parsing;

var parser = new ArgumentParser();
parser.Add<int[]>("--foo", action: ArgumentActions.Append, arity: Arity.AtLeastOne);
var argx = parser.Parse(["--foo", "0", "--foo", "1", "2", "--bar", "10", "--foo", "3"]);
Console.WriteLine(string.Join(", ", argx.GetValue<int[]>("foo"))); // Outputs: 0, 1, 2
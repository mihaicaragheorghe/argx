using Argx.Parsing;

var parser = new ArgumentParser();
parser.AddArgument<int>("x");
parser.AddArgument<int>("y");

var argx = parser.Parse(["2", "3"]);
var x = argx.GetValue<int>("x");
var y = argx.GetValue<int>("y");

Console.WriteLine(x + y); // Outputs: 5
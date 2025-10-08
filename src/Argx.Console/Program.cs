using Argx.Parsing;

var parser = new ArgumentParser();
parser.AddArgument<int>("x");
parser.AddArgument<int>("y");

var argx = parser.Parse(args);
var x = argx.GetRequired<int>("x");
var y = argx.GetRequired<int>("x");

Console.WriteLine(x + y);
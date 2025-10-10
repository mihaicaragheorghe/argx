using Argx.Parsing;

var parser = new ArgumentParser(program: "Argx", description: "Argx playground");
parser.AddArgument("filename", usage: "the file name");
parser.AddOption<int>("--count", ["-c"], usage: "count argument");
parser.AddFlag("--verbose", ["-v"], usage: "verbose argument, on/off flag");

var argx = parser.Parse(args);
var filename = argx.GetRequired<int>("filename");

Console.WriteLine(filename);
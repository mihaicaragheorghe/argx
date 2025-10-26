using Argx.Actions;
using Argx.Parsing;

var parser = new ArgumentParser();
parser.Add("file");
parser.Add<int>("--level");
parser.Add<bool>("--debug", ["-d"],
    usage: "enable debug mode",
    action: ArgumentActions.StoreTrue);
var argx = parser.Parse(args);
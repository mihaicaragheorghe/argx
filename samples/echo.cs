#!/usr/bin/env dotnet --

#:project ../src/Argx

using Argx.Actions;
using Argx.Parsing;

var parser = new ArgumentParser(
    app: "myapp",
    description: "A simple example application");

// Add a required positional argument
parser.AddArgument("echo", usage: "The string to be echoed back");

// Add an option
parser.AddOption<string>("--prefix", ["-p"], usage: "A prefix to add before the echoed string");

// Add an optional flag
parser.AddOption<int>("--count", ["-c"], usage: "Number of times to echo the string");

parser.AddOption<int>("-v",
    action: ArgumentActions.Count,
    usage: "Increase verbosity level",
    dest: "verbosity");

// Parse the arguments
var argx = parser.Parse(args);

// Access the values
// Required: will throw if not present
var echo = argx.GetRequired<string>("echo");

// Optional: returns default if not found
var prefix = argx.GetValue<string>("prefix") ?? string.Empty;

// TryGetValue pattern
if (!argx.TryGetValue<int>("count", out var count))
    count = 1;

var verbosity = argx.GetValue<int>("verbosity");
Console.WriteLine($"Verbosity level: {verbosity}");

for (int i = 0; i < count; i++)
{
    Console.WriteLine($"{prefix}{echo}");
}
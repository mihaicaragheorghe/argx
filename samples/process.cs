#!/usr/bin/env dotnet --

#:project ../src/Argx

using Argx;
using Argx.Parsing;

var parser = new ArgumentParser();

parser.AddArgument<string[]>("files",
    arity: Arity.AtLeastOne,
    usage: "One or more files to process");

var argx = parser.Parse(args);
var files = argx.GetRequired<string[]>("files");

foreach (var file in files)
{
    Console.WriteLine($"Processing {file}");
}
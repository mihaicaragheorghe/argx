#!/usr/bin/env dotnet

#:project ../src/Argx

using Argx;
using Argx.Parsing;

var app = new CommandLineApplication();

// dotnet run git.cs push main
app.AddSubcommand("push", args =>
{
    var parser = new ArgumentParser();
    parser.AddArgument("remote");
    parser.AddArgument("refspec");
    var pargs = parser.Parse(args);
    var remote = pargs.GetRequired<string>("remote");
    var refspec = pargs.GetRequired<string>("refspec");

    Console.WriteLine($"Pushing to {remote} {refspec}..");
    Console.WriteLine($"Done");
});

// dotnet run git.cs pull main
app.AddSubcommand("pull", async args =>
{
    var parser = new ArgumentParser();
    parser.AddArgument("remote");
    parser.AddArgument("refspec");
    var pargs = parser.Parse(args);
    var remote = pargs.GetRequired<string>("remote");
    var refspec = pargs.GetRequired<string>("refspec");

    Console.WriteLine($"Pulling from {remote} {refspec}..");
    await Task.Delay(1000);
    Console.WriteLine("Done");
});

// dotnet run git.cs status
app.AddSubcommand("status", async args =>
{
    var parser = new ArgumentParser();
    var pargs = parser.Parse(args);

    await Task.Delay(1000);
    Console.WriteLine($"Up to date with remote branch..");
});

await app.RunAsync(args);

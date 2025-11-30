#!/usr/bin/env dotnet --

#:project ../src/Argx

using Argx;
using Argx.Parsing;

var app = new CommandLineApplication(
    name: "git",
    description: "a fast, scalable, distributed revision control system with an unusually rich command set that provides both high-level operations and full access to internals.");

app.AddSubcommand("push", args =>
    {
        var parser = new ArgumentParser(app: app.Name);
        parser.AddArgument("remote");
        parser.AddArgument("refspec");
        var pargs = parser.Parse(args);
        var remote = pargs.GetRequired<string>("remote");
        var refspec = pargs.GetRequired<string>("refspec");

        Console.WriteLine($"Pushing to {remote} {refspec}..");
        Console.WriteLine($"Done");
    })
    .WithUsage("Update remote refs along with associated objects");

app.AddSubcommand("pull", async args =>
    {
        var parser = new ArgumentParser(app: app.Name);
        parser.AddArgument("remote");
        parser.AddArgument("refspec");
        var pargs = parser.Parse(args);
        var remote = pargs.GetRequired<string>("remote");
        var refspec = pargs.GetRequired<string>("refspec");

        Console.WriteLine($"Pulling from {remote} {refspec}..");
        await Task.Delay(1000);
        Console.WriteLine("Done");
    })
    .WithUsage("Fetch from and integrate with another repository or a local branch");

app.AddSubcommand("status", async args =>
    {
        var parser = new ArgumentParser(app: app.Name);
        var pargs = parser.Parse(args);

        await Task.Delay(1000);
        Console.WriteLine($"Up to date with remote branch..");
    })
    .WithUsage("Show the working tree status");

await app.RunAsync(args);

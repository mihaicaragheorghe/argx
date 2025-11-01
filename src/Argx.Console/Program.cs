using Argx;
using Argx.Parsing;

var app = new CommandLineApplication();

app.AddSubcommand("push", args =>
{
    var parser = new ArgumentParser();
    parser.AddArgument("branch");
    var pargs = parser.Parse(args);
    var branch = pargs.GetRequired<string>("branch");

    Console.WriteLine($"Pushing to remote branch {branch}..");
});

app.AddSubcommand("pull", async args =>
{
    var parser = new ArgumentParser();
    parser.AddArgument("branch");
    var pargs = parser.Parse(args);
    var branch = pargs.GetRequired<string>("branch");

    Console.WriteLine($"Pulling from remote branch {branch}..");
    await Task.Delay(1000);
    Console.WriteLine("Done");
});

app.AddSubcommand("status", async args =>
{
    var parser = new ArgumentParser();
    parser.AddArgument("branch");
    var pargs = parser.Parse(args);
    var branch = pargs.GetRequired<string>("branch");

    await Task.Delay(1000);
    Console.WriteLine($"Up to date with remote branch {branch}..");
});

await app.RunAsync(args);
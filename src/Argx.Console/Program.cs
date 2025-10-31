using Argx;
using Argx.Parsing;

var app = new CommandLineApplication();

app.AddSubcommand("push", (string[] args) =>
{
    var parser = new ArgumentParser();
    parser.AddArgument("branch");
    var pargs = parser.Parse(args);
    var branch = pargs.GetRequired<string>("branch");

    Console.WriteLine($"Pushing to remote branch {branch}..");
    return 0;
});

app.AddSubcommand("pull", async (string[] args) =>
{
    var parser = new ArgumentParser();
    parser.AddArgument("branch");
    var pargs = parser.Parse(args);
    var branch = pargs.GetRequired<string>("branch");

    Console.WriteLine($"Pulling from remote branch {branch}..");
    await Task.Delay(1000);
    Console.WriteLine("Done");
    return 0;
});

await app.RunAsync(args);
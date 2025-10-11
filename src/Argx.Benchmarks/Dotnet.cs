using Argx.Actions;
using Argx.Parsing;

using BenchmarkDotNet.Attributes;

namespace Argx.Benchmarks;

[MemoryDiagnoser(displayGenColumns: true)]
public class Dotnet
{
    [Benchmark]
    public void Run()
    {
        var parser = new ArgumentParser();

        parser.AddOption(name: "--arch", alias: ["-a"], dest: "arch", action: ArgumentActions.Store,
            usage: "Specifies the target architecture. Shorthand for setting the Runtime Identifier (RID)...");

        parser.AddOption(name: "--artifacts-path", alias: null, dest: "artifacts_path", action: ArgumentActions.Store,
            usage: "Specifies the artifacts (output) directory.");

        parser.AddOption(name: "--configuration", alias: ["-c"], dest: "configuration", action: ArgumentActions.Store,
            usage: "Defines the build configuration. The default is 'Debug' for most commands.");

        parser.AddOption(name: "--environment", alias: ["-e"], dest: "environment", action: ArgumentActions.Store,
            usage: "Sets environment variables using KEY=VALUE format.");

        parser.AddOption(name: "--file", alias: null, dest: "file", action: ArgumentActions.Store,
            usage: "Specifies the project or solution file to run.");

        parser.AddOption(name: "--framework", alias: ["-f"], dest: "framework", action: ArgumentActions.Store,
            usage: "Compiles for a specific target framework.");

        parser.AddOption(name: "--force", alias: null, dest: "force", action: ArgumentActions.StoreTrue,
            usage: "Forces all dependencies to be resolved even if the last restore was successful.");

        parser.AddOption(name: "--interactive", alias: null, dest: "interactive", action: ArgumentActions.StoreTrue,
            usage: "Allows the command to stop and wait for user input or action.");

        parser.AddOption(name: "--launch-profile", alias: null, dest: "launch_profile", action: ArgumentActions.Store,
            usage: "Specifies the launch profile to use.");

        parser.AddOption(name: "--no-build", alias: null, dest: "no_build", action: ArgumentActions.StoreTrue,
            usage: "Doesn't build the project before running.");

        parser.AddOption(name: "--no-dependencies", alias: null, action: ArgumentActions.StoreTrue,
            dest: ArgumentActions.StoreTrue,
            usage: "Ignores project-to-project references.");

        parser.AddOption(name: "--no-launch-profile", alias: null, action: ArgumentActions.StoreTrue,
            dest: ArgumentActions.StoreTrue,
            usage: "Runs without applying launchSettings.json.");

        parser.AddOption(name: "--no-restore", alias: null, dest: "no_restore", action: ArgumentActions.StoreTrue,
            usage: "Doesn't execute an implicit restore before running the command.");

        parser.AddOption(name: "--os", alias: null, dest: "os", action: ArgumentActions.Store,
            usage: "Specifies the operating system component of the RID.");

        parser.AddOption(name: "--project", alias: null, dest: "project", action: ArgumentActions.Store,
            usage: "Specifies the path to the project file.");

        parser.AddOption(name: "--runtime", alias: ["-r"], dest: "runtime", action: ArgumentActions.Store,
            usage: "Specifies the target runtime identifier (RID).");

        parser.AddOption(name: "--tl", alias: null, dest: "tl", action: ArgumentActions.Store,
            usage: "Sets trimming mode (auto|on|off).");

        parser.AddOption(name: "--verbosity", alias: ["-v"], dest: "verbosity", action: ArgumentActions.Store,
            usage: "Sets the verbosity level.");

        parser.AddArgument(name: "applicationArguments", dest: "applicationArguments",
            usage: "Arguments passed to the application that is being run.");

        var result = parser.Parse("dotnet run", "--project", "src/Argx.Benchmarks/", "-c", "Release");
    }
}
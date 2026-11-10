using Argx.Abstractions;

namespace Argx.Utils;

internal class EnvironmentControl : IEnvironment
{
    public void Exit(int exitCode)
    {
        Environment.Exit(exitCode);
    }

    public string[] GetCommandLineArgs()
    {
        return Environment.GetCommandLineArgs();
    }
}
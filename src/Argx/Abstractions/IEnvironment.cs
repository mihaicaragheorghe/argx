namespace Argx.Abstractions;

internal interface IEnvironment
{
    void Exit(int exitCode);

    string[] GetCommandLineArgs();
}
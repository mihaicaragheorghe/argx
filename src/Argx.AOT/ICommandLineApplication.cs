using Argx.Subcommands;

namespace Argx;

/// <summary>
/// Represents a command line application.
/// </summary>
public interface ICommandLineApplication
{
    /// <summary>
    /// The name of the command line application.
    /// Used in help text and prompts.
    /// <remarks>
    /// If not provided, the application will infer the name using argument zero from the environment.
    /// </remarks>
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// The usage information for the application.
    /// Used in help text.
    /// </summary>
    /// <remarks>
    /// If not provided, a default usage string will be generated based on registered subcommands.
    /// </remarks>
    string? Usage { get; }

    /// <summary>
    /// The description of the command line application.
    /// Used in help text.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// The epilogue text for the command line application.
    /// Displayed at the end of help text.
    /// </summary>
    string? Epilogue { get; }

    /// <summary>
    /// Adds a subcommand to the command line application.
    /// </summary>
    /// <param name="name">
    /// The name of the subcommand.
    /// This is the string used to invoke the subcommand.
    /// </param>
    /// <param name="handler">
    /// The asynchronous handler delegate for the subcommand.
    /// This delegate will be invoked when the subcommand is executed.
    /// </param>
    /// <returns>The registered subcommand, allowing for further configuration.</returns>
    Subcommand AddSubcommand(string name, AsyncSubcommandDelegate handler);

    /// <summary>
    /// Adds a subcommand to the command line application.
    /// </summary>
    /// <param name="name">
    /// The name of the subcommand.
    /// This is the string used to invoke the subcommand.
    /// </param>
    /// <param name="handler">
    /// The synchronous handler delegate for the subcommand.
    /// This delegate will be invoked when the subcommand is executed.
    /// </param>
    /// <returns>The registered subcommand, allowing for further configuration.</returns>
    Subcommand AddSubcommand(string name, SubcommandDelegate handler);

    /// <summary>
    /// Runs the command line application asynchronously with the specified arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync(string[] args);

    /// <summary>
    /// Runs the command line application synchronously with the specified arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    void Run(string[] args);
}
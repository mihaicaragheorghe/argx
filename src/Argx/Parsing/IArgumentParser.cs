using Argx.Actions;
using Argx.Errors;

namespace Argx.Parsing;

/// <summary>
/// Provides functionality for defining, parsing, and managing command-line arguments.
/// </summary>
/// <remarks>
/// Automatically generates help and usage text.
/// Use <see cref="ArgumentParserConfiguration"/> to customize its behavior.
/// </remarks>
public interface IArgumentParser
{
    /// <summary>
    /// Adds a new argument definition to the parser.
    /// </summary>
    /// <typeparam name="T">
    /// The expected type of the argument value.
    /// </typeparam>
    /// <param name="name">
    /// The primary name of the argument.<br/>
    /// Positional arguments should be specified without prefixes (e.g., <c>input</c>), while optional arguments should start with dashes (e.g., <c>--verbose</c>, <c>-verbose</c>, <c>-v</c>).
    /// </param>
    /// <param name="alias">
    /// Optional alternative names for the argument (e.g., <c>-v</c>). Use null for no alias.<br/>
    /// Must be null for positional arguments.
    /// </param>
    /// <param name="usage">
    /// A short description of the argument’s purpose, shown in help output.
    /// </param>
    /// <param name="dest">
    /// The key which will be added to the <see cref="Arguments"/> dictionary returned by the <see cref="Parse"/> method, used to store and retrieve the argument value.<br/>
    /// If null, the parser infers it from <paramref name="name"/>.
    /// </param>
    /// <param name="metavar">
    /// The placeholder name displayed in help messages for the argument’s value. Displayed only for arguments that expect values.
    /// </param>
    /// <param name="constValue">
    /// A constant value assigned when the argument is used without an explicit value. Relevant mainly for certain <paramref name="action"/> types.
    /// </param>
    /// <param name="action">
    /// The action to perform when the argument is encountered (e.g., <c>"store"</c>, <c>"store_true"</c>).<br/>
    /// Action names are defined in <see cref="ArgumentActions"/>.<br/>
    /// </param>
    /// <param name="arity">
    /// Specifies how many values the argument expects (e.g., <c>?</c>(optional), <c>*</c>(any), <c>"+"</c>(at least one), or a fixed number).<br/>
    /// Use <c>null</c> to rely on defaults inferred from the action.
    /// </param>
    /// <param name="choices">
    /// A set of allowed values for the argument. If specified, the argument's value must be one of these choices.
    /// </param>
    /// <returns>
    /// The current <see cref="ArgumentParser"/> instance, allowing for chaining.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the argument name or alias is invalid, or if the action is not in <see cref="ActionRegistry"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a positional argument has aliases or an invalid arity.
    /// </exception>
    /// <remarks>
    /// This method automatically determines whether the argument is positional or optional based on its name, and registers it accordingly.  
    /// </remarks>
    public ArgumentParser Add<T>(
        string name,
        string[]? alias = null,
        string? usage = null,
        string? dest = null,
        string? metavar = null,
        object? constValue = null,
        string? action = null,
        string? arity = null,
        string[]? choices = null);

    /// <inheritdoc cref="Add{T}(string, string[], string, string, string, object, string, string, string[])" />
    public ArgumentParser Add(
        string name,
        string[]? alias = null,
        string? action = null,
        string? usage = null,
        string? dest = null,
        string? metavar = null,
        object? constValue = null,
        string? arity = null,
        string[]? choices = null);

    /// <summary>
    /// Adds a new <b>positional argument</b> definition to the parser.
    /// </summary>
    /// <typeparam name="T">
    /// The expected type of the argument value.
    /// </typeparam>
    /// <param name="name">
    /// The name of the positional argument (must not start with a dash).
    /// Used to retrieve the argument value from the parsed result.
    /// </param>
    /// <param name="usage">
    /// A short description of the argument’s purpose, displayed in help output.
    /// </param>
    /// <param name="action">
    /// The action to perform when the argument is encountered (e.g., <c>"store"</c>, <c>"store_true"</c>).<br/>
    /// Action names are defined in <see cref="ArgumentActions"/>.<br/>
    /// </param>
    /// <param name="arity">
    /// Specifies how many values the argument expects (e.g., <c>?</c>(optional), <c>*</c>(any), <c>"+"</c>(at least one), or a fixed number).<br/>
    /// Use <c>null</c> to rely on defaults inferred from the action.
    /// </param>
    /// <returns>
    /// The current <see cref="ArgumentParser"/> instance, allowing for chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the argument name begins with <c>'-'</c>, indicating an optional argument or if the argument has aliases or an invalid arity.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the argument name or alias is invalid, or if the action is not in <see cref="ActionRegistry"/>.
    /// </exception>
    /// <remarks>
    /// This method is a convenience overload of <see cref="Add{T}(string, string[], string, string, string, object, string, string, string[])"/> for defining positional arguments.
    /// </remarks>
    public ArgumentParser AddArgument<T>(string name, string? usage = null, string? action = null, string? arity = null);

    /// <inheritdoc cref="AddArgument{T}(string, string?, string?, string?)" />
    ArgumentParser AddArgument(string name, string? usage = null, string? action = null, string? arity = null);

    /// <summary>
    /// Adds a new <b>boolean flag</b> to the parser.
    /// </summary>
    /// <param name="name">
    /// The primary name of the flag (must start with <c>'-'</c> or <c>"--"</c>).
    /// </param>
    /// <param name="alias">
    /// Optional alternative names for the flag (e.g., <c>"-v"</c>). Use <c>null</c> if no aliases are desired.
    /// </param>
    /// <param name="usage">
    /// A short description of the flag’s purpose, shown in help output.
    /// </param>
    /// <param name="dest">
    /// The key which will be added to the <c><see cref="Arguments"/></c> dictionary returned by the <c><see cref="Parse"/></c> method, used to store and retrieve the argument value.<br/>
    /// If null, the parser infers it from <c><paramref name="name"/></c>.
    /// </param>
    /// <param name="value">
    /// The boolean constant assigned when the flag is present.<br/>
    /// Defaults to <c>true</c> (using <see cref="ArgumentActions.StoreTrue"/>).<br/>
    /// Set to <c>false</c> to create an inverted flag using <see cref="ArgumentActions.StoreFalse"/>.
    /// </param>
    /// <returns>
    /// The current <see cref="ArgumentParser"/> instance, allowing for chaining.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the argument name is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if name does not start with <c>'-'</c>, indicating it is not an optional argument.
    /// </exception>
    /// <remarks>
    /// This is a convenience overload of <see cref="Add{T}(string, string[], string, string, string, object, string, string, string[])"/> for defining boolean flags.<br/>
    /// Flags always have an arity of <c>0</c> and do not accept values explicitly.
    /// </remarks>
    public ArgumentParser AddFlag(
        string name,
        string[]? alias = null,
        string? usage = null,
        string? dest = null,
        bool value = true);

    /// <summary>
    /// Adds a new <b>optional argument</b> (option) to the parser.
    /// </summary>
    /// <typeparam name="T">
    /// The expected type of the value.  
    /// </typeparam>
    /// <param name="name">
    /// The primary name of the option, must start with <c>'-'</c> or <c>"--"</c> (e.g. <c>"--output"</c> or <c>"-o"</c>).
    /// </param>
    /// <param name="alias">
    /// Optional alternative names for the option (e.g., <c>"-o"</c>). Use <c>null</c> for no alias.
    /// </param>
    /// <param name="usage">
    /// A short description of the option’s purpose, shown in help output.
    /// </param>
    /// <param name="dest">
    /// The key which will be used to retrieve the value from parsed result.<br/>
    /// If null, the parser infers it from <paramref name="name"/>.
    /// </param>
    /// <param name="metavar">
    /// The placeholder name displayed in help messages for the argument’s value. Displayed only for arguments that expect values.
    /// </param>
    /// <param name="constValue">
    /// A constant value assigned when the argument is used without an explicit value. Relevant mainly for certain <paramref name="action"/> types.
    /// </param>
    /// <param name="action">
    /// The action to perform when the argument is encountered (e.g., <c>"store"</c>, <c>"store_true"</c>).<br/>
    /// Action names are defined in <see cref="ArgumentActions"/>.<br/>
    /// </param>
    /// <param name="arity">
    /// Specifies how many values the argument expects (e.g., <c>?</c>(optional), <c>*</c>(any), <c>"+"</c>(at least one), or a fixed number).<br/>
    /// Use <c>null</c> to rely on defaults inferred from the action.
    /// </param>
    /// <param name="choices">
    /// A set of allowed values for the argument. If specified, the argument's value must be one of these choices.
    /// </param>
    /// <returns>
    /// The current <see cref="ArgumentParser"/> instance, allowing for chaining.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the argument name is null or empty or if the action is not in <see cref="ActionRegistry"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the option name does not begin with <c>'-'</c> or if the arity is invalid.
    /// </exception>
    /// <remarks>
    /// This is a convenience overload of <see cref="Add{T}(string, string[], string, string, string, object, string, string, string[])"/> for defining optional arguments.
    /// </remarks>
    public ArgumentParser AddOption<T>(
        string name,
        string[]? alias = null,
        string? usage = null,
        string? dest = null,
        string? metavar = null,
        object? constValue = null,
        string? action = null,
        string? arity = null,
        string[]? choices = null);

    /// <inheritdoc cref="AddOption{T}(string, string[], string, string, string, object, string, string, string[])" />
    public ArgumentParser AddOption(
        string name,
        string[]? alias = null,
        string? usage = null,
        string? dest = null,
        string? metavar = null,
        object? constValue = null,
        string? action = null,
        string? arity = null,
        string[]? choices = null);

    /// <summary>
    /// Parses the provided command-line arguments according to the defined argument schema.
    /// </summary>
    /// <param name="args">
    /// The command-line arguments to parse.  
    /// Usually taken directly from <c>string[] args</c> in <c>Main()</c>.
    /// </param>
    /// <returns>
    /// An <see cref="Arguments"/> instance containing the parsed values for all known and unknown arguments.  
    /// </returns>
    /// <exception cref="ArgumentValueException">
    /// Thrown when an argument fails validation and <see cref="ArgumentParserConfiguration.ExitOnError"/> is <c>false</c>.
    /// </exception>
    /// <remarks>
    /// If <see cref="ArgumentParserConfiguration.ExitOnError"/> is <c>true</c> (default behavior),
    /// the parser will print an error message and usage information to the console and terminate the application
    /// with the exit code specified in <see cref="ArgumentParserConfiguration.ErrorExitCode"/> upon encountering a parsing error.<br/>
    /// Otherwise, it will throw an <see cref="ArgumentValueException"/> which can be caught and handled by the caller.
    /// </remarks>
    public IArguments Parse(params string[] args);

    /// <summary>
    /// Registers a custom argument action handler.
    /// </summary>
    /// <param name="name">the name of the action.</param>
    /// <param name="action">the action handler instance.</param>
    /// <returns>The current <see cref="ArgumentParser"/> instance, allowing for chaining.</returns>
    public ArgumentParser AddAction(string name, ArgumentAction action);
}
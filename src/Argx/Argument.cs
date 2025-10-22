using Argx.Actions;

namespace Argx;

/// <summary>
/// Represents a command-line argument with its specifications.
/// </summary>
public class Argument
{
    /// <summary>
    /// The name of the argument, used to identify the argument in the command-line.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A set of alternative names for the argument (e.g. '-a', '-v')
    /// </summary>
    /// <remarks>
    /// Must be null for positional arguments.
    /// </remarks>
    public AliasSet? Aliases { get; }

    /// <summary>
    /// The action to be performed when the argument is encountered.
    /// Defaults to <see cref="ArgumentActions.Store"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ArgumentActions"/> for built-in actions.
    /// </remarks>
    public string Action { get; }

    /// <summary>
    /// The data type of the argument's value. Defaults to <see cref="string"/>.
    /// </summary>
    /// <remarks>
    /// For arguments with 'append' action, this should be a collection type.
    /// </remarks>
    public Type ValueType { get; }

    /// <summary>
    /// The destination key where the argument's value will be stored.
    /// Defaults to the argument's name without leading dashes.
    /// </summary>
    /// <remarks>
    /// This should be used to retrieve the value from <see cref="IArguments"/>.
    /// </remarks>
    public string Dest { get; }

    /// <summary>
    /// A brief description of the argument's purpose, used in help text.
    /// </summary>
    public string? Usage { get; }

    /// <summary>
    /// A string representing the expected value in the usage text.
    /// Defaults to the argument's name in uppercase without leading dashes.
    /// </summary>
    public string? Metavar { get; }

    /// <summary>
    /// A constant value to be used if no value is provided for the argument.
    /// </summary>
    /// <remarks>
    /// This should be used for arguments with optional arity or 'store_const' action.
    /// </remarks>
    public object? ConstValue { get; }

    /// <summary>
    /// An array of valid choices for the argument's value. If specified, the argument's value must be one of these choices.
    /// </summary>
    /// <remarks>
    /// This only applies to 'store' and 'append' actions.
    /// </remarks>
    public string[]? Choices { get; }

    /// <summary>
    /// The arity of the argument, defining how many values it can take.
    /// </summary>
    /// <remarks>
    /// This is determined based on the action if not explicitly specified.
    /// </remarks>
    public Arity Arity { get; }

    /// <summary>
    /// Indicates whether the argument is positional or optional.
    /// </summary>
    public bool IsPositional { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Argument"/> class.
    /// </summary>
    /// <param name="name">The name of the argument.</param>
    /// <param name="action">The action to be performed when the argument is encountered.</param>
    /// <param name="dest">The destination key where the argument's value will be stored.</param>
    /// <param name="usage">A brief description of the argument's purpose.</param>
    /// <param name="metavar">A string representing the expected value in the usage text.</param>
    /// <param name="arity">The arity of the argument, defining how many values it can take.</param>
    /// <param name="constValue">A constant value to be used if no value is provided for the argument.</param>
    /// <param name="choices">An array of valid choices for the argument's value.</param>
    /// <param name="isPositional">Indicates whether the argument is positional or optional.</param>
    /// <param name="type">The data type of the argument's value.</param>
    /// <param name="alias">A set of alternative names for the argument.</param>
    public Argument(
        string name,
        string? action = null,
        string? dest = null,
        string? usage = null,
        string? metavar = null,
        string? arity = null,
        object? constValue = null,
        string[]? choices = null,
        bool isPositional = false,
        Type? type = null,
        params string[]? alias)
    {
        Name = name;
        Aliases = alias == null ? null : new AliasSet(alias);
        ValueType = type ?? typeof(string);
        Action = action ?? ArgumentActions.Store;
        Dest = dest ?? name.TrimStart('-');
        Usage = usage;
        Metavar = metavar ?? Name.TrimStart('-').ToUpper();
        ConstValue = constValue;
        Choices = choices;
        IsPositional = isPositional;
        Arity = arity is null
            ? new Arity(ActionRegistry.DefaultArity(action ?? ArgumentActions.Store))
            : new Arity(arity);
    }

    /// <summary>
    /// Gets a display-friendly name for the argument, combining its name and aliases.
    /// </summary>
    public string GetDisplayName()
    {
        return Name + (string.IsNullOrEmpty(Aliases?.ToString()) ? "" : $", {Aliases}");
    }
}
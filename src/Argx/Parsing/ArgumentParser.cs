using Argx.Actions;
using Argx.Errors;
using Argx.Extensions;
using Argx.Help;
using Argx.Store;

namespace Argx.Parsing;

/// <summary>
/// Provides functionality for defining, parsing, and managing command-line arguments.
/// </summary>
/// <remarks>
/// Automatically generates help and usage text.
/// Use <see cref="ArgumentParserConfiguration"/> to customize its behavior.
/// </remarks>
public class ArgumentParser
{
    private readonly string? _program;
    private readonly string? _description;
    private readonly string? _usage;
    private readonly string? _epilogue;

    private readonly OptionSet _knownOpts = new();
    private readonly PositionalList _knownArgs = [];

    private readonly IArgumentRepository _repository;
    private readonly ArgumentParserConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentParser"/> class.
    /// </summary>
    /// <param name="program">The name of the program being executed, shown at the top of the help message and in usage (default: generated from <c>argv[0]</c>).</param>
    /// <param name="description">A short description of what the program does, displayed in the help output (default: no text).</param>
    /// <param name="usage">Custom usage text (default: generated from arguments added to parser).</param>
    /// <param name="epilogue">Optional text displayed at the end of the help message.</param>
    /// <param name="configuration">Optional parser configuration. If <c>null</c>, a default <see cref="ArgumentParserConfiguration"/> is created.</param>
    /// <remarks>
    /// If <see cref="ArgumentParserConfiguration.AddHelpArgument"/> is enabled (default behavior),
    /// a <c>--help</c> / <c>-h</c> argument is automatically registered to display help information.
    /// </remarks>
    public ArgumentParser(
        string? program = null,
        string? description = null,
        string? usage = null,
        string? epilogue = null,
        ArgumentParserConfiguration? configuration = null)
    {
        var config = configuration ?? new ArgumentParserConfiguration();

        _program = program;
        _description = description;
        _usage = usage;
        _epilogue = epilogue;
        _configuration = config;
        _repository = new ArgumentRepository();

        if (config.AddHelpArgument)
        {
            _knownOpts.Add(new Argument("--help", alias: "-h", usage: "Print help message",
                action: ArgumentActions.NoAction));
        }
    }

    internal ArgumentParser(
        IArgumentRepository repository,
        string? program = null,
        string? description = null,
        string? usage = null,
        string? epilogue = null,
        ArgumentParserConfiguration? configuration = null)
        : this(program, description, usage, epilogue, configuration)
    {
        _repository = repository;
    }

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
    /// Optional alternative names for the argument (e.g., <c>-v</c>). Use null if no aliases are desired.<br/>
    /// Must be null for positional arguments.
    /// </param>
    /// <param name="usage">
    /// A short description of the argument’s purpose, shown in help output.
    /// </param>
    /// <param name="dest">
    /// The key which will be added to the <see cref="Arguments"/> dictionary returned by the <see cref="Parse()"/> method, used to store and retrieve the argument value.<br/>
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
    /// A set of allowed values for the argument. Only applicable to <c>"choice"</c> action.
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
        string[]? choices = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Argument name cannot be null or empty", nameof(name));
        }

        if (alias?.Length == 0)
        {
            throw new ArgumentException($"Argument {name}: alias cannot be empty, use null instead", nameof(alias));
        }

        var isPositional = IsPositional(name);

        if (isPositional && alias != null)
        {
            throw new InvalidOperationException(
                $"Argument {name}: positional arguments cannot have an alias, consider using an optional argument");
        }

        var arg = new Argument(
            name: name,
            alias: alias,
            action: action,
            dest: dest,
            arity: arity,
            usage: usage,
            metavar: metavar,
            constValue: constValue,
            choices: choices,
            isPositional: isPositional,
            type: typeof(T));

        if (isPositional && arg.Arity == 0)
        {
            throw new InvalidOperationException($"Argument {arg.Name}: positional arguments cannot have arity 0");
        }

        if (!ActionRegistry.TryGetHandler(arg.Action, out var handler))
        {
            throw new ArgumentException($"Argument {arg.Name}: Unknown action '{action}'");
        }

        handler!.Validate(arg);

        if (isPositional)
        {
            _knownArgs.Add(arg);
        }
        else
        {
            _knownOpts.Add(arg);
        }

        return this;
    }

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
        string[]? choices = null)
    {
        return Add<string>(
            name: name,
            alias: alias,
            action: action,
            usage: usage,
            dest: dest,
            metavar: metavar,
            constValue: constValue,
            choices: choices,
            arity: arity);
    }

    /// <summary>
    /// Adds a new <b>positional argument</b> definition to the parser.
    /// </summary>
    /// <typeparam name="T">
    /// The expected type of the argument value.
    /// </typeparam>
    /// <param name="name">
    /// The name of the positional argument (must not start with a dash).
    /// </param>
    /// <param name="usage">
    /// A short description of the argument’s purpose, displayed in help output.
    /// </param>
    /// <param name="dest">
    /// The key which will be added to the <see cref="Arguments"/> dictionary returned by the <see cref="Parse()"/> method, used to store and retrieve the argument value.<br/>
    /// If null, <paramref name="name"/> will be used.
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
    public ArgumentParser AddArgument<T>(string name, string? usage = null, string? dest = null)
    {
        if (name.IsOption())
        {
            throw new InvalidOperationException($"Invalid positional argument {name}: cannot start with '-'");
        }

        return Add<T>(name: name, usage: usage, dest: dest);
    }

    /// <inheritdoc cref="AddArgument{T}(string, string, string)" />
    public ArgumentParser AddArgument(string name, string? usage = null, string? dest = null)
        => AddArgument<string>(name: name, usage: usage, dest: dest);

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
    /// The key which will be added to the <c><see cref="Arguments"/></c> dictionary returned by the <c><see cref="Parse()"/></c> method, used to store and retrieve the argument value.<br/>
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
        bool value = true)
    {
        if (!name.IsOption())
        {
            throw new InvalidOperationException($"Invalid flag {name}: should start with '-'");
        }

        return Add<bool>(
            name: name,
            alias: alias,
            usage: usage,
            dest: dest,
            constValue: value,
            action: value ? ArgumentActions.StoreTrue : ArgumentActions.StoreFalse,
            arity: "0");
    }

    /// <summary>
    /// Adds a new <b>optional argument</b> (option) to the parser.
    /// </summary>
    /// <typeparam name="T">
    /// The expected type of the option value.  
    /// </typeparam>
    /// <param name="name">
    /// The primary name of the option, must start with <c>'-'</c> or <c>"--"</c> (e.g. <c>"--output"</c> or <c>"-o"</c>).
    /// </param>
    /// <param name="alias">
    /// Optional alternative names for the option (e.g., <c>"-o"</c>). Use <c>null</c> if no aliases are desired.
    /// </param>
    /// <param name="usage">
    /// A short description of the option’s purpose, shown in help output.
    /// </param>
    /// <param name="dest">
    /// The key which will be added to the <see cref="Arguments"/> dictionary returned by the <see cref="Parse()"/> method, used to store and retrieve the argument value.<br/>
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
    /// A set of allowed values for the argument. Only applicable to <c>"choice"</c> action.
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
        string? arity = null)
    {
        if (!name.IsOption())
        {
            throw new InvalidOperationException($"Invalid option {name}: should start with '-'");
        }

        return Add<T>(
            name: name,
            alias: alias,
            usage: usage,
            dest: dest,
            metavar: metavar,
            constValue: constValue,
            action: action,
            arity: arity);
    }

    /// <inheritdoc cref="AddOption{T}(string, string[], string, string, string, object, string, string)" />
    public ArgumentParser AddOption(
        string name,
        string[]? alias = null,
        string? usage = null,
        string? dest = null,
        string? metavar = null,
        object? constValue = null,
        string? action = null,
        string? arity = null)
    {
        return AddOption<string>(
            name: name,
            alias: alias,
            usage: usage,
            dest: dest,
            metavar: metavar,
            constValue: constValue,
            action: action,
            arity: arity);
    }

    /// <summary>
    /// Parses the provided command-line arguments according to the defined argument schema.
    /// </summary>
    /// <param name="args">
    /// The command-line arguments to parse.  
    /// Typically taken directly from <c>string[] args</c> in <c>Main()</c>.
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
    public Arguments Parse(params string[] args)
    {
        try
        {
            return ParseInternal(args);
        }
        catch (ArgumentValueException ex)
        {
            if (!_configuration.ExitOnError)
            {
                throw;
            }

            Console.WriteLine(ex.Message);
            Console.WriteLine();
            WriteUsage(Console.Out);

            Environment.Exit(_configuration.ErrorExitCode);
            return null;
        }
    }

    internal Arguments ParseInternal(string[] args)
    {
        var result = new Arguments(_repository);
        var tokens = args.Tokenize();
        var consumeOpts = true;

        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].Type == TokenType.Separator)
            {
                consumeOpts = false;
                continue;
            }

            if (consumeOpts && tokens[i].Type == TokenType.Option)
            {
                i += ConsumeOption(i, tokens, result);
                continue;
            }

            if (tokens[i].Type == TokenType.Argument || !consumeOpts)
            {
                i += ConsumePositional(i, tokens, result);
            }
        }

        if (!ValidateRequiredArgs(out var name))
        {
            throw new ArgumentValueException(name!, "expected value");
        }

        return result;
    }

    private int ConsumePositional(int idx, ReadOnlySpan<Token> tokens, Arguments result)
    {
        if (_knownArgs.ReachedEnd)
        {
            result.Extras.Add(tokens[idx].Value);
            return 0;
        }

        var argument = _knownArgs.Next();

        if (!ActionRegistry.TryGetHandler(argument.Action, out var handler))
        {
            throw new InvalidOperationException($"Unknown action for argument {argument.Name}");
        }

        var len = ParseArity(argument, tokens, idx);

        if (idx + len > tokens.Length)
        {
            throw new ArgumentValueException(argument.Name, "not enough values provided");
        }

        handler!.Execute(
            arg: argument,
            invocation: new Token(argument.Name, TokenType.Argument, Token.ImplicitPosition),
            values: tokens.Slice(idx, len),
            store: _repository);

        return len - 1;
    }

    private int ConsumeOption(int idx, ReadOnlySpan<Token> tokens, Arguments result)
    {
        var token = tokens[idx];

        if (_configuration.AddHelpArgument && token == "-h" || token == "--help")
        {
            WriteHelp(Console.Out);
            Environment.Exit(0);
        }

        if (IsBundle(token.Value))
        {
            ParseBundle(token, result);
            return 0;
        }

        var arg = _knownOpts.Get(token.Value);

        if (arg is null)
        {
            result.Extras.Add(token);
            return 0;
        }

        if (!ActionRegistry.TryGetHandler(arg.Action, out var handler))
        {
            throw new InvalidOperationException($"Unknown action for argument {arg.Name}");
        }

        var len = ParseArity(arg, tokens, idx);

        if (idx + 1 + len > tokens.Length)
        {
            throw new ArgumentValueException(arg.Name, "not enough values provided");
        }

        handler!.Execute(
            arg: arg,
            invocation: tokens[idx],
            values: tokens.Slice(idx + 1, len),
            store: _repository);

        return len;
    }

    private static int ParseArity(Argument argument, ReadOnlySpan<Token> tokens, int from)
    {
        if (argument.Arity.IsFixed)
        {
            return int.Parse(argument.Arity.Value);
        }

        var count = 0;
        var idx = tokens[from].Type == TokenType.Option ? from + 1 : from;

        switch (argument.Arity.Value)
        {
            case Arity.Optional:
                if (idx < tokens.Length && tokens[idx].Type == TokenType.Argument)
                {
                    return 1;
                }

                return 0;

            case Arity.Any:
            case Arity.AtLeastOne:
                while (idx < tokens.Length && tokens[idx].Type == TokenType.Argument)
                {
                    count++;
                    idx++;
                }

                if (argument.Arity.Value == Arity.AtLeastOne && count == 0)
                {
                    throw new ArgumentValueException(argument.Name, "requires at least one value");
                }

                return count;

            default:
                throw new InvalidOperationException($"Unknown arity value: {argument.Arity.Value}");
        }
    }

    private void ParseBundle(Token token, Arguments result)
    {
        foreach (char c in token.Value[1..])
        {
            var alias = $"-{c}";
            var arg = _knownOpts.GetByAlias(alias);

            if (arg is null)
            {
                result.Extras.Add(alias);
                continue;
            }

            if (arg.Arity != 0 && arg.Arity.Value != Arity.Optional)
            {
                throw new ArgumentValueException(arg.Name, "cannot be bundled because it requires a value");
            }

            if (!ActionRegistry.TryGetHandler(arg.Action, out var handler))
            {
                throw new InvalidOperationException($"Unknown action for argument {arg.Name}");
            }

            handler!.Execute(
                arg: arg,
                invocation: token,
                values: [],
                store: _repository);
        }
    }

    private bool ValidateRequiredArgs(out string? argName)
    {
        foreach (var arg in _knownArgs)
        {
            if (!_repository.Contains(arg.Dest))
            {
                argName = arg.Name;
                return false;
            }
        }

        argName = null;
        return true;
    }

    private List<Argument> ConcatArguments() => new List<Argument>(_knownOpts.ToList()).Concat(_knownArgs).ToList();

    internal void WriteHelp(TextWriter writer)
    {
        var arguments = ConcatArguments();
        var builder = new HelpBuilder(_configuration.HelpConfiguration);

        if (!string.IsNullOrEmpty(_program))
        {
            builder.AddSection(_program, _description ?? string.Empty);
        }
        else if (!string.IsNullOrEmpty(_description))
        {
            builder.AddText(_description);
        }

        if (!string.IsNullOrEmpty(_usage))
        {
            builder.AddSection("Usage", _usage);
        }
        else
        {
            builder.AddUsage(arguments, _program);
        }

        builder.AddArguments(_knownArgs, "Positional arguments");
        builder.AddArguments(_knownOpts.ToList(), "Options");

        if (!string.IsNullOrEmpty(_epilogue))
        {
            builder.AddText(_epilogue);
        }

        writer.WriteLine(builder.Build());
    }

    private void WriteUsage(TextWriter writer)
    {
        var builder = new HelpBuilder(_configuration.HelpConfiguration);
        var arguments = ConcatArguments();

        if (!string.IsNullOrEmpty(_usage))
        {
            builder.AddSection("Usage", _usage);
        }
        else
        {
            builder.AddUsage(arguments, _program);
        }

        writer.WriteLine(builder.Build());
    }

    private static bool IsPositional(string s) => s.Length > 0 && s[0] != '-';

    private static bool IsBundle(string s) => s.Length > 2 && s[0] == '-' && s[1] != '-';

    /// <summary>
    /// Registers a custom argument action handler.
    /// </summary>
    /// <param name="name">the name of the action.</param>
    /// <param name="action">the action handler instance.</param>
    /// <returns>The current <see cref="ArgumentParser"/> instance, allowing for chaining.</returns>
    public ArgumentParser AddAction(string name, ArgumentAction action)
    {
        ActionRegistry.Add(name, action);
        return this;
    }
}
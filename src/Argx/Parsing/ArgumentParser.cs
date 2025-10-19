using Argx.Actions;
using Argx.Errors;
using Argx.Extensions;
using Argx.Help;
using Argx.Store;

namespace Argx.Parsing;

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

    public ArgumentParser AddArgument<T>(string name, string? usage = null, string? dest = null)
    {
        if (name.IsOption())
        {
            throw new InvalidOperationException($"Invalid positional argument {name}: cannot start with '-'");
        }

        return Add<T>(name: name, usage: usage, dest: dest);
    }

    public ArgumentParser AddArgument(string name, string? usage = null, string? dest = null)
        => AddArgument<string>(name: name, usage: usage, dest: dest);

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

    public ArgumentParser AddAction(string name, ArgumentAction action)
    {
        ActionRegistry.Add(name, action);
        return this;
    }
}
using Argx.Actions;
using Argx.Extensions;
using Argx.Store;

namespace Argx.Parsing;

public class ArgumentParser
{
    private readonly List<Argument> _knownOpts = [];
    private readonly Queue<Argument> _knowsArgs = [];
    private readonly IArgumentRepository _repository;

    public string? Program { get; }
    public string? Description { get; }
    public string? Usage { get; }
    public string? Epilogue { get; }
    public bool AddHelpArgument { get; }
    public bool ExitOnError { get; }

    public ArgumentParser(
        string? program = null,
        string? description = null,
        string? usage = null,
        string? epilogue = null,
        bool addHelp = true,
        bool exitOnError = true)
    {
        Program = program;
        Description = description;
        Usage = usage;
        Epilogue = epilogue;
        AddHelpArgument = addHelp;
        ExitOnError = exitOnError;
        _repository = new ArgumentRepository();
    }

    internal ArgumentParser(
        IArgumentRepository repository,
        string? program = null,
        string? description = null,
        string? usage = null,
        string? epilogue = null,
        bool addHelp = true,
        bool exitOnError = true)
        : this(program, description, usage, epilogue, addHelp, exitOnError)
    {
        _repository = repository;
    }

    public ArgumentParser Add(
        string name,
        string[]? alias = null,
        string? action = null,
        string? usage = null,
        string? dest = null,
        string? defaultValue = null,
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
            defaultValue: defaultValue,
            constValue: constValue,
            choices: choices,
            arity: arity);
    }

    public ArgumentParser Add<T>(
        string name,
        string[]? alias = null,
        string? usage = null,
        string? dest = null,
        string? defaultValue = null,
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
            throw new InvalidOperationException("Positional arguments cannot have an alias");
        }

        var arg = new Argument(
            name: name,
            alias: alias,
            action: action,
            dest: dest,
            arity: arity,
            usage: usage,
            defaultVal: defaultValue,
            constValue: constValue,
            choices: choices,
            isRequired: isPositional,
            type: typeof(T));

        if (!ActionRegistry.TryGetHandler(arg.Action, out var handler))
        {
            throw new ArgumentException($"Argument {arg.Name}: Unknown action '{action}'");
        }

        handler!.Validate(arg);

        if (isPositional)
        {
            _knowsArgs.Enqueue(arg);
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
        string? defaultValue = null,
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
            defaultValue: defaultValue,
            constValue: constValue,
            action: action,
            arity: arity);
    }

    public ArgumentParser AddOption(
        string name,
        string[]? alias = null,
        string? usage = null,
        string? dest = null,
        string? defaultValue = null,
        object? constValue = null,
        string? action = null,
        string? arity = null)
    {
        return AddOption<string>(
            name: name,
            alias: alias,
            usage: usage,
            dest: dest,
            defaultValue: defaultValue,
            constValue: constValue,
            action: action,
            arity: arity);
    }

    public Arguments Parse(string[] args)
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
                ConsumePositional(tokens[i], result);
            }
        }

        return result;
    }

    private void ConsumePositional(Token token, Arguments result)
    {
        if (_knowsArgs.Count == 0)
        {
            result.Extras.Add(token.Value);
            return;
        }

        var arg = _knowsArgs.Dequeue();

        if (arg.Arity != 1)
        {
            throw new InvalidOperationException($"Argument {arg.Name}: arity for positional arguments should be 1");
        }

        var handler = new StoreAction();

        handler.Execute(
            argument: arg,
            repository: _repository,
            tokens: [new Token(arg.Name, TokenType.Argument, Token.ImplicitPosition), token]);
    }

    private int ConsumeOption(int idx, ReadOnlySpan<Token> tokens, Arguments result)
    {
        var token = tokens[idx];
        var arg = _knownOpts.FirstOrDefault(a => a.Name == token.Value || a.Aliases?.Contains(token.Value) == true);

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

        handler!.Execute(arg, _repository, tokens.Slice(idx, len + 1));

        return len;
    }

    private static int ParseArity(Argument arg, ReadOnlySpan<Token> tokens, int from)
    {
        if (arg.Arity.IsFixed)
        {
            return int.Parse(arg.Arity.Value);
        }

        var count = 0;
        var idx = from + 1;

        switch (arg.Arity.Value)
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

                if (arg.Arity.Value == Arity.AtLeastOne && count == 0)
                {
                    throw new InvalidOperationException($"Argument '{arg.Name}' requires at least one value.");
                }

                return count;

            default:
                throw new InvalidOperationException($"Unknown arity value: {arg.Arity.Value}");
        }
    }

    private static bool IsPositional(string s) => s.Length > 0 && s[0] != '-';

    public ArgumentParser AddAction(string name, ArgumentAction action)
    {
        ActionRegistry.Add(name, action);
        return this;
    }
}
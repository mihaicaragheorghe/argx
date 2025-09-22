using Argx.Actions;
using Argx.Binding;
using Argx.Extensions;

namespace Argx.Parsing;

public class ArgumentParser
{
    private readonly List<Argument> _knownOpts = [];
    private readonly Queue<Argument> _knowsArgs = [];
    private readonly IArgumentRepository _repository;

    public ArgumentParser()
    {
        _repository = new ArgumentRepository();
    }

    public ArgumentParser(IArgumentRepository repository)
    {
        _repository = repository;
    }

    public ArgumentParser Add(
        string name,
        string? alias = null,
        string? action = null,
        string? usage = null,
        string? dest = null,
        string? defaultValue = null,
        object? constValue = null,
        string[]? choices = null,
        int? arity = null)
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
        string? alias = null,
        string? usage = null,
        string? dest = null,
        string? defaultValue = null,
        object? constValue = null,
        string? action = null,
        string[]? choices = null,
        int? arity = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Argument name cannot be null or empty", nameof(name));

        if (!IsValidAlias(alias))
            throw new ArgumentException(
                $"Invalid alias {alias}: it must start with '-' and the second character must be a letter, e.g. '-h', '-v'");

        var isPositional = IsPositional(name);

        if (isPositional && alias != null)
            throw new InvalidOperationException("Positional arguments cannot have an alias");

        var argument = new Argument(
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

        if (isPositional)
            _knowsArgs.Enqueue(argument);
        else
            _knownOpts.Add(argument);

        return this;
    }

    public ArgumentParser AddArgument<T>(string name, string? usage = null, string? dest = null)
    {
        if (IsOption(name))
        {
            throw new InvalidOperationException(
                $"Positional arguments should not start with '-', consider options instead. Argument name: {name}");
        }

        return Add<T>(name: name, usage: usage, dest: dest);
    }

    public ArgumentParser AddArgument(string name, string? usage = null, string? dest = null)
        => AddArgument<string>(name: name, usage: usage, dest: dest);

    public ArgumentParser AddFlag(
        string name,
        string? alias = null,
        string? usage = null,
        string? dest = null,
        bool value = true)
    {
        if (!IsOption(name))
        {
            throw new InvalidOperationException(
                $"Flags should start with '-', since they are optional arguments, {name} should be --{name}");
        }

        return Add<bool>(
            name: name,
            alias: alias,
            usage: usage,
            dest: dest,
            constValue: value,
            action: value ? ArgumentActions.StoreTrue : ArgumentActions.StoreFalse,
            arity: 0);
    }

    public ArgumentParser AddOption<T>(
        string name,
        string? alias = null,
        string? usage = null,
        string? dest = null,
        string? defaultValue = null,
        object? constValue = null,
        string? action = null,
        int? arity = null)
    {
        if (!IsOption(name))
        {
            throw new InvalidOperationException(
                $"Optional arguments should start with '-', {name} should be --{name}");
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
        string? alias = null,
        string? usage = null,
        string? dest = null,
        string? defaultValue = null,
        object? constValue = null,
        string? action = null,
        int? arity = null)
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
            if (IsSeparator(tokens[i].Value))
            {
                consumeOpts = false;
                continue;
            }

            if (consumeOpts && IsOption(tokens[i].Value))
            {
                i += ConsumeOption(i, tokens, result);
                continue;
            }

            ConsumePositional(tokens[i], result);
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

        TokenConversionResult conversionResult = TokenConverter.ConvertObject(arg.Type, token);

        if (conversionResult.IsError)
            throw new InvalidCastException(
                $"Could not convert argument {arg.Name} to type {arg.Type} in order to store. {conversionResult.Error}");

        _repository.Set(arg.Dest, conversionResult.Value!);
    }

    private int ConsumeOption(int idx, ReadOnlySpan<Token> tokens, Arguments result)
    {
        var token = tokens[idx];
        var arg = _knownOpts.FirstOrDefault(a => a.Name == token.Value || a.Alias == token.Value);

        if (arg is null)
        {
            result.Extras.Add(token);
            return 0;
        }

        if (!ActionRegistry.TryGetHandler(arg.Action, out var handler))
        {
            throw new InvalidOperationException($"Unknown action for argument {arg}");
        }

        handler!.Execute(arg, _repository, tokens.Slice(idx, arg.Arity + 1));

        return arg.Arity;
    }

    private static bool IsPositional(string s) => !string.IsNullOrEmpty(s) && s[0] != '-';

    private static bool IsOption(string s) => s[0] == '-' && s != "--";

    private static bool IsSeparator(string s) => s == "--";

    private bool IsValidAlias(string? s) => s is null
        || (s.Length == 2 && s[0] == '-' && char.IsLetter(s[1]))
        || (s.Length == 3 && s[0] == '-' && char.IsLetter(s[1]) && char.IsDigit(s[2]));

    public ArgumentParser AddAction(string name, ArgumentAction action)
    {
        ActionRegistry.Add(name, action);
        return this;
    }
}
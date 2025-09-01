using Argx.Actions;

namespace Argx.Parsing;

public class ArgumentParser
{
    private readonly List<Argument> _knownArgs = [];
    private readonly ArgumentStore _store = new();

    public ArgumentParser Add(
        string arg,
        string? alias = null,
        string? action = null,
        string? usage = null,
        string? defaultVal = null,
        string? constValue = null,
        string[]? choices = null,
        int? arity = null)
    {
        return Add<string>(
            arg: arg,
            alias: alias,
            action: action,
            usage: usage,
            defaultVal: defaultVal,
            constValue: constValue,
            choices: choices,
            arity: arity);
    }

    public ArgumentParser Add<T>(
        string arg,
        string? alias = null,
        string? usage = null,
        string? defaultVal = null,
        string? constValue = null,
        string? action = null,
        string[]? choices = null,
        int? arity = null)
    {
        if (string.IsNullOrWhiteSpace(arg))
            throw new ArgumentException("Argument name cannot be null or empty", nameof(arg));

        _knownArgs.Add(new Argument(
            name: arg,
            alias: alias,
            action: action,
            usage: usage,
            defaultVal: defaultVal,
            constValue: constValue,
            choices: choices,
            isRequired: IsPositional(arg),
            type: typeof(T)));

        return this;
    }

    public ArgumentParser AddAction(string name, ArgumentAction action)
    {
        ActionRegistry.Add(name, action);
        return this;
    }

    private bool IsPositional(string arg) => arg[0] == '-';
}
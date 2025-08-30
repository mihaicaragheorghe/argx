using Argx.Actions;

namespace Argx.Parsing;

public class ArgumentParser
{
    private readonly List<Argument> _knownArgs = [];

    public ArgumentParser Add(
        string arg,
        string? shorten = null,
        string? usage = null,
        string? defaultVal = null,
        string action = ArgumentActions.Store,
        int narg = 1)
    {
        if (string.IsNullOrWhiteSpace(arg))
            throw new ArgumentException("Argument name cannot be null or empty", nameof(arg));

        _knownArgs.Add(new Argument(
            name: arg,
            shorten: shorten,
            action: action,
            usage: usage,
            defaultVal: defaultVal,
            isRequired: IsPositional(arg),
            type: typeof(string)));

        return this;
    }

    public ArgumentParser Add<T>(
        string arg,
        string? shorten = null,
        string? usage = null,
        string? defaultVal = null,
        string action = ArgumentActions.Store,
        int narg = 1)
    {
        if (string.IsNullOrWhiteSpace(arg))
            throw new ArgumentException("Argument name cannot be null or empty", nameof(arg));

        if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
            narg = 0;

        _knownArgs.Add(new Argument(
            name: arg,
            shorten: shorten,
            action: action,
            usage: usage,
            defaultVal: defaultVal,
            isRequired: IsPositional(arg),
            type: typeof(T)));

        return this;
    }

    private bool IsPositional(string arg) => arg[0] == '-';
}
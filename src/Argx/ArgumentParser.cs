using Argx.Actions;
using Argx.Internal;

namespace Argx;

public class ArgumentParser
{
    private readonly List<Argument> _knownArgs = [];

    public ArgumentParser Add(
        string arg,
        string? shorten = null,
        string? usage = null,
        string? defaultVal = null,
        string action = ArgumentActions.Store)
    {
        if (string.IsNullOrWhiteSpace(arg))
            throw new ArgumentException("Argument name cannot be null or empty", nameof(arg));

        _knownArgs.Add(new Argument(
            name: arg,
            shorten: shorten,
            action: action,
            usage: usage,
            defaultVal: defaultVal,
            isRequired: IsPositional(arg)));

        return this;
    }

    public ArgumentParser Add<T>(
        string arg,
        string? shorten = null,
        string? usage = null,
        string? defaultVal = null,
        string action = ArgumentActions.Store)
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
            type: ArgType(typeof(T))));

        return this;
    }

    private ArgumentType ArgType(Type type)
    {
        return type.Name switch
        {
            _ when type == typeof(string) => ArgumentType.String,
            _ when type == typeof(bool) => ArgumentType.Bool,
            _ when type == typeof(short) => ArgumentType.Int16,
            _ when type == typeof(ushort) => ArgumentType.Uint16,
            _ when type == typeof(int) => ArgumentType.Int32,
            _ when type == typeof(uint) => ArgumentType.Uint32,
            _ when type == typeof(long) => ArgumentType.Int64,
            _ when type == typeof(ulong) => ArgumentType.Uint64,
            _ when type == typeof(float) => ArgumentType.Float,
            _ when type == typeof(double) => ArgumentType.Double,
            _ when type == typeof(decimal) => ArgumentType.Decimal,
            _ => throw new InvalidOperationException($"Unsupported argument type: {type.Name}") // TODO: add supported types list
        };
    }

    private bool IsPositional(string arg) => arg[0] == '-';

    public static T ParseValue<T>(string arg, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Console.WriteLine($"Value cannot be null or empty for argument {arg}");
            Environment.Exit(1);
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
        {
            Console.WriteLine($"Cannot convert '{value}' to type {typeof(T).Name}");
            Environment.Exit(1);
            throw new InvalidCastException($"Cannot convert '{value}' to type {typeof(T).Name}", ex);
        }
    }
}
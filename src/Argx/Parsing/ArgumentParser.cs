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
        string arg,
        string? alias = null,
        string? action = null,
        string? usage = null,
        string? dest = null,
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
            dest: dest,
            defaultVal: defaultVal,
            constValue: constValue,
            choices: choices,
            arity: arity);
    }

    public ArgumentParser Add<T>(
        string arg,
        string? alias = null,
        string? usage = null,
        string? dest = null,
        string? defaultVal = null,
        string? constValue = null,
        string? action = null,
        string[]? choices = null,
        int? arity = null)
    {
        if (string.IsNullOrWhiteSpace(arg))
            throw new ArgumentException("Argument name cannot be null or empty", nameof(arg));

        var isPositional = IsPositional(arg);
        var argument = new Argument(
            name: arg,
            alias: alias,
            action: action,
            dest: dest,
            arity: arity,
            usage: usage,
            defaultVal: defaultVal,
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

        handler.Execute(arg, _repository, tokens.Slice(idx, arg.Arity + 1));

        return arg.Arity;
    }

    private static bool IsPositional(string s) => !string.IsNullOrEmpty(s) && s[0] != '-';

    private static bool IsOption(string s) => s[0] == '-' && s != "--";

    private static bool IsSeparator(string s) => s == "--";

    public ArgumentParser AddAction(string name, ArgumentAction action)
    {
        ActionRegistry.Add(name, action);
        return this;
    }
}
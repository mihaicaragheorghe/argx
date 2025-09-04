using Argx.Actions;
using Argx.Extensions;

namespace Argx.Parsing;

public class ArgumentParser
{
    private readonly List<Argument> _knownArgs = [];
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

    public Arguments Parse(string[] args)
    {
        var tokens = args.Tokenize();
        var positionals = new List<Token>();

        for (int i = 0; i < tokens.Length; i++)
        {
            if (IsPositional(tokens[i].Value))
            {
                positionals.Add(tokens[i]);
                continue;
            }

            var token = tokens[i];
            var arg = _knownArgs.FirstOrDefault(a => a.Name == token.Value || a.Alias == token.Value);

            if (arg is null) continue;

            if (!ActionRegistry.TryGetHandler(arg.Action, out var handler))
            {
                throw new InvalidOperationException($"Unknown action for argument {arg}");
            }

            handler.Execute(arg, _repository, arg.Name, tokens);
            i += arg.Arity;
        }

        return new Arguments(_repository);
    }

    public ArgumentParser AddAction(string name, ArgumentAction action)
    {
        ActionRegistry.Add(name, action);
        return this;
    }

    private bool IsPositional(string arg) => arg[0] == '-';
}
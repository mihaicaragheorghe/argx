using System.Collections;
using System.Reflection;
using Argx.Binding;
using Argx.Errors;
using Argx.Extensions;
using Argx.Parsing;
using Argx.Utils;

namespace Argx.Actions;

public class AppendAction : ArgumentAction
{
    private static readonly MethodInfo s_tryGetValueGeneric = typeof(IArgumentRepository)
        .GetMethods()
        .First(m => m.Name == "TryGetValue" && m.IsGenericMethodDefinition);

    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        var name = tokens[0].Value;

        // TODO: arity = optional + const
        if (argument.Arity == 0)
            throw new InvalidOperationException($"Arity for 'append' must be != 0. Argument: {name}");

        if (tokens.Length < 2)
            throw new BadArgumentException(name, $"expected {(argument.Arity > 1 ? "at least " : "")}one value");

        if (!argument.Type.IsEnumerable())
            throw new InvalidOperationException($"Type for 'append' must be an enumerable. Argument: {name}");

        var genericMethod = s_tryGetValueGeneric.MakeGenericMethod(argument.Type);
        var itemType = argument.Type.GetElementTypeIfEnumerable()!;
        object[] parameters = [argument.Dest, null!];
        var found = (bool)genericMethod.Invoke(repository, parameters);
        var obj = found ? parameters[1] : null;

        IList list;
        var idx = 0;

        if (obj is IList existing)
        {
            list = CollectionUtils.CreateCollection(argument.Type, itemType, existing.Count + argument.Arity);
            CopyItems(existing, list);
            idx = existing.Count;
        }
        else
        {
            list = obj is null
                ? CollectionUtils.CreateCollection(argument.Type, itemType, argument.Arity)
                : throw new InvalidOperationException(
                    $"Invalid state: appending to value of argument '{name}' not possible, its type is not IList");
        }

        var isArray = list is Array;

        for (int i = 1; i < tokens.Length; i++)
        {
            TokenConversionResult result = TokenConverter.ConvertObject(itemType, tokens[i]);

            if (result.IsError)
            {
                throw new BadArgumentException(name,
                    $"invalid value '{tokens[i]}', expected type {argument.Type.GetFriendlyName()}. {result.Error}");
            }

            if (isArray)
            {
                list[idx] = result.Value;
                idx++;
            }
            else
            {
                list.Add(result.Value);
            }
        }

        repository.Set(argument.Dest, list);
    }

    private void CopyItems(IList src, IList dest)
    {
        var idx = 0;
        var isArray = dest is Array;
        foreach (var item in src)
        {
            if (isArray)
            {
                dest[idx] = item;
                idx++;
            }
            else
            {
                dest.Add(item);
            }
        }
    }
}
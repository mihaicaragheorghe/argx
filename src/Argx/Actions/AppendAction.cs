using System.Collections;
using System.Reflection;

using Argx.Binding;
using Argx.Errors;
using Argx.Extensions;
using Argx.Parsing;
using Argx.Store;
using Argx.Utils;

namespace Argx.Actions;

internal class AppendAction : ArgumentAction
{
    private static readonly MethodInfo TryGetValueMethod = typeof(IArgumentRepository)
        .GetMethods()
        .First(m => m.Name == "TryGetValue" && m.IsGenericMethodDefinition);

    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        var name = tokens[0].Value;

        if (argument.ConstValue == null && tokens.Length < 2)
        {
            throw new ArgumentValueException(name, $"expected value");
        }

        var genericMethod = TryGetValueMethod.MakeGenericMethod(argument.Type);
        var itemType = argument.Type.GetElementTypeIfEnumerable()!;
        object[] parameters = [argument.Dest, null!];
        var found = (bool)genericMethod.Invoke(repository, parameters)!;
        var obj = found ? parameters[1] : null;
        var len = tokens.Length == 1 ? GetConstValueLength(argument.ConstValue!) : tokens.Length - 1;

        IList list;
        var idx = 0;

        if (obj is IList existing)
        {
            list = CollectionUtils.CreateCollection(argument.Type, itemType, existing.Count + len);
            CopyItems(existing, list);
            idx = existing.Count;
        }
        else
        {
            list = obj is null
                ? CollectionUtils.CreateCollection(argument.Type, itemType, len)
                : throw new InvalidOperationException(
                    $"Invalid state: appending to value of argument '{name}' not possible, its type is not IList");
        }

        var isArray = list is Array;

        if (tokens.Length == 1)
        {
            if (itemType.IsInstanceOfType(argument.ConstValue))
            {
                Append(list, argument.ConstValue, isArray, idx);
                idx++;
            }
            else if (argument.ConstValue is IList constEnumerable)
            {
                foreach (var item in constEnumerable)
                {
                    if (!itemType.IsInstanceOfType(item))
                    {
                        var itemTypeName = itemType.GetFriendlyName();
                        throw new ArgumentValueException(name,
                            $"const value for argument '{name}' must be either an enumerable of type {itemTypeName} or an instance of type {itemTypeName}");
                    }

                    Append(list, item, isArray, idx);
                    idx++;
                }
            }
            else
            {
                var itemTypeName = itemType.GetFriendlyName();
                throw new ArgumentValueException(name,
                    $"const value for argument '{name}' must be either an enumerable of type {itemTypeName} or an instance of type {itemTypeName}");
            }
        }

        for (int i = 1; i < tokens.Length; i++)
        {
            TokenConversionResult result = TokenConverter.ConvertObject(itemType, tokens[i]);

            if (result.IsError)
            {
                throw new ArgumentValueException(name,
                    $"invalid value '{tokens[i]}', expected type {argument.Type.GetFriendlyName()}. {result.Error}");
            }

            Append(list, result.Value, isArray, idx);
            idx++;
        }

        repository.Set(argument.Dest, list);
    }

    private static void Append(IList list, object? value, bool isArray, int idx)
    {
        if (isArray)
        {
            list[idx] = value;
        }
        else
        {
            list.Add(value);
        }
    }

    private static void CopyItems(IList src, IList dest)
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

    public override void Validate(Argument argument)
    {
        if (argument.Arity == 0)
        {
            throw new ArgumentException($"Argument: {argument.Name}: arity for 'append' must be != 0.");
        }

        if (!argument.Type.IsEnumerable())
        {
            throw new ArgumentException($"Argument: {argument.Name}: Type for 'append' must be an enumerable.");
        }

        if (argument.ConstValue != null && !argument.Arity.IsOptional)
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: arity must be {Arity.Optional} or {Arity.Any} to supply a const value");
        }
    }

    private static int GetConstValueLength(object value) => value is IList list ? list.Count : 1;
}
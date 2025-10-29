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

    public override void Execute(Argument argument, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
        base.Execute(argument, invocation, values, store);

        if (argument.ConstValue == null && values.Length == 0)
        {
            throw new ArgumentValueException(invocation, "expected value");
        }

        var genericMethod = TryGetValueMethod.MakeGenericMethod(argument.ValueType);
        var itemType = argument.ValueType.GetElementTypeIfEnumerable()!;
        object[] parameters = [argument.Dest, null!];
        var found = (bool)genericMethod.Invoke(store, parameters)!;
        var obj = found ? parameters[1] : null;
        var len = values.Length == 0 ? GetConstValueLength(argument.ConstValue!) : values.Length;

        IList list;
        var idx = 0;

        if (obj is IList src)
        {
            list = CollectionUtils.CreateCollection(argument.ValueType, itemType, src.Count + len);
            CopyItems(src, list);
            idx = src.Count;
        }
        else
        {
            list = obj is null
                ? CollectionUtils.CreateCollection(argument.ValueType, itemType, len)
                : throw new InvalidOperationException(
                    $"Argument {invocation} in invalid state: appending to non enumerable type not possible");
        }

        var isArray = list is Array;

        if (values.Length == 0)
        {
            if (itemType.IsInstanceOfType(argument.ConstValue))
            {
                Append(list, argument.ConstValue, isArray, idx);
                idx++;
            }
            else if (argument.ConstValue is IList constList)
            {
                foreach (var item in constList)
                {
                    Append(list, item, isArray, idx);
                    idx++;
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid state for const value in append action");
            }
        }

        foreach (var value in values)
        {
            TokenConversionResult result = TokenConverter.ConvertObject(itemType, value);

            if (result.IsError)
            {
                throw new ArgumentValueException(invocation,
                    $"invalid value '{value}', expected type {argument.ValueType.GetFriendlyName()}. {result.Error}");
            }

            if (argument.Choices?.Length > 0 && !argument.Choices.Contains(result.Value?.ToString()))
            {
                throw new ArgumentValueException(invocation,
                    $"invalid choice '{value}', expected one of: {string.Join(", ", argument.Choices)}");
            }

            Append(list, result.Value, isArray, idx);
            idx++;
        }

        store.Set(argument.Dest, list);
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
            throw new ArgumentException($"Argument {argument.Name}: arity for 'append' must be != 0");
        }

        if (!argument.ValueType.IsEnumerable())
        {
            throw new ArgumentException($"Argument {argument.Name}: type for 'append' must be an enumerable");
        }

        if (argument.ConstValue != null && !argument.Arity.IsOptional)
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: arity must be {Arity.Optional} or {Arity.Any} to supply a const value");
        }

        if (argument.ConstValue != null && !IsValidConstType(argument))
        {
            var itemTypeName = argument.ValueType.GetElementTypeIfEnumerable()!.GetFriendlyName();
            throw new ArgumentException(
                $"Argument {argument.Name}: const value must be either an enumerable of type {itemTypeName} or an instance of type {itemTypeName}");
        }
    }

    private static bool IsValidConstType(Argument argument)
    {
        var itemType = argument.ValueType.GetElementTypeIfEnumerable()!;

        if (argument.ConstValue is IList constList)
        {
            var type = constList.GetType().GetElementTypeIfEnumerable()!;

            if (!itemType.IsAssignableFrom(type))
            {
                return false;
            }
        }
        else if (!itemType.IsInstanceOfType(argument.ConstValue))
        {
            return false;
        }

        return true;
    }

    private static int GetConstValueLength(object value) => value is IList list ? list.Count : 1;
}
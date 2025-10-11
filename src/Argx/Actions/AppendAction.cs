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

    public override void Execute(Argument arg, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
        base.Execute(arg, invocation, values, store);

        if (arg.ConstValue == null && values.Length == 0)
        {
            throw new ArgumentValueException(invocation, "expected value");
        }

        var genericMethod = TryGetValueMethod.MakeGenericMethod(arg.Type);
        var itemType = arg.Type.GetElementTypeIfEnumerable()!;
        object[] parameters = [arg.Dest, null!];
        var found = (bool)genericMethod.Invoke(store, parameters)!;
        var obj = found ? parameters[1] : null;
        var len = values.Length == 0 ? GetConstValueLength(arg.ConstValue!) : values.Length;

        IList list;
        var idx = 0;

        if (obj is IList src)
        {
            list = CollectionUtils.CreateCollection(arg.Type, itemType, src.Count + len);
            CopyItems(src, list);
            idx = src.Count;
        }
        else
        {
            list = obj is null
                ? CollectionUtils.CreateCollection(arg.Type, itemType, len)
                : throw new InvalidOperationException(
                    $"Argument {invocation} in invalid state: appending to non enumerable type not possible");
        }

        var isArray = list is Array;

        if (values.Length == 0)
        {
            if (itemType.IsInstanceOfType(arg.ConstValue))
            {
                Append(list, arg.ConstValue, isArray, idx);
                idx++;
            }
            else if (arg.ConstValue is IList constList)
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
                    $"invalid value '{value}', expected type {arg.Type.GetFriendlyName()}. {result.Error}");
            }

            Append(list, result.Value, isArray, idx);
            idx++;
        }

        store.Set(arg.Dest, list);
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

        if (!argument.Type.IsEnumerable())
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
            var itemTypeName = argument.Type.GetElementTypeIfEnumerable()!.GetFriendlyName();
            throw new ArgumentException(
                $"Argument {argument.Name}: const value must be either an enumerable of type {itemTypeName} or an instance of type {itemTypeName}");
        }
    }

    private static bool IsValidConstType(Argument argument)
    {
        var itemType = argument.Type.GetElementTypeIfEnumerable()!;

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
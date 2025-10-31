using System.Linq.Expressions;
using System.Reflection;

namespace Argx.Subcommands;

internal static class SubcommandDelegateFactory
{
    private static readonly MethodInfo FromResultMethod = typeof(Task).GetMethod("FromResult")!.MakeGenericMethod(typeof(int));
    private static readonly MethodInfo ValueTaskAsTaskMethod = typeof(ValueTask<int>).GetMethod("AsTask")!;

    internal static SubcommandDelegate Create(Delegate handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var method = handler.Method;
        var parameters = method.GetParameters();

        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string[]))
        {
            throw new ArgumentException(
                $"Handler must have exactly one parameter of type string[]. " +
                $"Found {parameters.Length} parameters: {string.Join(", ", parameters.Select(p => p.ParameterType.Name))}");
        }

        var argsParam = Expression.Parameter(typeof(string[]), "args");
        var target = handler.Target != null ? Expression.Constant(handler.Target) : null;
        MethodCallExpression call = Expression.Call(target, method, argsParam);

        Expression body = method.ReturnType switch
        {
            Type t when t == typeof(Task<int>) => call,
            Type t when t == typeof(ValueTask<int>) =>
                Expression.Call(call, ValueTaskAsTaskMethod),
            Type t when t == typeof(int) =>
                Expression.Call(FromResultMethod, call),
            Type t when t == typeof(void) =>
                Expression.Block(
                    call,
                    Expression.Call(FromResultMethod, Expression.Constant(0))),
            _ => throw new ArgumentException($"Unsupported return type: {method.ReturnType.Name}")
        };

        var lambda = Expression.Lambda<SubcommandDelegate>(body, argsParam);
        return lambda.Compile();
    }
}
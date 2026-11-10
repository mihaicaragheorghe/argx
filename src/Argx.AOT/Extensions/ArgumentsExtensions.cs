using Argx.Binding;

namespace Argx.Extensions;

public static class ArgumentsExtensions
{
    public static T Get<T>(this IArguments args) where T : class, new()
    {
        var value = ArgumentBindingRegistry.Default.Get<T>().Create();
        args.Bind(value);
        return value;
    }

    public static IArguments Bind<T>(this IArguments args, T instance) where T : class
    {
        ArgumentBindingRegistry.Default.Bind(args, instance, "");
        return args;
    }

    public static T Get<T>(this IArguments args, ArgumentBinding<T> binding) where T : class
    {
        var value = binding.Create();
        binding.Bind(args, value);
        return value;
    }

    public static IArguments Bind<T>(this IArguments args, T instance, ArgumentBinding<T> binding) where T : class
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance));
        binding.Bind(args, instance);
        return args;
    }
}

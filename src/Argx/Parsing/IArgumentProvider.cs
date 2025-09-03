namespace Argx.Parsing;

public interface IArgumentProvider
{
    void Add(string arg, object val);

    bool TryGetValue<T>(string arg, out T val);

    bool TryGetValue(string arg, out string? value);
}
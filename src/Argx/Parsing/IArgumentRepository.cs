namespace Argx.Parsing;

public interface IArgumentRepository
{
    void Set(string arg, object value);

    bool TryGetValue<T>(string arg, out T value);

    bool TryGetValue(string arg, out string? value);
}
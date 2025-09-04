namespace Argx.Parsing;

public class Arguments
{
    private readonly IArgumentRepository _repository;

    public Arguments(IArgumentRepository repository)
    {
        _repository = repository;
    }

    public string? this[string key] => GetValue(key);

    public string? GetValue(string arg)
    {
        if (_repository.TryGetValue(arg, out var value))
        {
            return value;
        }

        return null;
    }

    public T GetValue<T>(string arg) => TryGetValue<T>(arg, out T value) ? value : default!;

    public bool TryGetValue<T>(string arg, out T value) => _repository.TryGetValue(arg, out value);
}
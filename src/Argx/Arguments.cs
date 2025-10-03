using Argx.Store;

namespace Argx;

public class Arguments
{
    private readonly IArgumentRepository _repository;

    public List<string> Extras { get; }

    public Arguments(IArgumentRepository repository)
    {
        _repository = repository;
        Extras = [];
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

    public T GetValue<T>(string arg) => TryGetValue(arg, out T value) ? value : default!;

    public bool TryGetValue<T>(string arg, out T value) => _repository.TryGetValue(arg, out value);
}
namespace Argx;

public class Arguments
{
    private readonly Dictionary<string, string> _args = new();

    public string? Get(string arg)
    {
        if (_args.TryGetValue(arg, out string? value))
        {
            return value;
        }

        return null;
    }
}
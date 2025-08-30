namespace Argx.Parsing;

public class Arguments
{
    private readonly Dictionary<string, string> _args = new();
    
    public string? Get(string arg) => _args.GetValueOrDefault(arg);
}
using Argx.Parsing;

using BenchmarkDotNet.Attributes;

namespace Argx.Benchmarks;

[MemoryDiagnoser]
public class Demo
{
    [Benchmark]
    public void TwoArguments()
    {
        string[] args = ["foo.txt", "-c", "4"];
        var parser = new ArgumentParser(program: "Argx", description: "Argx playground");
        parser.AddArgument("filename", usage: "the file name");
        parser.AddFlag("--verbose", ["-v"], usage: "verbose argument, on/off flag");

        var argx = parser.Parse(args);
    }

    [Benchmark]
    public void FourArguments()
    {
        string[] args = ["123", "456", "-c", "4", "-v"];
        var parser = new ArgumentParser(program: "Argx", description: "Argx playground");
        parser.AddArgument<string>("x", usage: "The X argument");
        parser.AddArgument<string>("y", usage: "The Y argument");
        parser.AddOption<int>("--count", ["-c"], usage: "Count argument");
        parser.AddFlag("--verbose", ["-v"], usage: "verbose argument, on/off flag");

        var argx = parser.Parse(args);
    }

    [Benchmark]
    public void EightArguments()
    {
        string[] args = ["foo", "bar", "baz", "qux", "-c", "4", "--file", "foo.txt", "-v", "-d"];
        var parser = new ArgumentParser(program: "Argx", description: "Argx playground");
        parser.AddArgument<string>("foo", usage: "The foo argument");
        parser.AddArgument<string>("bar", usage: "The bar argument");
        parser.AddArgument<string>("baz", usage: "The baz argument");
        parser.AddArgument<string>("qux", usage: "The qux argument");
        parser.AddOption<int>("--count", ["-c"], usage: "Count argument");
        parser.AddOption<string>("--file", ["-f"], usage: "File argument");
        parser.AddFlag("--verbose", ["-v"], usage: "verbose argument, on/off flag");
        parser.AddFlag("-d", usage: "detached argument, on/off flag");

        var argx = parser.Parse(args);
    }
}
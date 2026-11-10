using Argx.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

public class GeneratorTests
{
    private const string Preamble = "using Argx.Attributes; using Argx.Binding; [ArgxBinding(typeof(Model))] partial class Context : ArgumentBindingContext {} ";
    private static GeneratorDriverRunResult Generate(string model, out Compilation compiled)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p)).ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(Argx.IArguments).Assembly.Location));
        var compilation = CSharpCompilation.Create("GeneratedTest", [CSharpSyntaxTree.ParseText(Preamble + model)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new BindingGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out compiled, out _);
        return driver.GetRunResult();
    }

    [Theory]
    [InlineData("class Model { public int Value {get; private set;} }")]
    [InlineData("class Model { public int Value {get; init;} }")]
    [InlineData("class Model { public Model Child {get; set;} }")]
    [InlineData("class Model { public System.Collections.Generic.HashSet<int> Values {get;set;} }")]
    public void InvalidModelsProduceDiagnostic(string model)
    {
        var result = Generate(model, out _);
        Assert.Contains(result.Diagnostics, d => d.Id == "ARGX001");
    }

    [Fact]
    public void OutputCompilesAndIsDeterministic()
    {
        const string model = "class Model { [Argument(\"a\\\"b\")] public int @event {get;set;} public int this[int i] {get=>0;set{}} }";
        var first = Generate(model, out var compiled);
        var second = Generate(model, out _);
        Assert.Empty(first.Diagnostics);
        Assert.DoesNotContain(compiled.GetDiagnostics(), d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal(first.GeneratedTrees.Single().ToString(), second.GeneratedTrees.Single().ToString());
    }
}

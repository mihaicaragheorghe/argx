using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Argx.Generators;

[Generator]
public sealed class BindingGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor Invalid = new("ARGX001", "Unsupported binding model",
        "{0}", "Argx", DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var contexts = context.SyntaxProvider.ForAttributeWithMetadataName("Argx.Attributes.ArgxBindingAttribute",
            (node, _) => node is ClassDeclarationSyntax,
            (ctx, _) => Generate(ctx));
        context.RegisterSourceOutput(contexts, (output, result) =>
        {
            if (result.Error != null) output.ReportDiagnostic(Diagnostic.Create(Invalid, Location.None, result.Error));
            else output.AddSource(result.Name + ".g.cs", result.Source!);
        });
    }

    private sealed class Result : IEquatable<Result>
    {
        public string Name = "";
        public string? Source;
        public string? Error;
        public bool Equals(Result? other) => other != null && Name == other.Name && Source == other.Source && Error == other.Error;
        public override bool Equals(object? obj) => Equals(obj as Result);
        public override int GetHashCode() => (Name + Source + Error).GetHashCode();
    }

    private static string TypeName(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    private static string Literal(string text) => SymbolDisplay.FormatLiteral(text, true);
    private static string Kebab(string text) => Regex.Replace(text, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", "-$1").ToLowerInvariant();
    private static bool Scalar(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol n && n.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) return Scalar(n.TypeArguments[0]);
        return type.SpecialType != SpecialType.None || type.TypeKind == TypeKind.Enum ||
            new[] { "System.Guid", "System.DateTime", "System.DateTimeOffset", "System.TimeSpan" }.Contains(type.ToDisplayString());
    }
    private static ITypeSymbol? Element(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol a && a.Rank == 1) return a.ElementType;
        if (type is INamedTypeSymbol n && n.IsGenericType && new[] { "System.Collections.Generic.List<T>", "System.Collections.Generic.IEnumerable<T>", "System.Collections.Generic.IList<T>", "System.Collections.Generic.ICollection<T>" }.Contains(n.OriginalDefinition.ToDisplayString())) return n.TypeArguments[0];
        return null;
    }

    private static Result Generate(GeneratorAttributeSyntaxContext ctx)
    {
        var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var result = new Result { Name = symbol.ToDisplayString().Replace('.', '_') };
        try
        {
            if (symbol.Arity != 0 || symbol.ContainingType != null || !((ClassDeclarationSyntax)ctx.TargetNode).Modifiers.Any(SyntaxKind.PartialKeyword) || symbol.BaseType?.ToDisplayString() != "Argx.Binding.ArgumentBindingContext")
                throw new InvalidOperationException("Binding context must be a top-level, non-generic partial class deriving from ArgumentBindingContext.");
            var compilation = ctx.SemanticModel.Compilation;
            bool Accessible(ISymbol s) => compilation.IsSymbolAccessibleWithin(s, symbol);
            bool Constructible(INamedTypeSymbol s) => !s.IsAbstract && s.InstanceConstructors.Any(c => c.Parameters.Length == 0 && Accessible(c)) && !s.GetMembers().OfType<IPropertySymbol>().Any(p => p.IsRequired);
            var models = new List<INamedTypeSymbol>();
            var bodies = new Dictionary<string, string>();
            var active = new HashSet<string>();
            var elements = new HashSet<string>();
            void Visit(INamedTypeSymbol model)
            {
                var name = TypeName(model);
                if (active.Contains(name)) throw new InvalidOperationException("Cyclic binding graph at " + name);
                if (bodies.ContainsKey(name)) return;
                if (model.TypeKind != TypeKind.Class || model.IsUnboundGenericType || model.TypeArguments.Any(t => t.TypeKind == TypeKind.TypeParameter) || !Accessible(model)) throw new InvalidOperationException("Inaccessible or unsupported model " + name);
                active.Add(name);
                models.Add(model);
                var b = new StringBuilder();
                var seen = new Dictionary<string, IPropertySymbol>();
                for (var current = model; current != null; current = current.BaseType)
                foreach (var p in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (p.IsStatic || p.IsIndexer || p.DeclaredAccessibility != Accessibility.Public || p.SetMethod == null || p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Argx.Attributes.IgnoreAttribute")) continue;
                    if (seen.TryGetValue(p.Name, out var previous))
                    {
                        if (previous.IsOverride) continue;
                        throw new InvalidOperationException("Hidden property " + name + "." + p.Name);
                    }
                    seen.Add(p.Name, p);
                    if (!Accessible(p.SetMethod) || p.SetMethod.IsInitOnly) throw new InvalidOperationException("Unsupported setter " + name + "." + p.Name);
                    var element = Element(p.Type);
                    if (element == null && p.Type.SpecialType != SpecialType.System_String &&
                        (p.Type is IArrayTypeSymbol || p.Type.AllInterfaces.Any(i => i.ToDisplayString() == "System.Collections.IEnumerable")))
                        throw new InvalidOperationException("Unsupported collection " + name + "." + p.Name);
                    var key = p.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Argx.Attributes.ArgumentAttribute")?.ConstructorArguments[0].Value as string;
                    var expression = key != null ? Literal(key) : "prefix + " + Literal(Kebab(p.Name));
                    var prop = "value.@" + p.Name;
                    if (Scalar(p.Type) || element != null)
                    {
                        if (element != null) elements.Add(TypeName(element));
                        b.AppendLine("if (args.TryGetValue<" + TypeName(p.Type) + ">(" + expression + ", out var v" + seen.Count + ")) " + prop + " = v" + seen.Count + ";");
                    }
                    else
                    {
                        if (!(p.Type is INamedTypeSymbol nested) || p.GetMethod == null || !Accessible(p.GetMethod)) throw new InvalidOperationException("Unsupported nested property " + name + "." + p.Name);
                        Visit(nested);
                        b.AppendLine("var child" + seen.Count + " = " + prop + ";");
                        b.AppendLine("if (child" + seen.Count + " is null) { " + (Constructible(nested) ? "child" + seen.Count + " = new " + TypeName(nested) + "(); " + prop + " = child" + seen.Count + ";" : "throw new global::System.MissingMethodException(" + Literal("Cannot construct " + TypeName(nested)) + ");") + " }");
                        b.AppendLine("resolver.Bind(args, child" + seen.Count + ", prefix + " + Literal(Kebab(p.Name) + "-") + ");");
                    }
                }
                bodies[name] = b.ToString();
                active.Remove(name);
            }
            var roots = ctx.Attributes.Select(a => (Model: (INamedTypeSymbol)a.ConstructorArguments[0].Value!, Name: a.NamedArguments.FirstOrDefault(p => p.Key == "Name").Value.Value as string)).ToArray();
            foreach (var root in roots) Visit(root.Model);
            var source = new StringBuilder("// <auto-generated/>\n#nullable enable\n");
            if (!symbol.ContainingNamespace.IsGlobalNamespace) source.AppendLine("namespace " + symbol.ContainingNamespace.ToDisplayString() + " {");
            source.AppendLine("partial class @" + symbol.Name + " {");
            source.AppendLine("public static " + TypeName(symbol) + " Default { get; } = new " + TypeName(symbol) + "();");
            for (int i = 0; i < models.Count; i++)
            {
                var t = TypeName(models[i]);
                source.AppendLine("private global::Argx.Binding.ArgumentBinding<" + t + ">? _b" + i + ";");
                source.AppendLine("private global::Argx.Binding.ArgumentBinding<" + t + "> B" + i + " => global::System.Threading.LazyInitializer.EnsureInitialized(ref _b" + i + ", () => new global::Argx.Binding.ArgumentBinding<" + t + ">(Bind" + i + ", " + (Constructible(models[i]) ? "() => new " + t + "()" : "null") + ", this));");
                source.AppendLine("private static void Bind" + i + "(global::Argx.IArguments args, " + t + " value, string prefix, global::Argx.Binding.IArgumentBindingResolver resolver) {" + bodies[t] + "}");
            }
            var names = new HashSet<string>(new[] { "Default", "Register", "RegisterCollections", "Bind", symbol.Name });
            for (int i = 0; i < models.Count; i++) { names.Add("B" + i); names.Add("_b" + i); names.Add("Bind" + i); }
            foreach (var root in roots)
            {
                var member = root.Name ?? root.Model.Name;
                if (!SyntaxFacts.IsValidIdentifier(member) || !names.Add(member)) throw new InvalidOperationException("Invalid or duplicate metadata name " + member + "; set ArgxBinding.Name.");
                source.AppendLine("public global::Argx.Binding.ArgumentBinding<" + TypeName(root.Model) + "> @" + member + " => B" + models.FindIndex(m => SymbolEqualityComparer.Default.Equals(m, root.Model)) + ";");
            }
            source.AppendLine("public override void Bind(global::Argx.IArguments args, object instance, string prefix) { if (instance is null) throw new global::System.ArgumentNullException(nameof(instance));");
            for (int i = 0; i < models.Count; i++) source.AppendLine("if (instance.GetType() == typeof(" + TypeName(models[i]) + ")) { B" + i + ".BindDeclared(args, (" + TypeName(models[i]) + ")instance, prefix, this); return; }");
            source.AppendLine("throw new global::System.InvalidOperationException(\"Unknown runtime binding type: \" + instance.GetType()); }");
            source.AppendLine("public override void RegisterCollections() {");
            foreach (var element in elements.OrderBy(e => e)) source.AppendLine("global::Argx.Binding.ArgumentCollectionFactories.Register<" + element + ">();");
            source.AppendLine("}\npublic override void Register() { RegisterCollections();");
            for (int i = 0; i < models.Count; i++) source.AppendLine("global::Argx.Binding.ArgumentBindingRegistry.Default.Register(B" + i + ");");
            source.AppendLine("}\n}");
            if (!symbol.ContainingNamespace.IsGlobalNamespace) source.AppendLine("}");
            result.Source = source.ToString();
        }
        catch (InvalidOperationException e) { result.Error = e.Message; }
        return result;
    }
}

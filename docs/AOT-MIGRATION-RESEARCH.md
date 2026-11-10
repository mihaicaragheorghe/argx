# Migrating Argx to one AOT-compatible library

Research date: 2026-11-10. Repository baseline: `25b39b996248a09a907f7ac0a6f787d26a3f5bf8`.

This is a research report and implementation proposal, not an implemented migration. Repository links are primary evidence for current behavior; Microsoft and Roslyn links support platform facts. Proposed APIs below do not exist yet. Filesystem inspection for this report is confined to this project; external documentation was consulted online.

## Recommendation

Make `src/Argx.AOT/Argx.AOT.csproj` the sole runtime implementation. Move the binding attributes there and replace reflection-based object binding with an incremental source generator that emits typed binders in consumer compilations. Keep the existing parser, actions, store, and subcommand infrastructure. Generate code for the operations that currently require runtime discovery, rather than regenerate the whole parser.

Use explicit generated binding contexts as the primary API. Offer an explicitly initialized registry adapter if retaining the existing `Get<T>()` / `Bind<T>()` signatures is a requirement. Remove the reflection fallback once parity and native-publish gates pass. This approach follows from the actual split between the two projects described below.

“Entire library AOT compatible” should mean every supported public operation has a statically reachable implementation and passes analysis and native execution. It cannot promise discovery of arbitrary unknown model types at runtime: Native AOT has no runtime code generation and restricts dynamic loading and generic instantiation. [Microsoft Native AOT overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

## Current implementation and gaps

| Area | Repository evidence | Migration consequence |
| --- | --- | --- |
| Shared runtime | [Argx.csproj](../src/Argx/Argx.csproj) links all AOT sources and defines `ARGX_REFLECTION`. | Consolidation does not require porting a second parser. |
| Existing AOT configuration | [Argx.AOT.csproj](../src/Argx.AOT/Argx.AOT.csproj) targets `netstandard2.1;net10.0` and enables `IsAotCompatible` for `net10.0`. | Retain both targets initially; validate AOT on the modern asset. |
| Object binding | [ArgumentsExtensions.cs](../src/Argx/Extensions/ArgumentsExtensions.cs) discovers runtime properties and attributes, invokes closed generic methods through reflection, and creates nested models with `Activator`. | Replace the complete binding path, including construction and nested traversal. |
| Attributes | [ArgumentAttribute](../src/Argx/Attributes/ArgumentAttribute.cs), [IgnoreAttribute](../src/Argx/Attributes/IgnoreAttribute.cs) exist only in regular Argx. | Move their namespaces and public contracts into the surviving runtime. |
| Collections | [CollectionUtils](../src/Argx.AOT/Utils/CollectionUtils.cs) already uses typed factories for built-ins and a registry, with reflection behind `ARGX_REFLECTION`. | Generate or explicitly supply closed factories for additional element types; delete conditional fallback. |
| Token conversion | [TokenConverter](../src/Argx.AOT/Binding/TokenConverter.cs) dispatches to a fixed scalar converter table. | Collection registration alone does not enable enum/custom token parsing. |
| Tests and benchmarks | [Tests project](../tests/Argx.Tests.csproj), [benchmarks project](../benchmarks/Argx.Benchmarks.csproj) reference regular Argx. | Switch both references before claiming parity. |
| Native sample | [Smoke program](../samples/Argx.AOT.Smoke/Program.cs) covers built-in parsing and registered enum constant collections. | Add generated object binding and package-consumer coverage. |
| Release pipeline | [publish.yml](../.github/workflows/publish.yml) packs both projects; [solution](../Argx.slnx) excludes the smoke sample. | Update packaging and explicitly publish/run native samples in CI. |

`IsAotCompatible` enables trim, AOT, and single-file analysis; it does not transform reflection into generated code. The modern target is necessary for AOT analysis, while a multi-targeted library can retain an older compatibility asset. [Microsoft AOT compatibility analyzers](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#aot-compatibility-analyzers).

## Generator and API design

Introduce `src/Argx.Generators` as a compiler component implementing `IIncrementalGenerator`. Discover explicit context attributes using `ForAttributeWithMetadataName`, inspect model symbols, construct immutable equatable descriptions, and emit stable source per context. Keep compiler symbols out of long-lived cached output models. These choices follow Roslyn's incremental-generation guidance. [Roslyn incremental generator cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md).

Proposed consumer code:

```csharp
[ArgxBinding(typeof(RootOptions))]
internal partial class CliBindings : ArgumentBindingContext;

var model = parsed.Get(CliBindings.Default.RootOptions);
parsed.Bind(existingModel, CliBindings.Default.RootOptions);
```

The context exposes typed metadata, for example `ArgumentBinding<RootOptions>`, containing a bind delegate and an optional creation delegate. `Bind` should support an existing instance without requiring a public parameterless constructor. `Get` requires a valid creation strategy. Context generation avoids requiring every options model to be partial and can reference accessible models from another assembly.

Illustrative emitted binding logic:

```csharp
if (args.TryGetValue<int>("count", out var count))
    instance.Count = count;

var child = instance.Child;
if (child is null)
{
    child = new ChildOptions();
    instance.Child = child;
}
if (args.TryGetValue<string>("child-label", out var label))
    child.Label = label;
```

Every property access, constructor, and `TryGetValue<TProperty>` call is ordinary typed code. Preserve missing-value behavior by assigning only when retrieval succeeds, as the current binder does. Keep parsing/conversion separate: the current binder retrieves typed stored values; it does not parse tokens itself. [Current binder](../src/Argx/Extensions/ArgumentsExtensions.cs), [argument store](../src/Argx.AOT/Parsing/ArgumentStore.cs).

A generator adds source; it cannot rewrite the existing generic extension implementation. Consequently, generating model-specific extension overloads alone does not replace `Get<T>()` reliably, including calls inside generic helpers. [Roslyn source-generator design](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.md).

If preserving those call signatures matters, implement a runtime registry with typed `Register<T>(...)` entry points and generated `CliBindings.Register()` initialization. Existing generic extensions dispatch only to registered delegates; an unregistered model fails with a clear message naming the required context/registration. This is a proposed compatibility layer, with explicit startup registration rather than implicit assembly scanning. Keep the metadata-taking API available to avoid global-state dependence. Do not promise source compatibility without the new registration requirement or binary compatibility across the assembly change.

## Binding contract decisions

The following are recommendations based on edge cases in the [existing binder](../src/Argx/Extensions/ArgumentsExtensions.cs), [type classification](../src/Argx.AOT/Extensions/TypeExtensions.cs), and [binding tests](../tests/Extensions/ArgumentsExtensionsTests.cs):

| Case | Recommended generated behavior |
| --- | --- |
| Names and attributes | Preserve kebab-case keys and absolute `[Argument("name")]` leaf overrides. Define culture-invariant name generation explicitly; current helper uses culture-sensitive `ToLower()`. |
| Nested paths | Build a fresh path for each property. Current code mutates `prefix` inside the sibling loop, leaking a child's prefix into later siblings. Fix deliberately and add regression cases. |
| Missing leaf | Preserve constructor/property initializer or existing instance value. |
| Null nested object | Preserve current eager construction when an accessible parameterless constructor exists. Allow a typed factory for other cases. |
| Constructor unavailable | Distinguish `Bind` of an existing nested instance from construction. To preserve existing behavior, emit a guarded `MissingMethodException` only if construction is actually needed; optionally diagnose this risk during generation. |
| Public properties | Enumerate inherited instance properties, exclude indexers/statics, honor `[Ignore]`, and skip getter-only properties. |
| Private setters and init-only | Diagnose unsupported binding targets rather than silently bypass access rules. Support constructor/init binding only as a separately specified creation feature. |
| Runtime subtypes | Current binder uses `instance.GetType()`. Either register explicit derived binders and dispatch to them or document declared-type binding as a behavior change. Never silently discard derived properties. |
| Cycles | Diagnose cyclic nested model graphs initially, with a path in the diagnostic. Current recursive traversal has no cycle protection. |
| Scalars | Explicitly recognize supported scalars, nullable scalars, enums, and `DateTimeOffset`. Current `IsComplexType` does not exclude `DateTimeOffset`, despite its token converter existing. |
| Collections | Treat supported collections as leaves; do not recursively bind their elements. Preserve exact stored-type retrieval semantics. |

Other generator diagnostics should cover inaccessible model types, open generic roots, duplicate generated member names, unsupported collection shapes, and ambiguous/hiding properties. Escape C# identifiers and string literals and use fully qualified type names. Compile emitted source in tests; textual snapshots alone cannot prove valid binding code.

## Collections and custom conversion

The supported existing factory shapes are arrays, `List<T>`, `IEnumerable<T>`, `IList<T>`, and `ICollection<T>`. Generate closed `ArgumentCollectionFactories.Register<TElement>()` calls in an explicit context initialization method for model collections. Retain manual registration for parser-only use without model binding. These paths already exist in [ArgumentCollectionFactories](../src/Argx.AOT/Binding/ArgumentCollectionFactories.cs).

A model generator cannot discover every collection used in imperative parser code, especially inside generic wrappers. For complete coverage without call-site inference, consider a separate typed collection-registration overload or carrying factory metadata from `Add<T>`; retain explicit registration as the dependable fallback. The current `Add<T>` supplies `typeof(T)` to the runtime parser. [ArgumentParser](../src/Argx.AOT/Parsing/ArgumentParser.cs).

Enum constants are already supported by registered factories, but enum tokens and custom scalar tokens are absent from the [converter table](../src/Argx.AOT/Binding/TokenConverter.StringConverters.cs). Do not conflate a migration with new conversion features. If those are required, add a typed converter registration contract and generate closed delegates using agreed parsing rules. Specify culture, nullable behavior, enum case sensitivity, and numeric enum acceptance independently. Never rediscover arbitrary `Parse` methods at runtime.

Retain bounded type inspection that passes analysis; the target is AOT compatibility, not removing every `typeof` or `GetType`. Audit all remaining paths after removing `ARGX_REFLECTION`, including collection actions and type helpers. The [subcommand delegate factory](../src/Argx.AOT/Subcommands/SubcommandDelegateFactory.cs) already wraps known delegate types and does not need generation.

## Packaging and project migration

1. Add the generator and typed binding/context contracts while keeping both runtime projects available for comparison. Move the existing attributes and generated binding entry points into the AOT runtime.
2. Build the generator for a compiler-host-compatible target, with `netstandard2.0` as the conservative starting choice. Choose and test the minimum Roslyn/compiler version separately from the runtime targets. Package its assembly under `analyzers/dotnet/cs`; keep Roslyn dependencies out of runtime references. Roslyn documents analyzer packaging and compiler-host constraints. [Generator packaging guidance](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#package-a-generator-as-a-nuget-package).
3. Include the generator in the `Argx.AOT` package so consumers receive it automatically. For repository consumers, add an analyzer project reference (`OutputItemType="Analyzer"`, `ReferenceOutputAssembly="false"`) where generation is required. Verify this behavior using the packed NuGet package, not only project references.
4. Switch tests, benchmarks, samples, solution membership, README installation guidance, and CI to the surviving runtime. Add generator and smoke projects to the solution as appropriate. Generate binding contexts for current test models.
5. Remove `ARGX_REFLECTION`, its fallback implementation, and the regular project's unique source after all parity gates pass. Remove `Argx.csproj` from the solution and release workflow.
6. Choose package identity deliberately. Recommended: keep `Argx.AOT` as requested and document migration from `Argx`; optionally publish a dependency-only transition package later. Removing a project does not migrate installed packages or preserve assembly identity. The existing projects produce different assemblies and package IDs. [Project definitions](../src/Argx/Argx.csproj), [AOT definition](../src/Argx.AOT/Argx.AOT.csproj).

Keep `IsAotCompatible` on the modern runtime target. Do not add `PublishAot` to the managed package project merely to make it usable from native apps: publishing a native shared library is a distinct interop scenario. Native publish belongs in executable validation consumers. [Microsoft native-library publishing](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/libraries).

## Validation and completion gates

Observed before implementation: all **492 existing tests passed**, and the existing smoke sample published for `osx-arm64` and printed **`AOT smoke passed`** on execution. These checks predate the latest filesystem restriction; no checks requiring access outside the project were rerun afterward. This establishes the existing baseline only. The tests exercise regular Argx; the smoke sample does not exercise generated binding. [Tests project](../tests/Argx.Tests.csproj), [smoke project](../samples/Argx.AOT.Smoke/Argx.AOT.Smoke.csproj), [smoke code](../samples/Argx.AOT.Smoke/Program.cs).

Required implementation gates:

- Run the complete existing suite against the surviving AOT runtime and generated binders. Preserve intentional semantics; explicitly update tests for documented behavior changes.
- Add focused binding cases for sibling prefixes, null nested construction, factory-only models, inherited/derived types, nullable values, `DateTimeOffset`, inaccessible setters, init-only properties, indexers, cycles, and generic helper dispatch.
- Test generator diagnostics, deterministic output, escaped names, referenced-assembly models, and compilation of emitted source on each supported compiler baseline.
- Publish and execute an expanded native smoke consumer covering generated `Get` and `Bind`, attributes, nested models, collections, constants, help/errors, custom actions, and sync/async subcommands. Run on each advertised OS/architecture using appropriate CI hosts.
- Add a full-library trimming analysis app with `TrimmerRootAssembly Include="Argx.AOT"`. Microsoft recommends a consuming test application and rooting the library to analyze paths that an ordinary sample may leave unused. [Preparing libraries for trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming#show-all-warnings-with-test-app).
- Add a separate realistic package consumer without full-library rooting. Restore from the locally packed package, compile generated binding, publish natively, and execute. This validates analyzer delivery and natural trimming reachability.
- Fail CI on unexplained trim/AOT warnings. Do not mark completion by adding blanket suppressions or moving unsafe operations behind warning attributes.
- Confirm the solution/release workflow contains no build or pack dependency on `Argx.csproj`, both runtime targets build, and documented package migration examples compile.

The result is one runtime implementation with generated model binding and explicit escape hatches for custom factories/converters. Arbitrary runtime model discovery is outside that contract; supported declared and registered models must work equally in JIT and Native AOT consumers.

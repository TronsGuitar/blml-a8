# Phase6 Advanced Features status

## Completed

### Added Phase6 project surface area

The repository now contains a dedicated Phase6 folder:

- `src/Phase6-AdvancedFeatures/README.md`
- `src/Phase6-AdvancedFeatures/PropertyProcedureGenerator.cs`

### Analyzed current prerequisites

The current codebase already contains useful Phase6 building blocks in earlier phases:

- `VB6Parser.ParseProperty()` for property-procedure parsing
- accessibility-modified `Public`/`Private`/`Friend`/`Static` declaration parsing for `Function`, `Sub`, and `Property`
- optional-parameter parsing in `VB6Parser.ParseVariableDeclaration(bool v)`
- `ParameterNode.IsOptional` and `ParameterNode.DefaultValue` in `AstNodes.cs`
- preserved `DefaultValueExpression` support for optional parameters
- dedicated `PropertyDeclarationNode` handling in `AstBuilder`
- grouped C# property generation in `PropertyProcedureGenerator`
- ActiveX/OCX wrapper scaffolding in `ActiveXFormCodeGenerator.GenerateAxWrapper()`
- baseline WinForms control mapping in `FrmParser` and `Vb6FormCodeGenerator`

### Validation added

Added repository-level tests for:

- Phase6 documentation presence
- README content describing current implementation state and remaining gaps
- DONE/status content describing remaining work
- source-analysis checks that confirm current Phase6 prerequisites still exist in `Phase1` and `Phase3`
- executable transpiler coverage for VB6 property procedures and optional/default parameters

## Current state

`Phase6` is now partially implemented.

The first implemented slice covers:

- VB6 `Property Get` + `Property Let/Set` conversion into C# properties
- optional/default parameter emission in generated C# method signatures

The broader advanced-feature conversion pipeline is still incomplete.

## Remaining follow-up

1. Broaden property conversion beyond the current direct-assignment getter/setter patterns.
2. ~~Carry `ParamArray` and named arguments through parsing, AST, and code generation.~~ Done -
   `VB6Parser`/`AstBuilder`/`VB6CodeGenerator` now handle both end-to-end.
3. Add late-binding and VB `Collection` migration strategies. (`CollectionConverter`/
   `DynamicConverter` in `src/Phase6-Advanced/` are still empty stubs - `CreateObject` itself is
   handled, see item 4, but general late-bound member access and VB `Collection` conversion are
   not.)
4. ~~Implement COM and Win32 interop conversion, including `CreateObject` and API `Declare`
   handling.~~ Done - `CreateObject` converts to late-bound `Activator.CreateInstance(Type
   .GetTypeFromProgID(...))` via `BuiltInFunctionHandler`, `Declare` converts to `[DllImport]`
   extern methods, and `src/Phase6-Advanced/COM/TypeLibConverter.cs` wraps the existing
   tlbimp-based `LibraryInspector` for interop-assembly generation. See
   `src/Phase6-Advanced/README.md` for the full breakdown, including why the old
   `typelibConverter.cs` in this folder had to be replaced rather than merely wired up (it used
   `System.Runtime.InteropServices.TypeLibConverter`, a .NET-Framework-only API unavailable on
   .NET 8).
5. ~~Add VB6 `Enum` parsing and C# enum generation.~~ Done.
6. ~~Expand advanced control coverage for `SSTab`, `MSFlexGrid`, `TreeView`, `ListView`,
   `CommonDialog`, `RichTextBox`, and third-party controls.~~ Done for the six named controls,
   via `FrmParser.Vb6ToCSharpControls` (the load-bearing mapping table for the CLI's actual
   `convert`/`form-export` pipeline - not `src/Phase3-FormsUI/ControlMapping/`, which turned out
   to be a separate, not-fully-wired-in pipeline). Third-party/other controls still fall through
   to a pass-through default rather than a mapping. See `src/Phase6-Advanced/README.md`.
7. Add more fixture-based tests with representative VB6 samples for each advanced feature. Done
   for `Enum`/`Declare`/`ParamArray`/named-arguments/`With`/`CreateObject` - see
   `tests/BLML.Tests/Phase6LanguageFeaturesTests.cs`. Still needed for `CollectionConverter`/
   `DynamicConverter` and advanced control mapping once those land.

## Folder consolidation

`src/Phase6-AdvancedFeatures/` (which only ever held `PropertyProcedureGenerator.cs`) has been
removed. Its one file moved to `src/Phase6-Advanced/PropertyProcedureGenerator.cs`, matching the
`Collections`/`COM`/`LateBinding` subfolders already there and the `BLML.Phase6Advanced`
namespace they use. `src/BLML.Transpiler.csproj` no longer excludes `Phase6-Advanced` from
compilation - previously the entire folder (including a real 275-line COM interop analyzer) was
dead code.

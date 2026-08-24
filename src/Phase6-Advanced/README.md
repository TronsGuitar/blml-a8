# Phase6 Advanced Features

## Status

- **Current status:** COM interop, language-feature parsing, property procedures, and advanced control mapping are implemented and covered by executable tests. Only Collections/late-binding wiring into the main codegen pipeline remains open.
- **Validated state:** property procedure conversion, optional/default parameter emission, `Enum`, `Declare`/`DllImport`, `ParamArray`, named arguments, `With`, and `CreateObject` conversion are all covered by executable tests.
- **Folder consolidation:** this folder used to have a sibling `src/Phase6-AdvancedFeatures/` holding only `PropertyProcedureGenerator.cs`, while `src/BLML.Transpiler.csproj` excluded this folder (`Phase6-Advanced`) from compilation entirely - meaning everything under here, including a real 275-line COM interop analyzer, was dead code. Both problems are now fixed: `PropertyProcedureGenerator.cs` moved here (namespace `BLML.Phase6Advanced`, matching `Collections`/`COM`/`LateBinding`), the old folder is gone, and the exclusion was removed from the csproj.

## Current Phase6 surface area

- `src/Phase6-Advanced/PropertyProcedureGenerator.cs` - groups `Property Get`/`Property Let`/`Property Set` into a generated C# property (moved from the old `Phase6-AdvancedFeatures` folder)
- `src/Phase6-Advanced/COM/determineInterop.cs` (`ReferencedLibrary`, `LibraryInspector`) - tlbimp-based type-library inspection, now properly namespaced (`BLML.Phase6Advanced.COM`) instead of sitting in the global namespace
- `src/Phase6-Advanced/COM/TypeLibConverter.cs` - wraps `LibraryInspector` for interop-assembly generation and converts VB6 `CreateObject("ProgId")` to late-bound `Activator.CreateInstance(Type.GetTypeFromProgID(...))`. Replaces a file that used to live here (`typelibConverter.cs`) which was a throwaway `class Program { static void Main }` sample built on `System.Runtime.InteropServices.TypeLibConverter`/`ITypeLibImporterNotifySink` - APIs that only ever existed in .NET Framework and don't exist on .NET 8 at all, so it could never have compiled once this folder's exclusion was lifted.
- `src/Phase6-Advanced/Collections/CollectionConverter.cs` - VB6 `Collection` -> `List<object>` (unkeyed) or `Dictionary<string, object>` (keyed) - both conversion targets are exposed since VB6's Collection supports both a positional index and an optional string key per element and there's no single C# type that's a faithful match for both at once; see the class doc comment for how a caller is expected to pick
- `src/Phase6-Advanced/LateBinding/DynamicConverter.cs` - VB6 `Variant`/`Object`/untyped -> C# `dynamic`

The core language-feature groundwork lives in Phase 1, extended (not stubbed) as part of this pass:

- `src/Phase1-Foundation/Lexer/VB6Lexer.cs` - `:=` (named-argument syntax) added to the operator table
- `src/Phase1-Foundation/AST/VB6SyntaxNode.cs` - `Enum`, `EnumMember`, `Declare` node types added
- `src/Phase1-Foundation/AST/AstNodes.cs` - `EnumDeclarationNode`, `EnumMemberNode`, `DeclareStatementNode`, `WithStatementNode`, `WithMemberAccessExpressionNode`, `NamedArgumentExpressionNode`, and `ParameterNode.IsParamArray` added
- `src/Phase1-Foundation/Parser/VB6Parser.cs` - parses `Enum ... End Enum`, `Declare Function/Sub ... Lib "x" [Alias "y"]`, `ParamArray`, `name:=value` call arguments, and `With ... End With` (including bare `.Member` references inside the block)
- `src/Phase1-Foundation/AST/AstBuilder.cs` - builds the semantic AST for all of the above
- `src/Phase1-Foundation/Parser/VB6CodeGenerator.cs` - emits `enum`, `[DllImport]` extern methods, `params` arrays, C# named arguments, and inlines `With` block bodies (capturing non-identifier targets into a compiler-generated local exactly once, so a `With SomeCall()` target isn't re-evaluated per `.Member` reference)
- `src/Phase1-Foundation/Parser/BuiltInFunctionHandler.cs` - `CreateObject` now converts through the real pipeline (every VB6 file run through `VB6Parser.TranspileFile` converts it), not just through the standalone `TypeLibConverter` class

Advanced control mapping (plan items 143-148) is implemented, but not in `src/Phase3-FormsUI/ControlMapping/` as originally scoped - the actual load-bearing mapping table turned out to be `FrmParser.Vb6ToCSharpControls` in `src/Phase3-FormsUI/FormParsing/frmParser.cs`, which is what the real `FormFileConverter`/`Vb6FormCodeGenerator` conversion pipeline (the one wired into the CLI's `convert`/`form-export` commands) actually reads from. `ControlMapping/` holds a separate, not-fully-wired-in ActiveX/ScaleLayout pipeline (`ActiveXFormCodeGenerator`, `WinFormsTableLayoutConverter`) that ordinary control-type mapping doesn't go through:

- `SSTab` (`TabDlg.SSTab`) -> `TabControl`
- `TreeView`/`ListView` (`MSComctlLib.*`) -> `TreeView`/`ListView` directly
- `MSFlexGrid` (`MSFlexGridLib.MSFlexGrid`) -> `DataGridView`
- `RichTextBox` (`RichTextLib.RichTextBox`) -> `RichTextBox` directly
- `CommonDialog` (`MSComDlg.CommonDialog`) -> `OpenFileDialog` (lossy - see the code comment on this mapping entry: VB6's CommonDialog is one control with `ShowOpen`/`ShowSave`/`ShowColor`/`ShowFont`/`ShowPrint` methods, and .NET has no single equivalent type since each is a separate dialog class; call sites using the other `Show*` methods need a manual follow-up)

## Not implemented yet

- Neither `CollectionConverter` nor `DynamicConverter` is wired into `VB6CodeGenerator` yet - they're real, tested conversion primitives, but nothing in the main pipeline calls into them the way `BuiltInFunctionHandler` now calls into `CreateObject` conversion. Doing so needs `VB6Parser`/`AstBuilder` to first represent `New Collection` / `.Add`/`.Remove`/`.Item` calls and `Variant`/`Object`-typed declarations distinctly enough to detect when these conversions apply.
- API `Declare` support does not attempt to map common Win32 signature idioms (e.g. `As Any`, string marshaling nuances) beyond a direct type mapping through the existing VB6-type-to-C#-type table
- Type-library import (`TypeLibConverter.GenerateInteropAssembly`) still depends on `tlbimp.exe` being available on the machine running the conversion - there is no managed .NET 8 API replacement for that specific step

## TODO

1. wire `CollectionConverter`/`DynamicConverter` into `VB6CodeGenerator`'s expression/statement generation, the way `CreateObject` is wired through `BuiltInFunctionHandler`
2. broaden `Declare` parameter marshaling beyond the direct VB6-type-to-C#-type mapping (string/array marshaling attributes, `As Any`)
3. decide whether `src/Phase3-FormsUI/ControlMapping/`'s separate ActiveX pipeline should be merged into the `frmParser`/`Vb6FormCodeGenerator` path that's actually wired into the CLI, since right now two different, non-interoperating form-conversion pipelines exist side by side

# Phase6 Advanced Features

## Status

- **Current status:** first implementation slice is active
- **Validated state:** property procedure conversion and optional/default parameter emission are covered by executable tests
- **Known gap:** the broader advanced-feature pipeline remains incomplete beyond the property/parameter slice

## Current Phase6 surface area

The current `Phase6-AdvancedFeatures` folder now contains one active implementation helper:

- `src/Phase6-AdvancedFeatures/PropertyProcedureGenerator.cs`

The current Phase6 groundwork lives in earlier phases:

- `src/Phase1-Foundation/Parser/VB6Parser.cs`
- `src/Phase1-Foundation/AST/AstBuilder.cs`
- `src/Phase1-Foundation/AST/AstNodes.cs`
- `src/Phase1-Foundation/Parser/VB6CodeGenerator.cs`
- `src/Phase1-Foundation/Lexer/VB6Keywords.cs`
- `src/Phase3-FormsUI/FormParsing/frmParser.cs`
- `src/Phase3-FormsUI/FormParsing/Vb6FormCodeGenerator.cs`
- `src/Phase3-FormsUI/ControlMapping/ActiveXFormCodeGenerator.cs`

## Implemented in this pass

The Phase6 work added in this pass is:

- a dedicated `src/Phase6-AdvancedFeatures` folder
- a Phase6 status README describing current prerequisites and gaps
- repository tests that lock in the current analysis of existing Phase6 building blocks
- a completed Phase6 status document under `docs/Phase6-AdvancedFeatures-TODO.md`

## Existing prerequisites already present in the repository

### `src/Phase1-Foundation/Parser/VB6Parser.cs`

- parses `Property` declarations through `ParseProperty()`
- now handles accessibility-modified `Public`/`Private`/`Friend`/`Static` declarations before `Function`, `Sub`, and `Property`
- records parameter `Optional` state during parsing
- now preserves parsed optional-parameter default values for later code generation
- records `ByVal` and `ByRef` parameter intent
- supports `Set` and `Let` assignment statements at parse time

### `src/Phase1-Foundation/AST/AstNodes.cs` and `src/Phase1-Foundation/AST/AstBuilder.cs`

- `ParameterNode` already exposes `IsOptional` and `DefaultValue`
- `ParameterNode` now also preserves `DefaultValueExpression`
- `PropertyDeclarationNode` and `PropertyProcedureKind` now represent property procedures directly in the AST
- `AstBuilder` now builds dedicated property declarations instead of routing `NodeType.Property` through plain method-building logic

### `src/Phase6-AdvancedFeatures/PropertyProcedureGenerator.cs`

- groups `Property Get` and `Property Let/Set` procedures into a generated C# property
- rewrites setter parameter references to the implicit C# `value` symbol
- falls back to property accessors only when the VB6 body shape is compatible with the current converter

### `src/Phase1-Foundation/Parser/VB6CodeGenerator.cs`

- now emits real C# properties from grouped VB6 property procedures when possible
- now emits optional/default parameter values in generated method signatures

### `src/Phase1-Foundation/Lexer/VB6Keywords.cs`

- already contains several VB-style enum-backed keyword groups that can inform later enum and constant work

### `src/Phase3-FormsUI/ControlMapping/ActiveXFormCodeGenerator.cs`

- already supports wrapper-backed ActiveX/OCX control generation through `GenerateAxWrapper()`
- already resolves registered OCX paths through the Windows registry

### `src/Phase3-FormsUI/FormParsing/frmParser.cs` and `src/Phase3-FormsUI/FormParsing/Vb6FormCodeGenerator.cs`

- already cover a basic set of WinForms control mappings and code generation primitives that Phase6 advanced-control work can extend

## Not implemented yet

### `src/Phase6-AdvancedFeatures`

- only the first Phase6 slice is implemented so far: property procedures and optional/default parameters
- `ParamArray`, named-argument normalization, late binding, and VB `Collection` conversion are not implemented
- VB6 `Enum` declarations are not parsed and emitted as user-defined C# enums
- `CreateObject`, API `Declare`, `DllImport`, and general COM interop conversion are not implemented
- advanced control mappings such as `SSTab`, `MSFlexGrid`, `CommonDialog`, `RichTextBox`, and broader third-party controls are not implemented in the active pipeline
- property getter conversion currently expects a direct assignment to the property name inside the `Property Get` body
- property setter conversion currently assumes the final VB6 property parameter maps to the implicit C# `value` parameter

## TODO

1. extend the Phase6 parser/codegen slice beyond property procedures and optional parameters into named arguments and `ParamArray`
2. improve property conversion to handle more complex getter/setter bodies and backing-field inference
3. add late-binding and `Collection` conversion strategies, with a documented fallback for unsupported cases
4. implement COM interop handling for `CreateObject`, API declares, and `DllImport` generation
5. add VB6 `Enum` parsing and C# enum generation
6. extend Phase3 control mapping for `SSTab`, `MSFlexGrid`, `TreeView`, `ListView`, `CommonDialog`, and `RichTextBox`
7. add more executable tests with representative VB6 samples for each advanced feature area

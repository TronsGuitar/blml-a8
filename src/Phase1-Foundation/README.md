# Phase1 Foundation

## Status

- **Current status:** core foundation layers are active
- **Validated state:** lexer, parser, AST, code generation, project parsing, dependency analysis, symbol-table building, and type-inference helpers all exist in the active codebase
- **Known gap:** the Phase1 building blocks are present, but the full planned project-generation and end-to-end integration story is still incomplete

## Current Phase1 surface area

The current `Phase1-Foundation` folder contains the core infrastructure for the transpiler:

- `src/Phase1-Foundation/Lexer/VB6Lexer.cs`
- `src/Phase1-Foundation/Lexer/VB6Keywords.cs`
- `src/Phase1-Foundation/Parser/VB6Parser.cs`
- `src/Phase1-Foundation/Parser/VB6CodeGenerator.cs`
- `src/Phase1-Foundation/Parser/BuiltInFunctionHandler.cs`
- `src/Phase1-Foundation/AST/AstNodes.cs`
- `src/Phase1-Foundation/AST/AstBuilder.cs`
- `src/Phase1-Foundation/AST/VB6SyntaxNode.cs`
- `src/Phase1-Foundation/SymbolTable/SymbolTableBuilder.cs`
- `src/Phase1-Foundation/TypeInference/TypeInferenceEngine.cs`
- `src/Phase1-Foundation/ProjectModel/ProjectFileParser.cs`
- `src/Phase1-Foundation/ProjectModel/VB6Project.cs`
- `src/Phase1-Foundation/DependencyGraph/DependencyAnalyzer.cs`

## Implemented in the current repository

### `src/Phase1-Foundation/Lexer/VB6Lexer.cs`

- tokenizes core VB6 source input for the active parser pipeline
- works with the current keyword model in `VB6Keywords.cs`

### `src/Phase1-Foundation/Parser/VB6Parser.cs`

- parses modules, procedures, statements, and several VB6 language constructs used by later phases
- supports property procedures and optional/default parameter metadata used by Phase6
- feeds the current transpiler and CLI conversion paths

### `src/Phase1-Foundation/AST/*`

- provides the active syntax-node and AST model used by the transpiler
- captures method, property, parameter, and statement information for code generation

### `src/Phase1-Foundation/Parser/VB6CodeGenerator.cs`

- emits Roslyn-based C# output from the active AST pipeline
- integrates built-in VB6 function handling and the current property-procedure support

### `src/Phase1-Foundation/ProjectModel/ProjectFileParser.cs`

- parses `.vbp` files into a structured project model
- exposes forms, modules, classes, and references for later tooling and reporting

### `src/Phase1-Foundation/ProjectModel/CsprojGenerator.cs`

- generates `.csproj` files targeting **net8.0-windows** (when forms/user controls are present) or **net8.0** (library-only)
- generated projects use **C# 12** (`LangVersion 12`), `UseWindowsForms`, and nullable reference types

### `src/Phase1-Foundation/SymbolTable`, `TypeInference`, and `DependencyGraph`

- contain active helper layers for semantic analysis and project understanding
- provide groundwork for later whole-project conversion and optimization work

## What is left to do

1. complete more edge-case VB6 lexer and parser coverage so the active pipeline can handle a broader set of real-world modules
2. deepen symbol-table and dependency analysis from helper-level support into consistent whole-project resolution
3. connect type inference more directly to emitted C# for `Variant`, implicit declarations, and ambiguous member usage
4. implement the remaining planned Phase1 generation pieces from `ProjectPlan.md`, especially namespace generation, using/import generation, `.csproj` generation, and solution generation
5. add broader fixture-based tests for parser, AST, and code-generation behavior across larger VB6 samples

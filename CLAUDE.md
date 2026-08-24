# CLAUDE.md - Project Intelligence

## Project Overview

**BLML (Business Logic Migration Library)** - A comprehensive transpiler/converter toolkit for:

- Converting Visual Basic 6 (VB6) desktop applications to modern C# with Windows Forms/WPF
- Migrating Classic ASP applications to Angular 20 + .NET Core 8 + SQL Server
- Converting Microsoft Access databases to modern web-based solutions

**Current Status:** Phase 1 (Foundation) - **Complete**, Phases 2-8 - **Implementation Started/Stubbed**

## Quick Reference

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run specific test categories
dotnet test tests/Unit/
dotnet test tests/Integration/
```

## Architecture

### Multi-Phase Pipeline

The project follows a staged compiler/transpiler architecture:

```
VB6 Source → Lexer → Parser → AST → Symbol Table → Type Inference → C# Code Generation
```

### Directory Structure

```
src/
├── Phase1-Foundation/     # [COMPLETE] Core parsing infrastructure (Lexer, Parser, AST, SymbolTable)
├── Phase2-CoreLanguage/   # [IMPLEMENTED] Language feature conversion (CodeGeneration, Converters)
├── Phase3-FormsUI/        # [IMPLEMENTED] WinForms conversion (Layout, FormParsing)
├── Phase4-DataAccess/     # [IMPLEMENTED] Database migration (ADO, SQL, EF)
├── Phase5-ASPtoAngular/   # [IMPLEMENTED] ASP/VBScript parsing, analysis, .NET 8 API + standalone Angular 17+ generation, DB (see README.md)
├── Phase6-Advanced/       # [STUBBED] COM, Late Binding, Optimize
├── Phase7-Optimization/   # [STUBBED] Code Cleanup, Refactoring
└── Phase8-Tooling/        # [STUBBED] CLI, IDE integration

tests/
├── BLML.Tests/           # Main test suite
├── Unit/                 # Unit tests
└── Integration/          # Integration tests
```

## Key Files

| File | Purpose |
|------|---------|
| `src/Phase1-Foundation/Lexer/VB6Lexer.cs` | Core VB6 tokenization |
| `src/Phase1-Foundation/Parser/VB6Parser.cs` | Main parsing logic |
| `src/Phase1-Foundation/AST/AstNodes.cs` | AST node definitions |
| `src/Phase2-CoreLanguage/CodeGeneration/VB6CodeGenerator.cs` | Main C# Generator (Roslyn) |
| `src/Phase2-CoreLanguage/Converters/ControlFlowConverter.cs` | Control Flow logic (If, Select Case) |
| `src/Phase3-FormsUI/Layout/LayoutConverter.cs` | Form parser and Designer generator |
| `src/Phase4-DataAccess/ADO/AdoConverter.cs` | ADO to SqlClient mapper |
| `ProjectPlan.md` | 170-item task breakdown by phase |

## Technology Stack

- **.NET 8** - Target framework
- **Microsoft.CodeAnalysis.CSharp (Roslyn)** - C# code generation
- **xUnit** - Testing framework
- **PowerShell 7+** - Automation scripts

## Namespace Convention

```
BLML.Phase1Foundation
BLML.Phase2CoreLanguage
BLML.Phase3FormsUI
BLML.Phase4DataAccess
BLML.Phase5ASPtoAngular
...
```

## Development Notes

### Setup

All core phases have been structurally implemented.

- **Phase 1**: Logic is mostly complete.
- **Phase 2**: Core converters (Variable, ControlFlow) and Generator are implemented.
- **Phase 3**: LayoutConverter implements `.frm` parsing and WinForms Designer generation.
- **Phase 4**: Basic ADO and Schema generators stubbed/implemented.
- **Phase 5**: Real ASP/VBScript parser, analysis passes, .NET 8 Web API backend generation,
  standalone Angular 17+ frontend generation with an anti-pattern checker, and DB generation
  (delegates to Phase 4). Wired end-to-end via `AspProjectConverter` / CLI `convert-asp-project`.
- **Phase 6-8**: Stubs created for all core components.

### Build Configuration

```xml
Target Framework: net8.0 (net8.0-windows for WinForms output)
C# Language Version: 12
Implicit Usings: Enabled
Nullable Reference Types: Enabled
Root Namespace: BLML
```

### Code Generation Output Target

Generated C# projects target **net8.0-windows** with **LangVersion 12** when the source VB6 project contains forms or user controls, otherwise **net8.0**.

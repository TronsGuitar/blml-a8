# CLAUDE.md - Project Intelligence

## Project Overview

**BLML (Business Logic Migration Library)** - A comprehensive transpiler/converter toolkit for:
- Converting Visual Basic 6 (VB6) desktop applications to modern C# with Windows Forms/WPF
- Migrating Classic ASP applications to Angular 20 + .NET Core 8 + SQL Server
- Converting Microsoft Access databases to modern web-based solutions

**Current Status:** Phase 1 (Foundation) - approximately 40% complete

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
├── Phase1-Foundation/     # [ACTIVE] Core parsing infrastructure
│   ├── Lexer/            # VB6 tokenization (VB6Lexer.cs, VB6Keywords.cs)
│   ├── Parser/           # Grammar parsing (VB6Parser.cs)
│   ├── AST/              # Abstract syntax tree (AstBuilder.cs, AstNodes.cs)
│   ├── SymbolTable/      # Variable/function tracking
│   ├── TypeInference/    # Type checking engine
│   ├── ProjectModel/     # VB6 project (.vbp) parsing
│   └── DependencyGraph/  # Inter-project dependency analysis
├── Phase2-CoreLanguage/   # [STUB] Language feature conversion
├── Phase3-FormsUI/        # [STUB] WinForms conversion
├── Phase4-DataAccess/     # [STUB] Database migration
└── Phase5-ASPtoAngular/   # [STUB] Web stack migration

tests/
├── BLML.Tests/           # Main test suite
├── Unit/                 # Unit tests
└── Integration/          # Integration tests

docs/
├── Reference/            # VB6 language documentation
├── Migration/            # Migration guides
└── Lists/                # Keyword lists, control property tables

tools/
├── PowerShell/           # Repository automation scripts
├── Python/               # Form conversion, test utilities
└── SQL/                  # Database migration scripts

samples/VB6/              # Sample VB6 code for testing
```

## Key Files

| File | Purpose |
|------|---------|
| `src/Phase1-Foundation/Lexer/VB6Lexer.cs` | Core VB6 tokenization |
| `src/Phase1-Foundation/Lexer/VB6Keywords.cs` | VB6 keyword definitions (45+ keywords) |
| `src/Phase1-Foundation/Parser/VB6Parser.cs` | Main parsing logic |
| `src/Phase1-Foundation/AST/AstNodes.cs` | AST node definitions |
| `src/Phase1-Foundation/SymbolTable/SymbolTableBuilder.cs` | VB6 constants and symbols (~80+ constants) |
| `ProjectPlan.md` | 170-item task breakdown by phase |
| `REORGANIZATION.md` | File organization guide |

## Technology Stack

- **.NET 8** - Target framework
- **Microsoft.CodeAnalysis.CSharp (Roslyn)** - C# code generation
- **xUnit** - Testing framework
- **PowerShell 7+** - Automation scripts

## Namespace Convention

```
BLML.Phase1Foundation.Lexer
BLML.Phase1Foundation.Parser
BLML.Phase1Foundation.AST
BLML.Phase2CoreLanguage.Converters
```

## Development Notes

### Current Phase 1 Implementation

Only Phase 1 code is compiled into the build. Phases 2-8 have stub structures but are excluded from compilation.

### Token/AST Flow

```csharp
VB6Token (Value, Type, Line, Column)
  → TokenList passed to Parser
    → Raw AST (syntax tree)
      → Semantic AST (AstBuilder)
        → Symbol Table + Type Inference
          → C# Code Generation
```

### Build Configuration

```xml
Target Framework: net8.0
Implicit Usings: Enabled
Nullable Reference Types: Enabled
Root Namespace: BLML
```

## Important Conventions

1. **Phase-based organization** - Each phase can be worked on somewhat independently
2. **Modular design** - Clear separation: Lexer → Parser → AST → Codegen
3. **Extensive documentation** - Reference docs in `docs/Reference/`
4. **Two conversion tracks** - Desktop (VB6→C#) and Web (ASP→Angular)

## Testing

Tests use xUnit. Run with `dotnet test`. Test files are in `/tests/BLML.Tests/`.

## Related Documentation

- [ProjectPlan.md](ProjectPlan.md) - Complete prioritized task list
- [REORGANIZATION.md](REORGANIZATION.md) - File organization details
- [docs/Reference/Keywords.md](docs/Reference/Keywords.md) - VB6 keywords reference
- [docs/Reference/KeywordsControlsFunctions.md](docs/Reference/KeywordsControlsFunctions.md) - VB6 controls and functions

# VB6 to C# Converter Project

A comprehensive toolkit for converting Visual Basic 6 applications to modern C# and migrating Classic ASP to Angular/.NET Core.

---

## 📋 Project Status

This is an **in-progress** conversion framework. See [ProjectPlan.md](ProjectPlan.md) for detailed roadmap and priorities.

**Current Phase:** Foundation (Phase 1) - Core parsing and infrastructure

---

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Git
- PowerShell 7+ (for automation scripts)

### Reorganize Repository (First Time Setup)

```powershell
# Run a dry run first to see what will happen
.\reorganize-repo.ps1 -DryRun

# Perform actual reorganization
.\reorganize-repo.ps1

# Or reorganize without using git mv (if you haven't committed yet)
.\reorganize-repo.ps1 -UseGitMv:$false
```

---

## 📁 Repository Structure

After reorganization, the repository follows this structure:

```
blml-a8/
├── docs/                    # All documentation
│   ├── Reference/          # VB6 language reference
│   ├── Migration/          # Migration guides
│   ├── Training/           # LLM training data
│   └── Lists/              # Keyword/control lists
├── src/
│   ├── Phase1-Foundation/  # Parser, lexer, AST
│   ├── Phase2-CoreLanguage/# Code generation, converters
│   ├── Phase3-FormsUI/     # Form parsing and conversion
│   ├── Phase4-DataAccess/  # Database migration
│   ├── Phase5-ASPtoAngular/# ASP to modern web stack
│   ├── Phase6-Advanced/    # COM, collections, etc.
│   ├── Phase7-Optimization/# Code cleanup and polish
│   └── Phase8-Tooling/     # IDE support, CLI
├── tests/                  # Unit and integration tests
├── tools/                  # Utility scripts
├── samples/                # Example VB6 code
└── AngularTutor/          # Separate Angular tutorial project
```

See [REORGANIZATION.md](REORGANIZATION.md) for complete details.

---

## 📚 Key Documents

| Document | Purpose |
|----------|---------|
| [ProjectPlan.md](ProjectPlan.md) | Complete task list organized by priority |
| [REORGANIZATION.md](REORGANIZATION.md) | File organization guide |
| [reorganize-repo.ps1](reorganize-repo.ps1) | Automation script for restructuring |

---

## 🎯 Project Goals

### 1. VB6 → C# Conversion
Convert Visual Basic 6 desktop applications to modern C# with Windows Forms or WPF.

**Key Features:**
- Full VB6 syntax parsing
- Form and control conversion
- Database access modernization
- COM interop handling

### 2. Classic ASP → Angular + .NET Core
Migrate classic ASP applications to modern web stack.

**Target Architecture:**
- **Frontend:** Angular 20 + Angular Material
- **Backend:** .NET Core 8 Web API
- **Database:** SQL Server with Entity Framework Core

### 3. Access → Web
Convert Microsoft Access applications to web-based solutions.

**Components:**
- Schema migration to SQL Server
- Forms to Razor Pages or Blazor
- Reports to web reports
- Queries to stored procedures

---

## 🔧 Current Capabilities

### ✅ Implemented
- Basic VB6 lexical analysis
- Partial VB6 parser
- Form (.frm) file parsing
- Basic code generation (CodeDom)
- Access database extraction
- Some control mapping

### 🚧 In Progress
- Complete VB6 grammar parser
- Symbol table and type inference
- AST builder
- Roslyn-based code generation

### 📝 Planned
- ASP parser
- Angular component generator
- .NET Core API generator
- EF Core scaffolding
- Complete test suite

See [ProjectPlan.md](ProjectPlan.md) for detailed breakdown.

---

## 🏗️ Development Phases

| Phase | Status | Priority | Timeline |
|-------|--------|----------|----------|
| **Phase 1:** Foundation | 🟡 In Progress | CRITICAL | Weeks 1-4 |
| **Phase 2:** Core Language | 🔴 Not Started | HIGH | Weeks 5-8 |
| **Phase 3:** Forms & UI | 🔴 Not Started | HIGH | Weeks 9-12 |
| **Phase 4:** Data Access | 🔴 Not Started | HIGH | Weeks 9-12 |
| **Phase 5:** ASP to Angular | 🔴 Not Started | HIGH | Weeks 13-20 |
| **Phase 6:** Advanced Features | 🔴 Not Started | MEDIUM | Weeks 21-24 |
| **Phase 7:** Optimization | 🔴 Not Started | MEDIUM | Weeks 25-26 |
| **Phase 8:** Tooling | 🔴 Not Started | LOW | Ongoing |

---

## 💻 Usage Examples

### Convert a VB6 Project (Planned)

```bash
# Using CLI (not yet implemented)
vb6convert --input MyProject.vbp --output ./CSharp/MyProject

# Using library
var converter = new VB6Converter();
var result = converter.ConvertProject("MyProject.vbp");
```

### Convert Classic ASP to Angular (Planned)

```bash
# Analyze ASP application
asp2angular analyze --input ./AspApp --output ./analysis.json

# Generate Angular + API
asp2angular generate --input ./AspApp --output ./ModernApp
```

---

## 🧪 Testing

```bash
# Run unit tests
dotnet test tests/Unit/

# Run integration tests
dotnet test tests/Integration/

# Run all tests
dotnet test
```

### Current failing test status

The current `tests/BLML.Tests/TranspilerTests.cs` suite is failing.

- `11/11` tests currently fail.
- Most parser failures come from unimplemented methods in `src/Phase1-Foundation/Parser/VB6Parser.cs`.
- `TranspileFile()` catches those exceptions and records `Transpilation failed: The method or operation is not implemented.`, which leaves `CSharpCode` null and causes the parser assertions to fail.

#### Confirmed causes

- `ParseProperty()` is not implemented.
- `ParseVariableDeclaration(bool)` is not implemented.
- `ParseVariableDeclaration()` is not implemented.
- `ParseReDimStatement()` is not implemented.
- `ParseExpression()` is not implemented.

These gaps explain the failures for:

- basic sub parsing
- `If` statements
- `For` loops
- `While/Wend`
- `Do/Loop`
- `Select Case`
- built-in functions
- predefined constants

The lexer test also fails, but the failure output already shows the expected tokens in the returned collection. That one needs separate investigation.

### TODO

- Implement `ParseExpression()` in `VB6Parser`.
- Implement both `ParseVariableDeclaration` overloads in `VB6Parser`.
- Implement `ParseProperty()` in `VB6Parser`.
- Implement `ParseReDimStatement()` in `VB6Parser`.
- Re-run `BLML.Tests` after parser work is complete.
- Isolate `Lexer_ShouldTokenizeSimpleExpression` to determine whether the failure is in tokenization or the assertion path.

---

## 📖 Documentation

### Reference Documentation
- [VB6 Keywords](docs/Reference/Keywords.md)
- [VB6 Controls & Functions](docs/Reference/KeywordsControlsFunctions.md)
- [VB6 Properties Reference](docs/Lists/vb6controlprops.csv)

### Migration Guides
- [Access to Razor Pages](docs/Migration/access2razor.md)
- [EF Core and Razor](docs/Migration/efcoreandrazor.md)
- [React vs Razor](docs/Migration/makeReactRazor.md)
- [Razor without EF](docs/Migration/razorNoEF.md)

### Technical Documents
- [RFC-DCLR](docs/Reference/RFC-DCLR.md)
- [Realtime Requirements](docs/Reference/RealtimeRequirements.md)
- [LLM Training Dataset](docs/Reference/LLMRagDataset.md)

---

## 🤝 Contributing

This project is under active development. Contributions are welcome!

### How to Contribute

1. Check [ProjectPlan.md](ProjectPlan.md) for tasks
2. Pick a task from an active phase
3. Create a feature branch
4. Implement with tests
5. Submit a pull request

### Development Workflow

```bash
# Clone repository
git clone https://github.com/TronsGuitar/blml-a8.git
cd blml-a8

# Reorganize (first time only)
.\reorganize-repo.ps1

# Create feature branch
git checkout -b feature/my-feature

# Make changes
# ...

# Run tests
dotnet test

# Commit and push
git add .
git commit -m "Add feature X"
git push origin feature/my-feature
```

---

## 🛠️ Tools and Scripts

### PowerShell Scripts
- **reorganize-repo.ps1** - Reorganize repository structure
- **accdb2sql.ps1** - Convert Access to SQL Server
- **acesss2razor.ps1** - Generate Razor pages from Access
- **zippr.ps1** - Archive utilities

### Python Scripts
- **vb6frm2csharpfrmx.py** - Form converter
- **generatetests.py** - Test generator
- **searchText.py** - Code search utility

### SQL Scripts
- **SqlServerRunPSJob.sql** - SQL Server job integration

---

## 📊 Project Metrics

### Code Statistics (Estimated)
- **Total Files:** ~100
- **C# Files:** ~85
- **Documentation:** ~25 MD files
- **Scripts:** ~10 PS1/PY/SQL files

### Completion Status
- **Foundation:** ~40%
- **Core Features:** ~20%
- **Forms:** ~30%
- **Data Access:** ~25%
- **ASP Migration:** ~5%
- **Advanced:** ~10%

---

## 🗺️ Technology Stack

### VB6 → C# Conversion
- **.NET 8** - Target framework
- **Roslyn** - Code generation
- **CodeDom** - Legacy code generation (being replaced)
- **Windows Forms** - UI framework
- **ADO.NET / EF Core** - Data access

### ASP → Modern Web
- **Angular 20** - Frontend framework
- **Angular Material** - UI components
- **TypeScript** - Frontend language
- **.NET Core 8** - Backend API
- **Entity Framework Core** - ORM
- **SQL Server** - Database

---

## ✅ Final TODO List

1. complete the remaining `Phase1-Foundation` parser work, especially `ParseExpression()`, `ParseVariableDeclaration(...)`, `ParseProperty()`, and `ParseReDimStatement()`
2. get `tests/BLML.Tests` passing again and isolate the lexer assertion issue
3. finish `Phase2-CoreLanguage` code-generation and conversion coverage beyond the current partial pipeline
4. expand `Phase3-FormsUI` form conversion, layout reconstruction, property mapping, and resource conversion
5. implement the remaining `Phase4-DataAccess` migration pipeline for schema extraction, entity generation, data migration, and ADO modernization
6. decide and execute the real `Phase5-ASPtoAngular` target architecture, then replace the current prototypes with an actual migration pipeline
7. extend `Phase6-AdvancedFeatures` into `ParamArray`, named arguments, enums, collections, COM interop, and broader advanced control support
8. broaden `Phase7-Optimization` into whole-project analysis, migration reporting, code metrics, and safer rewrite support
9. turn the `Phase8-Tooling` folders into actual projects for CLI, IDE/LSP, VS Code, and web-hosted tooling
10. add broader fixture-based tests, sample projects, and end-to-end validation across all phases
11. finish repository polish items such as project packaging, CI coverage, and the top-level license declaration

---

## 📄 License

[Specify your license here]

---

## 📞 Contact

For questions or support, please open an issue on GitHub.

---

## 🙏 Acknowledgments

This project builds upon various open-source VB6 parsers and conversion tools. Special thanks to all contributors to the VB6 migration community.

---

**Note:** This is an active development project. Features and structure may change as development progresses. Always refer to [ProjectPlan.md](ProjectPlan.md) for the most current roadmap.

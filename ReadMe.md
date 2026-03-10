# VB6 to C# Converter Project

A comprehensive toolkit for converting Visual Basic 6 applications to modern C# and migrating Classic ASP to Angular/.NET Core.

---

## 📋 Project Status

This is an **in-progress** conversion framework. See [ProjectPlan.md](ProjectPlan.md) for detailed roadmap and priorities.

**Current validated status:**

- the solution builds successfully on `.NET 8`
- `tests/BLML.Tests` currently passes `48/49` tests
- `1` test is intentionally skipped for the isolated `vb6binary.cs` compatibility path
- active partial implementation currently spans `Phase1`, `Phase3`, `Phase4`, `Phase6`, and `Phase7`
- `Phase5` and most of `Phase8` remain prototype or planning surfaces rather than finished projects

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
- Roslyn-based C# code generation for the active transpiler path
- Access database extraction
- Some control mapping
- Phase6 property procedure conversion and optional/default parameter emission
- Phase7 XML documentation, dead-code cleanup, and LINQ suggestion helpers

### 🚧 In Progress
- Complete VB6 grammar and semantic coverage
- Symbol table and type inference
- AST builder
- richer form, data, and advanced-feature conversion coverage

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
| **Phase 1:** Foundation | 🟡 Active and partially implemented | CRITICAL | Weeks 1-4 |
| **Phase 2:** Core Language | 🔴 Incomplete / not fully documented in current pass | HIGH | Weeks 5-8 |
| **Phase 3:** Forms & UI | 🟡 Partially implemented and tested | HIGH | Weeks 9-12 |
| **Phase 4:** Data Access | 🟡 Partially implemented | HIGH | Weeks 9-12 |
| **Phase 5:** ASP to Angular | 🟠 Prototype-only assets | HIGH | Weeks 13-20 |
| **Phase 6:** Advanced Features | 🟡 First implementation slice active | MEDIUM | Weeks 21-24 |
| **Phase 7:** Optimization | 🟡 Multiple helper slices active | MEDIUM | Weeks 25-26 |
| **Phase 8:** Tooling | 🟠 Documentation, stubs, and prototypes | LOW | Ongoing |

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

### Current test status

The current `tests/BLML.Tests` project is in a healthy state.

- `48/49` tests pass
- `1` test is intentionally skipped
- the solution builds successfully
- the parser and transpiler tests called out earlier are now passing

#### Remaining known test gap

- `Phase3FormsUiTodoTests.Vb6BinaryCompatibilityLayer_ShouldSupportBinaryRecordAccessInDedicatedCompatibilityProject` is still skipped
- that skip is intentional because `src/Phase3-FormsUI/FormParsing/vb6binary.cs` remains isolated until a dedicated compatibility project restores its VB runtime dependencies

### Next test-focused follow-up

- create the dedicated compatibility project for `vb6binary.cs` if binary VB6 record access is still required
- add broader fixture-based coverage for end-to-end conversion scenarios across phases
- keep README status synchronized with the actual build and test state

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
- **Foundation:** active and partially implemented
- **Core Features:** still incomplete
- **Forms:** partially implemented with test coverage
- **Data Access:** partially implemented
- **ASP Migration:** prototype stage
- **Advanced:** first slice implemented
- **Optimization:** active helper slice implemented
- **Tooling:** planning and stub stage

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

1. finish `Phase2-CoreLanguage` code-generation and conversion coverage beyond the current partial pipeline
2. expand `Phase3-FormsUI` form conversion, layout reconstruction, property mapping, resource conversion, and compatibility-project handling for `vb6binary.cs`
3. implement the remaining `Phase4-DataAccess` migration pipeline for schema extraction, entity generation, data migration, and ADO modernization
4. decide and execute the real `Phase5-ASPtoAngular` target architecture, then replace the current prototypes with an actual migration pipeline
5. extend `Phase6-AdvancedFeatures` into `ParamArray`, named arguments, enums, collections, COM interop, and broader advanced control support
6. broaden `Phase7-Optimization` into whole-project analysis, migration reporting, code metrics, and safer rewrite support
7. turn the `Phase8-Tooling` folders into actual projects for CLI, IDE/LSP, VS Code, and web-hosted tooling
8. add broader fixture-based tests, sample projects, and end-to-end validation across all phases
9. finish repository polish items such as project packaging, CI coverage, and the top-level license declaration

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

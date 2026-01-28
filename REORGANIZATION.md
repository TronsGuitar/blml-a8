# Repository Reorganization Guide

This document maps existing files to the project structure based on ProjectPlan.md

---

## **Proposed Folder Structure**

```
blml-a8/
├── README.md
├── ProjectPlan.md
├── REORGANIZATION.md (this file)
├── .github/
│   └── workflows/
├── docs/
│   ├── Reference/
│   │   ├── Keywords.md
│   │   ├── KeywordsControlsFunctions.md
│   │   ├── LLMRagDataset.md
│   │   ├── Pseudocode.md
│   │   ├── RFC-DCLR.md
│   │   ├── RealtimeRequirements.md
│   │   ├── CreateVB6TrainingDS.md
│   │   ├── diagramFormats.md
│   │   ├── howllmswork.md
│   │   └── vbnetpropsfromvb6.md
│   ├── Migration/
│   │   ├── access2razor.md
│   │   ├── efcoreandrazor.md
│   │   ├── makeReactRazor.md
│   │   ├── razorNoEF.md
│   │   ├── mdbsolutions.md
│   │   ├── transpile-pipe.md
│   │   └── vbnetvb6binary.md
│   ├── Training/
│   │   ├── exampleCSharpLLMDataset.json
│   │   └── howtocreatedataset.htm
│   └── Lists/
│       ├── constants.txt
│       ├── vb6keywords.txt
│       ├── vb6controlproplist.txt
│       ├── vb6controlprops.csv
│       └── vb6controlproptypes.txt
├── src/
│   ├── Phase1-Foundation/
│   │   ├── Parser/
│   │   │   ├── VB6Parser.cs                    [MOVE FROM ROOT]
│   │   │   ├── IScannerParser.cs               [MOVE FROM ROOT]
│   │   │   ├── preprocess.cs                   [MOVE FROM ROOT]
│   │   │   └── trees.cs                        [MOVE FROM ROOT]
│   │   ├── Lexer/
│   │   │   ├── VB6Keywords.cs                  [MOVE FROM ROOT]
│   │   │   ├── parseVB6Constants.cs            [MOVE FROM ROOT]
│   │   │   ├── parsevb6Constants2.cs           [MOVE FROM ROOT]
│   │   │   └── parsebuiltinfunctions.cs        [MOVE FROM ROOT]
│   │   ├── SymbolTable/
│   │   │   ├── ImmutableSymbolTable            [MOVE FROM ROOT]
│   │   │   └── SymbolTableBuilder.cs           [TO CREATE]
│   │   ├── AST/
│   │   │   └── AstBuilder.cs                   [TO CREATE]
│   │   ├── TypeInference/
│   │   │   └── TypeInferenceEngine.cs          [TO CREATE]
│   │   ├── ProjectModel/
│   │   │   └── ProjectFileParser.cs            [TO CREATE]
│   │   └── DependencyGraph/
│   │       └── DependencyAnalyzer.cs           [TO CREATE]
│   ├── Phase2-CoreLanguage/
│   │   ├── CodeGeneration/
│   │   │   ├── codedom.cs                      [MOVE FROM ROOT]
│   │   │   ├── codedomType.cs                  [MOVE FROM ROOT]
│   │   │   ├── roslynReplacesCodeDom.cs        [MOVE FROM ROOT]
│   │   │   └── mistralVB6Compiler.cs           [MOVE FROM ROOT]
│   │   ├── Converters/
│   │   │   ├── vb62cs12cvrt.cs                 [MOVE FROM ROOT]
│   │   │   ├── VariableConverter.cs            [TO CREATE]
│   │   │   ├── ControlFlowConverter.cs         [TO CREATE]
│   │   │   └── ErrorHandlingConverter.cs       [TO CREATE]
│   │   ├── Constants/
│   │   │   ├── VB6Constants.cs                 [MOVE FROM ROOT]
│   │   │   └── constants.cs                    [MOVE FROM ROOT]
│   │   └── Project/
│   │       ├── csprojclass.cs                  [MOVE FROM ROOT]
│   │       └── csprojgenerator.cs              [MOVE FROM ROOT]
│   ├── Phase3-FormsUI/
│   │   ├── FormParsing/
│   │   │   ├── frmParser.cs                    [MOVE FROM ROOT]
│   │   │   ├── vb6formparser.cs                [MOVE FROM ROOT]
│   │   │   ├── vb6formsparser.cs               [MOVE FROM ROOT]
│   │   │   └── vb6binary.cs                    [MOVE FROM ROOT]
│   │   ├── ControlMapping/
│   │   │   ├── vb6controlinfo.cs               [MOVE FROM ROOT]
│   │   │   ├── convertVB6AxControlCSharp.cs    [MOVE FROM ROOT]
│   │   │   └── WinformPixelToTableLayout.cs    [MOVE FROM ROOT]
│   │   ├── Layout/
│   │   │   └── LayoutConverter.cs              [TO CREATE]
│   │   └── Resources/
│   │       └── ResourceExtractor.cs            [TO CREATE]
│   ├── Phase4-DataAccess/
│   │   ├── Access/
│   │   │   ├── readAccessForms.cs              [MOVE FROM ROOT]
│   │   │   ├── accessExtraction.cs             [MOVE FROM ROOT]
│   │   │   ├── access2sql.cs                   [MOVE FROM ROOT]
│   │   │   ├── mcdfOleReader.cs                [MOVE FROM ROOT]
│   │   │   └── msaccess64bit.py                [MOVE FROM ROOT]
│   │   ├── ADO/
│   │   │   └── AdoConverter.cs                 [TO CREATE]
│   │   ├── SqlServer/
│   │   │   ├── SchemaGenerator.cs              [TO CREATE]
│   │   │   └── DataMigration.cs                [TO CREATE]
│   │   └── EntityFramework/
│   │       ├── DbContextGenerator.cs           [TO CREATE]
│   │       └── EntityGenerator.cs              [TO CREATE]
│   ├── Phase5-ASPtoAngular/
│   │   ├── AspParser/
│   │   │   ├── AspLexer.cs                     [TO CREATE]
│   │   │   ├── AspParser.cs                    [TO CREATE]
│   │   │   └── VBScriptParser.cs               [TO CREATE]
│   │   ├── Analysis/
│   │   │   ├── PageFlowAnalyzer.cs             [TO CREATE]
│   │   │   ├── SessionVariableTracker.cs       [TO CREATE]
│   │   │   └── DatabaseCallAnalyzer.cs         [TO CREATE]
│   │   ├── Backend/
│   │   │   ├── ApiGenerator/
│   │   │   │   ├── ControllerGenerator.cs      [TO CREATE]
│   │   │   │   ├── ServiceGenerator.cs         [TO CREATE]
│   │   │   │   └── DtoGenerator.cs             [TO CREATE]
│   │   │   └── Infrastructure/
│   │   │       ├── AuthConverter.cs            [TO CREATE]
│   │   │       └── MiddlewareGenerator.cs      [TO CREATE]
│   │   ├── Frontend/
│   │   │   ├── ComponentGenerator.cs           [TO CREATE]
│   │   │   ├── TemplateConverter.cs            [TO CREATE]
│   │   │   ├── FormConverter.cs                [TO CREATE]
│   │   │   └── RoutingGenerator.cs             [TO CREATE]
│   │   ├── Database/
│   │   │   ├── EFCoreGenerator.cs              [TO CREATE]
│   │   │   ├── RepositoryGenerator.cs          [TO CREATE]
│   │   │   └── MigrationScripts.cs             [TO CREATE]
│   │   └── RazorPages/
│   │       ├── Templates/
│   │       │   ├── blazer.razor                [MOVE FROM ROOT]
│   │       │   ├── gformviewer.razor           [MOVE FROM ROOT]
│   │       │   ├── gindex.razor                [MOVE FROM ROOT]
│   │       │   ├── glayiut.razor               [MOVE FROM ROOT]
│   │       │   ├── gnav.razor                  [MOVE FROM ROOT]
│   │       │   ├── gnavmenu.razor              [MOVE FROM ROOT]
│   │       │   ├── gqueryeditor.razor          [MOVE FROM ROOT]
│   │       │   ├── greportviewer.razor         [MOVE FROM ROOT]
│   │       │   └── gtableviewer.razor          [MOVE FROM ROOT]
│   │       ├── Styles/
│   │       │   └── gapp.css                    [MOVE FROM ROOT]
│   │       └── Scripts/
│   │           ├── accdb2sql.ps1               [MOVE FROM ROOT]
│   │           ├── acesss2razor.ps1            [MOVE FROM ROOT]
│   │           └── providers.ps1               [MOVE FROM ROOT]
│   ├── Phase6-Advanced/
│   │   ├── COM/
│   │   │   ├── typelibConverter.cs             [MOVE FROM ROOT]
│   │   │   └── determineInterop.cs             [MOVE FROM ROOT]
│   │   ├── LateBinding/
│   │   │   └── DynamicConverter.cs             [TO CREATE]
│   │   └── Collections/
│   │       └── CollectionConverter.cs          [TO CREATE]
│   ├── Phase7-Optimization/
│   │   ├── CodeCleanup/
│   │   │   └── DeadCodeRemover.cs              [TO CREATE]
│   │   ├── Refactoring/
│   │   │   └── LinqOptimizer.cs                [TO CREATE]
│   │   └── Documentation/
│   │       └── XmlDocGenerator.cs              [TO CREATE]
│   └── Phase8-Tooling/
│       ├── IDE/
│       │   ├── blmlide.cs                      [MOVE FROM ROOT]
│       │   ├── blmlide.designer.cs             [MOVE FROM ROOT]
│       │   ├── vb6-lsp.cs                      [MOVE FROM ROOT]
│       │   ├── vb6languageserverclient.cs      [MOVE FROM ROOT]
│       │   └── lspconfig.json                  [MOVE FROM ROOT]
│       ├── VSCode/
│       │   └── VB6.tmLanguage                  [MOVE FROM ROOT]
│       ├── CLI/
│       │   ├── mainprogm.cs                    [MOVE FROM ROOT]
│       │   └── CommandLineInterface.cs         [TO CREATE]
│       └── Web/
│           └── WebConverter.cs                 [TO CREATE]
├── tests/
│   ├── Unit/
│   │   └── MSUnitTestVB6.cs                    [MOVE FROM ROOT]
│   ├── Integration/
│   │   └── IntegrationTests.cs                 [TO CREATE]
│   └── TestData/
│       ├── generatesql.frm                     [MOVE FROM ROOT]
│       ├── vb6sux.cls                          [MOVE FROM ROOT]
│       └── vbcontrol.frm                       [MOVE FROM ROOT]
├── tools/
│   ├── Python/
│   │   ├── vb6frm2csharpfrmx.py                [MOVE FROM ROOT]
│   │   ├── generatetests.py                    [MOVE FROM ROOT]
│   │   └── searchText.py                       [MOVE FROM ROOT]
│   ├── PowerShell/
│   │   ├── haveaSQLServerCallRest.ps1          [MOVE FROM ROOT]
│   │   └── zippr.ps1                           [MOVE FROM ROOT]
│   ├── SQL/
│   │   └── SqlServerRunPSJob.sql               [MOVE FROM ROOT]
│   └── Utilities/
│       ├── csvwriter.cs                        [MOVE FROM ROOT]
│       ├── splitfile.cs                        [MOVE FROM ROOT]
│       └── makeonefile.cs                      [MOVE FROM ROOT]
├── samples/
│   └── VB6/
│       ├── Forms/
│       │   ├── generatesql.frm                 [COPY FROM tests]
│       │   └── vbcontrol.frm                   [COPY FROM tests]
│       └── Classes/
│           └── vb6sux.cls                      [COPY FROM tests]
├── AngularTutor/                               [KEEP AS IS - SEPARATE PROJECT]
└── migration/                                  [KEEP AS IS - MIGRATION ARTIFACTS]
```

---

## **Migration Steps**

### Step 1: Create New Folder Structure
```bash
mkdir -p src/Phase{1..8}-{Foundation,CoreLanguage,FormsUI,DataAccess,ASPtoAngular,Advanced,Optimization,Tooling}
mkdir -p docs/{Reference,Migration,Training,Lists}
mkdir -p tests/{Unit,Integration,TestData}
mkdir -p tools/{Python,PowerShell,SQL,Utilities}
mkdir -p samples/VB6/{Forms,Classes}
```

### Step 2: Move Documentation Files
```bash
# Reference docs
mv Keywords.md KeywordsControlsFunctions.md LLMRagDataset.md Pseudocode.md docs/Reference/
mv RFC-DCLR.md RealtimeRequirements.md CreateVB6TrainingDS.md diagramFormats.md docs/Reference/
mv howllmswork.md vbnetpropsfromvb6.md docs/Reference/

# Migration docs
mv access2razor.md efcoreandrazor.md makeReactRazor.md razorNoEF.md docs/Migration/
mv mdbsolutions.md transpile-pipe.md vbnetvb6binary.md docs/Migration/

# Training data
mv exampleCSharpLLMDataset.json howtocreatedataset.htm docs/Training/

# Lists
mv constants.txt vb6keywords.txt vb6controlproplist.txt docs/Lists/
mv vb6controlprops.csv vb6controlproptypes.txt docs/Lists/
```

### Step 3: Move Phase 1 Files (Foundation)
```bash
# Parser
mv VB6Parser.cs IScannerParser.cs preprocess.cs trees.cs src/Phase1-Foundation/Parser/

# Lexer
mv VB6Keywords.cs parseVB6Constants.cs parsevb6Constants2.cs src/Phase1-Foundation/Lexer/
mv parsebuiltinfunctions.cs src/Phase1-Foundation/Lexer/

# Symbol table
mv ImmutableSymbolTable src/Phase1-Foundation/SymbolTable/
```

### Step 4: Move Phase 2 Files (Core Language)
```bash
# Code generation
mv codedom.cs codedomType.cs roslynReplacesCodeDom.cs src/Phase2-CoreLanguage/CodeGeneration/
mv mistralVB6Compiler.cs src/Phase2-CoreLanguage/CodeGeneration/

# Converters
mv vb62cs12cvrt.cs src/Phase2-CoreLanguage/Converters/

# Constants
mv VB6Constants.cs constants.cs src/Phase2-CoreLanguage/Constants/

# Project files
mv csprojclass.cs csprojgenerator.cs src/Phase2-CoreLanguage/Project/
```

### Step 5: Move Phase 3 Files (Forms & UI)
```bash
# Form parsing
mv frmParser.cs vb6formparser.cs vb6formsparser.cs src/Phase3-FormsUI/FormParsing/
mv vb6binary.cs src/Phase3-FormsUI/FormParsing/

# Control mapping
mv vb6controlinfo.cs convertVB6AxControlCSharp.cs src/Phase3-FormsUI/ControlMapping/
mv WinformPixelToTableLayout.cs src/Phase3-FormsUI/ControlMapping/
```

### Step 6: Move Phase 4 Files (Data Access)
```bash
# Access database
mv readAccessForms.cs accessExtraction.cs access2sql.cs src/Phase4-DataAccess/Access/
mv mcdfOleReader.cs msaccess64bit.py src/Phase4-DataAccess/Access/
```

### Step 7: Move Phase 5 Files (ASP to Angular)
```bash
# Razor templates
mv *.razor src/Phase5-ASPtoAngular/RazorPages/Templates/

# Styles
mv gapp.css src/Phase5-ASPtoAngular/RazorPages/Styles/

# Scripts
mv accdb2sql.ps1 acesss2razor.ps1 providers.ps1 src/Phase5-ASPtoAngular/RazorPages/Scripts/
```

### Step 8: Move Phase 6 Files (Advanced)
```bash
# COM interop
mv typelibConverter.cs determineInterop.cs src/Phase6-Advanced/COM/
```

### Step 9: Move Phase 8 Files (Tooling)
```bash
# IDE
mv blmlide.cs blmlide.designer.cs src/Phase8-Tooling/IDE/
mv vb6-lsp.cs vb6languageserverclient.cs lspconfig.json src/Phase8-Tooling/IDE/

# VS Code
mv VB6.tmLanguage src/Phase8-Tooling/VSCode/

# CLI
mv mainprogm.cs src/Phase8-Tooling/CLI/
```

### Step 10: Move Test Files
```bash
mv MSUnitTestVB6.cs tests/Unit/
mv generatesql.frm vb6sux.cls vbcontrol.frm tests/TestData/
```

### Step 11: Move Tool Files
```bash
# Python tools
mv vb6frm2csharpfrmx.py generatetests.py searchText.py tools/Python/

# PowerShell
mv haveaSQLServerCallRest.ps1 zippr.ps1 tools/PowerShell/

# SQL
mv SqlServerRunPSJob.sql tools/SQL/

# Utilities
mv csvwriter.cs splitfile.cs makeonefile.cs tools/Utilities/
```

### Step 12: Move Sample Files
```bash
cp tests/TestData/generatesql.frm samples/VB6/Forms/
cp tests/TestData/vbcontrol.frm samples/VB6/Forms/
cp tests/TestData/vb6sux.cls samples/VB6/Classes/
```

### Step 13: Miscellaneous Files
```bash
# Keep existing location
# - .github/workflows/
# - AngularTutor/ (separate project)
# - migration/ (migration artifacts)
# - ambiguit md (review and move to docs if needed)
```

---

## **File Status Legend**

- **[MOVE FROM ROOT]** - File exists, needs to be moved
- **[TO CREATE]** - File needs to be created to complete the phase
- **[KEEP AS IS]** - File/folder should remain in current location
- **[COPY FROM tests]** - Create a copy for samples

---

## **Implementation Priority by Phase**

### Immediate (Week 1)
1. Create folder structure
2. Move all documentation files
3. Move Phase 1 files (Foundation)
4. Move Phase 2 files (Core Language)

### Short Term (Weeks 2-3)
5. Move Phase 3 files (Forms)
6. Move Phase 4 files (Data Access)
7. Move tooling and test files

### Medium Term (Weeks 4-6)
8. Move Phase 5 files (ASP to Angular)
9. Move Phase 6 files (Advanced)
10. Create missing foundation files (AST, Symbol Table, etc.)

### Long Term (Ongoing)
11. Create missing converter files for each phase
12. Build out test suites
13. Complete documentation

---

## **Files Needing Review**

The following files may need review before moving:

1. **ambiguit md** - Unclear purpose, review content
2. **CombinedClasses.cs** - May need to be split across phases
3. **gptvb6.cs** - Determine if this is a utility or core component
4. **READMEInput.md** - Merge into main README or docs

---

## **Git Commands for Reorganization**

```bash
# Option 1: Preserve history (recommended)
git mv <source> <destination>

# Option 2: Bulk reorganization script
# Create a script file: reorganize.sh

#!/bin/bash
# See individual migration steps above
# Use git mv for each file to preserve history

# Option 3: After manual moves
git add -A
git commit -m "Reorganize repository structure according to ProjectPlan.md"
```

---

## **Post-Reorganization Tasks**

1. **Update .csproj files** - Fix any broken references
2. **Update imports** - Fix namespace references in moved files
3. **Update documentation** - Fix links to moved files
4. **Update CI/CD** - Adjust build paths in workflows
5. **Create README.md in each phase folder** - Explain that phase's purpose
6. **Add .gitkeep files** - In empty folders to preserve structure

---

## **Benefits of This Organization**

1. **Clear Phase Boundaries** - Easy to see what's complete vs. pending
2. **Parallel Development** - Multiple teams can work on different phases
3. **Better Onboarding** - New developers can focus on specific phases
4. **Testability** - Each phase can have isolated tests
5. **Documentation** - Phase-specific docs make learning easier
6. **Tracking Progress** - GitHub issues/projects can align with phases

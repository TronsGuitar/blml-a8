# reorganize-repo.ps1
# Automated repository reorganization according to ProjectPlan.md

param(
    [Parameter(Mandatory = $false)]
    [switch]$DryRun = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$UseGitMv = $true
)

$ErrorActionPreference = "Stop"

function Move-FileWithGit {
    param(
        [string]$Source,
        [string]$Destination,
        [bool]$DryRun,
        [bool]$UseGit
    )
    
    if (-not (Test-Path $Source)) {
        Write-Warning "Source file not found: $Source"
        return
    }
    
    $destDir = Split-Path -Parent $Destination
    if (-not (Test-Path $destDir)) {
        if ($DryRun) {
            Write-Host "[DRY RUN] Would create directory: $destDir" -ForegroundColor Yellow
        }
        else {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
            Write-Host "Created directory: $destDir" -ForegroundColor Green
        }
    }
    
    if ($DryRun) {
        Write-Host "[DRY RUN] Would move: $Source -> $Destination" -ForegroundColor Cyan
    }
    else {
        if ($UseGit) {
            git mv $Source $Destination
            Write-Host "Git moved: $Source -> $Destination" -ForegroundColor Green
        }
        else {
            Move-Item -Path $Source -Destination $Destination -Force
            Write-Host "Moved: $Source -> $Destination" -ForegroundColor Green
        }
    }
}

Write-Host "=== Repository Reorganization Script ===" -ForegroundColor Magenta
Write-Host "Dry Run: $DryRun" -ForegroundColor Magenta
Write-Host "Use Git MV: $UseGitMv" -ForegroundColor Magenta
Write-Host ""

# Step 1: Create base folder structure
Write-Host "Step 1: Creating folder structure..." -ForegroundColor Yellow
$folders = @(
    "src/Phase1-Foundation/Parser",
    "src/Phase1-Foundation/Lexer",
    "src/Phase1-Foundation/SymbolTable",
    "src/Phase1-Foundation/AST",
    "src/Phase1-Foundation/TypeInference",
    "src/Phase1-Foundation/ProjectModel",
    "src/Phase1-Foundation/DependencyGraph",
    "src/Phase2-CoreLanguage/CodeGeneration",
    "src/Phase2-CoreLanguage/Converters",
    "src/Phase2-CoreLanguage/Constants",
    "src/Phase2-CoreLanguage/Project",
    "src/Phase3-FormsUI/FormParsing",
    "src/Phase3-FormsUI/ControlMapping",
    "src/Phase3-FormsUI/Layout",
    "src/Phase3-FormsUI/Resources",
    "src/Phase4-DataAccess/Access",
    "src/Phase4-DataAccess/ADO",
    "src/Phase4-DataAccess/SqlServer",
    "src/Phase4-DataAccess/EntityFramework",
    "src/Phase5-ASPtoAngular/AspParser",
    "src/Phase5-ASPtoAngular/Analysis",
    "src/Phase5-ASPtoAngular/Backend/ApiGenerator",
    "src/Phase5-ASPtoAngular/Backend/Infrastructure",
    "src/Phase5-ASPtoAngular/Frontend",
    "src/Phase5-ASPtoAngular/Database",
    "src/Phase5-ASPtoAngular/RazorPages/Templates",
    "src/Phase5-ASPtoAngular/RazorPages/Styles",
    "src/Phase5-ASPtoAngular/RazorPages/Scripts",
    "src/Phase6-Advanced/COM",
    "src/Phase6-Advanced/LateBinding",
    "src/Phase6-Advanced/Collections",
    "src/Phase7-Optimization/CodeCleanup",
    "src/Phase7-Optimization/Refactoring",
    "src/Phase7-Optimization/Documentation",
    "src/Phase8-Tooling/IDE",
    "src/Phase8-Tooling/VSCode",
    "src/Phase8-Tooling/CLI",
    "src/Phase8-Tooling/Web",
    "docs/Reference",
    "docs/Migration",
    "docs/Training",
    "docs/Lists",
    "tests/Unit",
    "tests/Integration",
    "tests/TestData",
    "tools/Python",
    "tools/PowerShell",
    "tools/SQL",
    "tools/Utilities",
    "samples/VB6/Forms",
    "samples/VB6/Classes"
)

foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        if ($DryRun) {
            Write-Host "[DRY RUN] Would create: $folder" -ForegroundColor Yellow
        }
        else {
            New-Item -ItemType Directory -Path $folder -Force | Out-Null
            Write-Host "Created: $folder" -ForegroundColor Green
        }
    }
}

# Step 2: Move Documentation Files
Write-Host "`nStep 2: Moving documentation files..." -ForegroundColor Yellow

$docMoves = @{
    # Reference docs
    "Keywords.md"                  = "docs/Reference/Keywords.md"
    "KeywordsControlsFunctions.md" = "docs/Reference/KeywordsControlsFunctions.md"
    "LLMRagDataset.md"             = "docs/Reference/LLMRagDataset.md"
    "Pseudocode.md"                = "docs/Reference/Pseudocode.md"
    "RFC-DCLR.md"                  = "docs/Reference/RFC-DCLR.md"
    "RealtimeRequirements.md"      = "docs/Reference/RealtimeRequirements.md"
    "CreateVB6TrainingDS.md"       = "docs/Reference/CreateVB6TrainingDS.md"
    "diagramFormats.md"            = "docs/Reference/diagramFormats.md"
    "howllmswork.md"               = "docs/Reference/howllmswork.md"
    "vbnetpropsfromvb6.md"         = "docs/Reference/vbnetpropsfromvb6.md"
    
    # Migration docs
    "access2razor.md"              = "docs/Migration/access2razor.md"
    "efcoreandrazor.md"            = "docs/Migration/efcoreandrazor.md"
    "makeReactRazor.md"            = "docs/Migration/makeReactRazor.md"
    "razorNoEF.md"                 = "docs/Migration/razorNoEF.md"
    "mdbsolutions.md"              = "docs/Migration/mdbsolutions.md"
    "transpile-pipe.md"            = "docs/Migration/transpile-pipe.md"
    "vbnetvb6binary.md"            = "docs/Migration/vbnetvb6binary.md"
    
    # Training
    "exampleCSharpLLMDataset.json" = "docs/Training/exampleCSharpLLMDataset.json"
    "howtocreatedataset.htm"       = "docs/Training/howtocreatedataset.htm"
    
    # Lists
    "constants.txt"                = "docs/Lists/constants.txt"
    "vb6keywords.txt"              = "docs/Lists/vb6keywords.txt"
    "vb6controlproplist.txt"       = "docs/Lists/vb6controlproplist.txt"
    "vb6controlprops.csv"          = "docs/Lists/vb6controlprops.csv"
    "vb6controlproptypes.txt"      = "docs/Lists/vb6controlproptypes.txt"
}

foreach ($move in $docMoves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 3: Move Phase 1 Files
Write-Host "`nStep 3: Moving Phase 1 (Foundation) files..." -ForegroundColor Yellow

$phase1Moves = @{
    # Parser
    "VB6Parser.cs"             = "src/Phase1-Foundation/Parser/VB6Parser.cs"
    "IScannerParser.cs"        = "src/Phase1-Foundation/Parser/IScannerParser.cs"
    "preprocess.cs"            = "src/Phase1-Foundation/Parser/preprocess.cs"
    "trees.cs"                 = "src/Phase1-Foundation/Parser/trees.cs"
    
    # Lexer
    "VB6Keywords.cs"           = "src/Phase1-Foundation/Lexer/VB6Keywords.cs"
    "parseVB6Constants.cs"     = "src/Phase1-Foundation/Lexer/parseVB6Constants.cs"
    "parsevb6Constants2.cs"    = "src/Phase1-Foundation/Lexer/parsevb6Constants2.cs"
    "parsebuiltinfunctions.cs" = "src/Phase1-Foundation/Lexer/parsebuiltinfunctions.cs"
    
    # Symbol table
    "ImmutableSymbolTable"     = "src/Phase1-Foundation/SymbolTable/ImmutableSymbolTable"
}

foreach ($move in $phase1Moves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 4: Move Phase 2 Files
Write-Host "`nStep 4: Moving Phase 2 (Core Language) files..." -ForegroundColor Yellow

$phase2Moves = @{
    # Code generation
    "codedom.cs"               = "src/Phase2-CoreLanguage/CodeGeneration/codedom.cs"
    "codedomType.cs"           = "src/Phase2-CoreLanguage/CodeGeneration/codedomType.cs"
    "roslynReplacesCodeDom.cs" = "src/Phase2-CoreLanguage/CodeGeneration/roslynReplacesCodeDom.cs"
    "mistralVB6Compiler.cs"    = "src/Phase2-CoreLanguage/CodeGeneration/mistralVB6Compiler.cs"
    
    # Converters
    "vb62cs12cvrt.cs"          = "src/Phase2-CoreLanguage/Converters/vb62cs12cvrt.cs"
    
    # Constants
    "VB6Constants.cs"          = "src/Phase2-CoreLanguage/Constants/VB6Constants.cs"
    "constants.cs"             = "src/Phase2-CoreLanguage/Constants/constants.cs"
    
    # Project
    "csprojclass.cs"           = "src/Phase2-CoreLanguage/Project/csprojclass.cs"
    "csprojgenerator.cs"       = "src/Phase2-CoreLanguage/Project/csprojgenerator.cs"
}

foreach ($move in $phase2Moves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 5: Move Phase 3 Files
Write-Host "`nStep 5: Moving Phase 3 (Forms & UI) files..." -ForegroundColor Yellow

$phase3Moves = @{
    # Form parsing
    "frmParser.cs"                 = "src/Phase3-FormsUI/FormParsing/frmParser.cs"
    "vb6formparser.cs"             = "src/Phase3-FormsUI/FormParsing/vb6formparser.cs"
    "vb6formsparser.cs"            = "src/Phase3-FormsUI/FormParsing/vb6formsparser.cs"
    "vb6binary.cs"                 = "src/Phase3-FormsUI/FormParsing/vb6binary.cs"
    
    # Control mapping
    "vb6controlinfo.cs"            = "src/Phase3-FormsUI/ControlMapping/vb6controlinfo.cs"
    "convertVB6AxControlCSharp.cs" = "src/Phase3-FormsUI/ControlMapping/convertVB6AxControlCSharp.cs"
    "WinformPixelToTableLayout.cs" = "src/Phase3-FormsUI/ControlMapping/WinformPixelToTableLayout.cs"
}

foreach ($move in $phase3Moves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 6: Move Phase 4 Files
Write-Host "`nStep 6: Moving Phase 4 (Data Access) files..." -ForegroundColor Yellow

$phase4Moves = @{
    "readAccessForms.cs"  = "src/Phase4-DataAccess/Access/readAccessForms.cs"
    "accessExtraction.cs" = "src/Phase4-DataAccess/Access/accessExtraction.cs"
    "access2sql.cs"       = "src/Phase4-DataAccess/Access/access2sql.cs"
    "mcdfOleReader.cs"    = "src/Phase4-DataAccess/Access/mcdfOleReader.cs"
    "msaccess64bit.py"    = "src/Phase4-DataAccess/Access/msaccess64bit.py"
}

foreach ($move in $phase4Moves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 7: Move Phase 5 Files
Write-Host "`nStep 7: Moving Phase 5 (ASP to Angular) files..." -ForegroundColor Yellow

# Razor templates
$razorFiles = Get-ChildItem -Filter "*.razor" -ErrorAction SilentlyContinue
foreach ($file in $razorFiles) {
    Move-FileWithGit -Source $file.Name -Destination "src/Phase5-ASPtoAngular/RazorPages/Templates/$($file.Name)" -DryRun $DryRun -UseGit $UseGitMv
}

$phase5Moves = @{
    "gapp.css"         = "src/Phase5-ASPtoAngular/RazorPages/Styles/gapp.css"
    "accdb2sql.ps1"    = "src/Phase5-ASPtoAngular/RazorPages/Scripts/accdb2sql.ps1"
    "acesss2razor.ps1" = "src/Phase5-ASPtoAngular/RazorPages/Scripts/acesss2razor.ps1"
    "providers.ps1"    = "src/Phase5-ASPtoAngular/RazorPages/Scripts/providers.ps1"
}

foreach ($move in $phase5Moves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 8: Move Phase 6 Files
Write-Host "`nStep 8: Moving Phase 6 (Advanced) files..." -ForegroundColor Yellow

$phase6Moves = @{
    "typelibConverter.cs" = "src/Phase6-Advanced/COM/typelibConverter.cs"
    "determineInterop.cs" = "src/Phase6-Advanced/COM/determineInterop.cs"
}

foreach ($move in $phase6Moves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 9: Move Phase 8 Files
Write-Host "`nStep 9: Moving Phase 8 (Tooling) files..." -ForegroundColor Yellow

$phase8Moves = @{
    "blmlide.cs"                 = "src/Phase8-Tooling/IDE/blmlide.cs"
    "blmlide.designer.cs"        = "src/Phase8-Tooling/IDE/blmlide.designer.cs"
    "vb6-lsp.cs"                 = "src/Phase8-Tooling/IDE/vb6-lsp.cs"
    "vb6languageserverclient.cs" = "src/Phase8-Tooling/IDE/vb6languageserverclient.cs"
    "lspconfig.json"             = "src/Phase8-Tooling/IDE/lspconfig.json"
    "VB6.tmLanguage"             = "src/Phase8-Tooling/VSCode/VB6.tmLanguage"
    "mainprogm.cs"               = "src/Phase8-Tooling/CLI/mainprogm.cs"
}

foreach ($move in $phase8Moves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 10: Move Test Files
Write-Host "`nStep 10: Moving test files..." -ForegroundColor Yellow

$testMoves = @{
    "MSUnitTestVB6.cs" = "tests/Unit/MSUnitTestVB6.cs"
    "generatesql.frm"  = "tests/TestData/generatesql.frm"
    "vb6sux.cls"       = "tests/TestData/vb6sux.cls"
    "vbcontrol.frm"    = "tests/TestData/vbcontrol.frm"
}

foreach ($move in $testMoves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 11: Move Tool Files
Write-Host "`nStep 11: Moving tool files..." -ForegroundColor Yellow

$toolMoves = @{
    # Python
    "vb6frm2csharpfrmx.py"       = "tools/Python/vb6frm2csharpfrmx.py"
    "generatetests.py"           = "tools/Python/generatetests.py"
    "searchText.py"              = "tools/Python/searchText.py"
    
    # PowerShell
    "haveaSQLServerCallRest.ps1" = "tools/PowerShell/haveaSQLServerCallRest.ps1"
    "zippr.ps1"                  = "tools/PowerShell/zippr.ps1"
    
    # SQL
    "SqlServerRunPSJob.sql"      = "tools/SQL/SqlServerRunPSJob.sql"
    
    # Utilities
    "csvwriter.cs"               = "tools/Utilities/csvwriter.cs"
    "splitfile.cs"               = "tools/Utilities/splitfile.cs"
    "makeonefile.cs"             = "tools/Utilities/makeonefile.cs"
}

foreach ($move in $toolMoves.GetEnumerator()) {
    Move-FileWithGit -Source $move.Key -Destination $move.Value -DryRun $DryRun -UseGit $UseGitMv
}

# Step 12: Copy Sample Files
Write-Host "`nStep 12: Copying sample files..." -ForegroundColor Yellow

if (-not $DryRun) {
    if (Test-Path "tests/TestData/generatesql.frm") {
        Copy-Item "tests/TestData/generatesql.frm" "samples/VB6/Forms/" -Force
    }
    if (Test-Path "tests/TestData/vbcontrol.frm") {
        Copy-Item "tests/TestData/vbcontrol.frm" "samples/VB6/Forms/" -Force
    }
    if (Test-Path "tests/TestData/vb6sux.cls") {
        Copy-Item "tests/TestData/vb6sux.cls" "samples/VB6/Classes/" -Force
    }
}

Write-Host "`n=== Reorganization Complete ===" -ForegroundColor Green

if ($DryRun) {
    Write-Host "`nThis was a DRY RUN. No files were actually moved." -ForegroundColor Yellow
    Write-Host "Run without -DryRun parameter to perform actual reorganization." -ForegroundColor Yellow
}
else {
    Write-Host "`nFiles have been reorganized according to ProjectPlan.md" -ForegroundColor Green
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Review the changes with: git status" -ForegroundColor White
    Write-Host "  2. Commit the changes with: git commit -m 'Reorganize repository structure'" -ForegroundColor White
    Write-Host "  3. Update any broken references in project files" -ForegroundColor White
    Write-Host "  4. Update namespace declarations in moved C# files" -ForegroundColor White
}

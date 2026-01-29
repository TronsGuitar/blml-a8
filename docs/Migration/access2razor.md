Here’s a PowerShell script that automates the conversion of an MS Access .accdb database to SQL Server. This script:

Extracts tables from Access and migrates them to SQL Server.

Converts Access queries into SQL Server stored procedures.

Generates Razor class code for forms, modules, and reports.


You'll need:

1. Access Database (.accdb)


2. SQL Server Instance


3. Access Database Engine installed


4. SQL Server Management Studio (SSMS) or SQLCMD for execution




---

PowerShell Script

param(
    [string]$AccessFilePath = "C:\Path\To\Your\Database.accdb",
    [string]$SqlServer = "YourSqlServer",
    [string]$DatabaseName = "YourSqlDatabase",
    [string]$OutputDir = "C:\Path\To\Output"
)

# Ensure output directory exists
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# Load Access COM Object
$access = New-Object -ComObject Access.Application
$access.OpenCurrentDatabase($AccessFilePath)

# Connect to Access and extract tables
$tables = $access.CurrentData.AllTables | Where-Object { $_.Name -notmatch "^~" -and $_.Name -notmatch "MSys" }
$queries = $access.CurrentData.AllQueries

# Generate SQL Server table scripts
$sqlTables = @"
USE [$DatabaseName];
"@

foreach ($table in $tables) {
    $tableName = $table.Name
    $columns = $access.CurrentDb.TableDefs[$tableName].Fields
    $columnSql = @()

    foreach ($column in $columns) {
        $colName = $column.Name
        $colType = Switch ($column.Type) {
            1 { "BIT" }          # Yes/No
            2 { "SMALLINT" }     # Integer
            3 { "INTEGER" }      # Long Integer
            4 { "CURRENCY" }     # Currency
            5 { "FLOAT" }        # Single
            6 { "FLOAT" }        # Double
            7 { "DATETIME" }     # Date/Time
            10 { "TEXT(255)" }   # Text
            12 { "MEMO" }        # Long Text
            20 { "BIGINT" }      # Replication ID
            Default { "NVARCHAR(255)" } # Fallback
        }
        $columnSql += "[$colName] $colType"
    }

    $sqlTables += "CREATE TABLE [$tableName] (" + ($columnSql -join ", ") + ");`n"
}

# Write to SQL file
$sqlTables | Out-File "$OutputDir\Tables.sql"

# Convert Queries to Stored Procedures
$sqlProcedures = @"
USE [$DatabaseName];
"@

foreach ($query in $queries) {
    $queryName = $query.Name
    $querySQL = $access.CurrentDb.QueryDefs[$queryName].SQL

    $sqlProcedures += @"
CREATE PROCEDURE [$queryName]
AS
BEGIN
    $querySQL
END;
"@
}

$sqlProcedures | Out-File "$OutputDir\StoredProcedures.sql"

# Extract Forms, Reports, Modules to Razor Pages
$forms = $access.CurrentProject.AllForms
$modules = $access.CurrentProject.AllModules
$reports = $access.CurrentProject.AllReports

function Convert-ToRazorClass($objName, $type) {
    $code = @"
@page "/$objName"
@model $objName

<h1>$objName</h1>

<form>
    <!-- Replace with actual fields -->
    <input type="text" placeholder="Field1" />
    <button type="submit">Submit</button>
</form>
"@
    return $code
}

foreach ($form in $forms) {
    $formName = $form.Name
    Convert-ToRazorClass -objName $formName -type "Form" | Out-File "$OutputDir\$formName.cshtml"
}

foreach ($report in $reports) {
    $reportName = $report.Name
    Convert-ToRazorClass -objName $reportName -type "Report" | Out-File "$OutputDir\$reportName.cshtml"
}

foreach ($module in $modules) {
    $moduleName = $module.Name
    $moduleCode = $access.Modules[$moduleName].Lines(1, $access.Modules[$moduleName].CountOfLines)
    $moduleCode | Out-File "$OutputDir\$moduleName.cs"
}

# Close Access
$access.CloseCurrentDatabase()
$access.Quit()

Write-Host "Conversion completed! Files are in $OutputDir"


---

What This Script Does

1. Migrates Tables

Reads table definitions and converts them into SQL Server tables.

Handles data types mapping from Access to SQL Server.



2. Converts Queries to Stored Procedures

Extracts SQL from Access queries and converts them into stored procedures.



3. Generates Razor Pages for Forms and Reports

Generates a simple Razor form structure for each form and report.



4. Extracts Modules and Converts to C# Files

Extracts VBA module code and saves it as .cs files for further conversion.





---

How to Run

Open PowerShell and execute:


.\Convert-AccessToSQL.ps1 -AccessFilePath "C:\Path\To\Your\Database.accdb" -SqlServer "YourSqlServer" -DatabaseName "YourSqlDatabase" -OutputDir "C:\Path\To\Output"

Import the generated .sql scripts into SQL Server using SSMS or sqlcmd.



---

Next Steps

Convert VBA to C# manually or use an AI-based transpiler.

Enhance Razor UI to match business logic.

Validate complex queries to ensure proper SQL Server execution.


Would you like any modifications or explanations for specific parts?


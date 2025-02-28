Add-Type -AssemblyName System.Windows.Forms

# Prompt to select the Access database file
$openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
$openFileDialog.Filter = "Access Database (*.accdb)|*.accdb"
$openFileDialog.Title = "Select Access Database File"
if ($openFileDialog.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) {
    Write-Host "No file selected. Exiting."
    exit
}
$accessFilePath = $openFileDialog.FileName

# Prompt to select the output folder for generated code
$folderBrowserDialog = New-Object System.Windows.Forms.FolderBrowserDialog
$folderBrowserDialog.Description = "Select the output folder for generated Razor and C# code"
if ($folderBrowserDialog.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) {
    Write-Host "No folder selected. Exiting."
    exit
}
$outputDir = $folderBrowserDialog.SelectedPath

$accessConnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Path\To\YourDatabase.accdb;Persist Security Info=False;"
$accessConnection = New-Object System.Data.OleDb.OleDbConnection($accessConnStr)
$accessConnection.Open()
$allTables = $accessConnection.GetSchema("Tables")
$accessConnection.Close()

$allTables | Format-Table TABLE_NAME, TABLE_TYPE -AutoSize

# Create subdirectories for Models and Pages if they don't exist
$modelsDir = Join-Path $outputDir "Models"
$pagesDir = Join-Path $outputDir "Pages"
if (!(Test-Path $modelsDir)) { New-Item -ItemType Directory -Path $modelsDir | Out-Null }
if (!(Test-Path $pagesDir)) { New-Item -ItemType Directory -Path $pagesDir | Out-Null }

Write-Host "Using Access file: $accessFilePath"
Write-Host "Output directory: $outputDir"
Write-Host "Models will be saved in: $modelsDir"
Write-Host "Pages will be saved in: $pagesDir"

# Connection string for the Access database
$accessConnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$accessFilePath;Persist Security Info=False;"

# Open the Access connection
$accessConnection = New-Object System.Data.OleDb.OleDbConnection($accessConnStr)
try {
    $accessConnection.Open()
} catch {
    Write-Host "Error opening Access connection: $_"
    exit
}

# Get the list of user tables
$tables = $accessConnection.GetSchema("Tables") | Where-Object { $_.TABLE_TYPE -eq "TABLE" }
Write-Host "Found $($tables.Count) tables in the Access database."

# Function to map Access data types to C# types
function MapAccessToCSharpType($accessType) {
    switch ($accessType) {
        2   { return "short" }       # dbInteger
        3   { return "int" }         # dbLong
        4   { return "float" }       # dbSingle
        5   { return "double" }      # dbDouble
        6   { return "decimal" }     # dbCurrency
        7   { return "DateTime" }    # dbDate
        130 { return "string" }      # dbText
        201 { return "string" }      # dbMemo
        11  { return "bool" }        # dbBoolean
        default { return "string" }
    }
}

foreach ($table in $tables) {
    $tableName = $table.TABLE_NAME
    Write-Host "Processing table: $tableName"

    # Get column schema for the table
    $colRestrictions = @($null, $null, $tableName, $null)
    $columns = $accessConnection.GetSchema("Columns", $colRestrictions)
    Write-Host "Found $($columns.Rows.Count) columns for table: $tableName"

    if ($columns.Rows.Count -eq 0) {
        Write-Host "Skipping table $tableName as no columns were found."
        continue
    }

    # Build the properties for the C# model
    $modelProperties = ""
    foreach ($col in $columns.Rows) {
        $colName = $col.COLUMN_NAME
        $dataType = $col.DATA_TYPE
        $csharpType = MapAccessToCSharpType $dataType
        $modelProperties += "        public $csharpType $colName { get; set; }`r`n"
    }
    
    # Create the C# model class
    $modelClass = @"
using System;
using System.Collections.Generic;

namespace YourNamespace.Models
{
    public class $tableName
    {
$modelProperties
    }
}
"@
    $modelFile = Join-Path $modelsDir "$tableName.cs"
    Write-Host "Writing model file: $modelFile"
    $modelClass | Out-File -Encoding utf8 $modelFile

    # Generate a basic Razor Index page for listing items
    $razorPage = @"
@page
@model YourNamespace.Pages.$tableName.IndexModel
@{
    ViewData[""Title""] = ""$tableName List"";
}

<h1>$tableName List</h1>

<table>
    <thead>
        <tr>
"@
    foreach ($col in $columns.Rows) {
        $razorPage += "            <th>$($col.COLUMN_NAME)</th>`r`n"
    }
    $razorPage += @"
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model.Items)
        {
            <tr>
"@
    foreach ($col in $columns.Rows) {
        $razorPage += "                <td>@item.$($col.COLUMN_NAME)</td>`r`n"
    }
    $razorPage += @"
                <td>
                    <a asp-page="".\Edit"" asp-route-id=""@item.Id"">Edit</a> |
                    <a asp-page="".\Details"" asp-route-id=""@item.Id"">Details</a> |
                    <a asp-page="".\Delete"" asp-route-id=""@item.Id"">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>

<a asp-page="".\Create"">Create New</a>
"@
    $razorFile = Join-Path $pagesDir "$tableName.Index.cshtml"
    Write-Host "Writing Razor page: $razorFile"
    $razorPage | Out-File -Encoding utf8 $razorFile
}

$accessConnection.Close()
Write-Host "Code generation complete. Files are saved in $outputDir"

Add-Type -AssemblyName System.Windows.Forms

# Helper function to sanitize names by removing all non-alphanumeric characters.
function SanitizeName($name) {
    # Remove all non-alphanumeric characters.
    $sanitized = $name -replace '[^a-zA-Z0-9]', ''
    # If the sanitized name starts with a digit, add an underscore at the beginning.
    if ($sanitized -match '^[0-9]') {
        $sanitized = "_" + $sanitized
    }
    return $sanitized
}

# Prompt to select the Access database file.
$openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
$openFileDialog.Filter = "Access Database (*.accdb)|*.accdb"
$openFileDialog.Title = "Select Access Database File"
if ($openFileDialog.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) {
    Write-Host "No file selected. Exiting."
    exit
}
$accessFilePath = $openFileDialog.FileName

# Prompt to select the output folder for generated code.
$folderBrowserDialog = New-Object System.Windows.Forms.FolderBrowserDialog
$folderBrowserDialog.Description = "Select the output folder for generated Razor and C# code"
if ($folderBrowserDialog.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) {
    Write-Host "No folder selected. Exiting."
    exit
}
$outputDir = $folderBrowserDialog.SelectedPath

# Create subdirectories for Models and Pages if they don't exist.
$modelsDir = Join-Path $outputDir "Models"
$pagesDir = Join-Path $outputDir "Pages"
if (!(Test-Path $modelsDir)) { New-Item -ItemType Directory -Path $modelsDir | Out-Null }
if (!(Test-Path $pagesDir)) { New-Item -ItemType Directory -Path $pagesDir | Out-Null }

Write-Host "Using Access file: $accessFilePath"
Write-Host "Output directory: $outputDir"
Write-Host "Models will be saved in: $modelsDir"
Write-Host "Pages will be saved in: $pagesDir"

# Connection string for the Access database.
$accessConnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$accessFilePath;Persist Security Info=False;"

# Open the Access connection.
$accessConnection = New-Object System.Data.OleDb.OleDbConnection($accessConnStr)
try {
    $accessConnection.Open()
} catch {
    Write-Host "Error opening Access connection: $_"
    exit
}

# Get the list of tables (including local and linked).
$allTables = $accessConnection.GetSchema("Tables")
$tables = $allTables | Where-Object { @("TABLE", "LINK") -contains $_.TABLE_TYPE }
Write-Host "Found $($tables.Count) user tables (local or linked) in the Access database."

# Retrieve all columns once.
$allColumns = $accessConnection.GetSchema("Columns")

# Function to map Access data types to C# types.
function MapAccessToCSharpType($accessType) {
    switch ($accessType) {
        2   { return "short" }
        3   { return "int" }
        4   { return "float" }
        5   { return "double" }
        6   { return "decimal" }
        7   { return "DateTime" }
        130 { return "string" }
        201 { return "string" }
        11  { return "bool" }
        default { return "string" }
    }
}

# List to hold sanitized table names for the main menu generation.
$tableNames = @()

foreach ($table in $tables) {
    $tableName = $table.TABLE_NAME
    # Sanitize the table name.
    $sanitizedTableName = SanitizeName $tableName
    $tableNames += $sanitizedTableName
    $tableType = $table.TABLE_TYPE
    if ($tableType -eq "LINK") {
        Write-Host "Processing linked table: $tableName -> $sanitizedTableName"
    } else {
        Write-Host "Processing table: $tableName -> $sanitizedTableName"
    }

    # Filter the full columns list for the current table.
    $columns = $allColumns | Where-Object { $_.TABLE_NAME -eq $tableName }
    Write-Host "Found $($columns.Count) columns for table: $tableName"

    if ($columns.Count -eq 0) {
        Write-Host "Skipping table $tableName as no columns were found."
        continue
    }

    # Build the properties for the C# model using sanitized field names.
    $modelProperties = ""
    foreach ($col in $columns) {
        $originalColName = $col.COLUMN_NAME
        $sanitizedColName = SanitizeName $originalColName
        $dataType = $col.DATA_TYPE
        $csharpType = MapAccessToCSharpType $dataType
        $modelProperties += "        public $csharpType $sanitizedColName { get; set; }`r`n"
    }
    
    # Create the C# model class.
    $modelClass = @"
using System;
using System.Collections.Generic;

namespace YourNamespace.Models
{
    public class $sanitizedTableName
    {
$modelProperties
    }
}
"@
    $modelFile = Join-Path $modelsDir "$sanitizedTableName.cs"
    Write-Host "Writing model file: $modelFile"
    $modelClass | Out-File -Encoding utf8 $modelFile

    # Create a folder for the table's Razor pages and PageModel.
    $tablePageDir = Join-Path $pagesDir $sanitizedTableName
    if (!(Test-Path $tablePageDir)) { New-Item -ItemType Directory -Path $tablePageDir | Out-Null }

    # Generate the Razor Index page.
    $razorPage = @"
@page
@model YourNamespace.Pages.$sanitizedTableName.IndexModel
@{
    ViewData["Title"] = "$sanitizedTableName List";
}

<h1>$sanitizedTableName List</h1>

<table>
    <thead>
        <tr>
"@
    foreach ($col in $columns) {
        $originalColName = $col.COLUMN_NAME
        $sanitizedColName = SanitizeName $originalColName
        $razorPage += "            <th>$sanitizedColName</th>`r`n"
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
    foreach ($col in $columns) {
        $originalColName = $col.COLUMN_NAME
        $sanitizedColName = SanitizeName $originalColName
        $razorPage += "                <td>@item.$sanitizedColName</td>`r`n"
    }
    $razorPage += @"
                <td>
                    <a asp-page="./Edit" asp-route-id="@item.Id">Edit</a> |
                    <a asp-page="./Details" asp-route-id="@item.Id">Details</a> |
                    <a asp-page="./Delete" asp-route-id="@item.Id">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>

<a asp-page="./Create">Create New</a>
"@
    $razorFile = Join-Path $tablePageDir "Index.cshtml"
    Write-Host "Writing Razor page: $razorFile"
    $razorPage | Out-File -Encoding utf8 $razorFile

    # Generate the PageModel class for this table.
    $pageModelContent = @"
using Microsoft.AspNetCore.Mvc.RazorPages;
using YourNamespace.Models;
using System.Collections.Generic;

namespace YourNamespace.Pages.$sanitizedTableName
{
    public class IndexModel : PageModel
    {
        public List<$sanitizedTableName> Items { get; set; }

        public void OnGet()
        {
            // TODO: Retrieve data from SQL Server.
            Items = new List<$sanitizedTableName>();
        }
    }
}
"@
    $pageModelFile = Join-Path $tablePageDir "Index.cshtml.cs"
    Write-Host "Writing PageModel file: $pageModelFile"
    $pageModelContent | Out-File -Encoding utf8 $pageModelFile
}

$accessConnection.Close()

# Generate a Main Menu page that links to each table's Index page.
$mainMenuContent = @"
@page
@{
    ViewData["Title"] = "Main Menu";
}
<h1>Main Menu</h1>
<ul>
"@
foreach ($sanitizedName in $tableNames) {
    $mainMenuContent += "    <li><a asp-page='/$sanitizedName/Index'>$sanitizedName</a></li>`r`n"
}
$mainMenuContent += @"
</ul>
"@
$mainMenuFile = Join-Path $pagesDir "MainMenu.cshtml"
Write-Host "Writing Main Menu page: $mainMenuFile"
$mainMenuContent | Out-File -Encoding utf8 $mainMenuFile

Write-Host "Code generation complete. Files are saved in $outputDir"

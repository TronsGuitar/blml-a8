Add-Type -AssemblyName System.Windows.Forms

# Open file dialog to select the Access database
$openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
$openFileDialog.Filter = "Access Database (*.accdb)|*.accdb"
$openFileDialog.Title = "Select Access Database File"
if ($openFileDialog.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) {
    Write-Host "No file selected. Exiting."
    exit
}
$accessFilePath = $openFileDialog.FileName

# Save file dialog to choose the output SQL file
$saveFileDialog = New-Object System.Windows.Forms.SaveFileDialog
$saveFileDialog.Filter = "SQL Script (*.sql)|*.sql"
$saveFileDialog.Title = "Select Output SQL Script File"
if ($saveFileDialog.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) {
    Write-Host "No output file selected. Exiting."
    exit
}
$outputSqlPath = $saveFileDialog.FileName

# Connection string for the Access database
$accessConnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$accessFilePath;Persist Security Info=False;"

# Open the Access connection
$accessConnection = New-Object System.Data.OleDb.OleDbConnection($accessConnStr)
$accessConnection.Open()

# Retrieve list of tables (filtering for actual tables)
$tables = $accessConnection.GetSchema("Tables") | Where-Object { $_.TABLE_TYPE -eq "TABLE" }

# Open output file for writing the SQL script
$outputFile = [System.IO.StreamWriter]::new($outputSqlPath, $false)

foreach ($table in $tables) {
    $tableName = $table.TABLE_NAME
    $outputFile.WriteLine("-- SQL script for table [$tableName]")
    
    # Retrieve column details for the table
    $colRestrictions = @($null, $null, $tableName, $null)
    $columns = $accessConnection.GetSchema("Columns", $colRestrictions)
    
    # Build a basic CREATE TABLE statement.
    # In this example, all columns are defined as NVARCHAR(255). Adjust mapping as needed.
    $createTable = "CREATE TABLE [$tableName] ("
    $colDefs = @()
    foreach ($col in $columns) {
        $colName = $col.COLUMN_NAME
        $colDefs += "[$colName] NVARCHAR(255)"
    }
    $createTable += ($colDefs -join ", ") + ");"
    $outputFile.WriteLine($createTable)
    $outputFile.WriteLine("")  # blank line for clarity
    
    # Get all data from the table
    $accessQuery = "SELECT * FROM [$tableName]"
    $accessCommand = $accessConnection.CreateCommand()
    $accessCommand.CommandText = $accessQuery
    $dataAdapter = New-Object System.Data.OleDb.OleDbDataAdapter($accessCommand)
    $dataTable = New-Object System.Data.DataTable
    $dataAdapter.Fill($dataTable) | Out-Null

    # Generate INSERT statements for each row in the table
    foreach ($row in $dataTable.Rows) {
        $columnsList = @()
        $valuesList = @()
        foreach ($col in $dataTable.Columns) {
            $columnsList += "[$($col.ColumnName)]"
            $value = $row[$col.ColumnName]
            if ($value -eq $null -or $value -eq [DBNull]::Value) {
                $valuesList += "NULL"
            } else {
                # Escape single quotes in strings
                $escapedValue = $value.ToString().Replace("'", "''")
                $valuesList += "'$escapedValue'"
            }
        }
        $insertStmt = "INSERT INTO [$tableName] (" + ($columnsList -join ", ") + ") VALUES (" + ($valuesList -join ", ") + ");"
        $outputFile.WriteLine($insertStmt)
    }
    $outputFile.WriteLine("")  # extra line between tables
}

$outputFile.Close()
$accessConnection.Close()

Write-Host "SQL script generated successfully at $outputSqlPath"

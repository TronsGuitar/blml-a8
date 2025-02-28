# Set file paths and connection strings
$accessFilePath = "C:\Path\To\YourDatabase.accdb"
$accessConnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$accessFilePath;Persist Security Info=False;"

$sqlServer = "YourSQLServerInstance"
$sqlDatabase = "YourSQLDatabase"
$sqlConnStr = "Server=$sqlServer;Database=$sqlDatabase;Integrated Security=True;"

# Open the Access database connection
$accessConnection = New-Object System.Data.OleDb.OleDbConnection($accessConnStr)
$accessConnection.Open()

# Get the list of tables from the Access database
$tables = $accessConnection.GetSchema("Tables") | Where-Object { $_.TABLE_TYPE -eq "TABLE" }

foreach ($table in $tables) {
    $tableName = $table.TABLE_NAME
    Write-Host "Processing table: $tableName"

    # Retrieve all records from the current table in Access
    $accessQuery = "SELECT * FROM [$tableName]"
    $accessCommand = $accessConnection.CreateCommand()
    $accessCommand.CommandText = $accessQuery
    $dataAdapter = New-Object System.Data.OleDb.OleDbDataAdapter($accessCommand)
    $dataTable = New-Object System.Data.DataTable
    $dataAdapter.Fill($dataTable) | Out-Null

    # Open a connection to SQL Server
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection($sqlConnStr)
    $sqlConnection.Open()

    # Set up SQL Bulk Copy for fast transfer
    $bulkCopy = New-Object Data.SqlClient.SqlBulkCopy($sqlConnection)
    $bulkCopy.DestinationTableName = $tableName

    try {
        $bulkCopy.WriteToServer($dataTable)
        Write-Host "Table '$tableName' transferred successfully."
    }
    catch {
        Write-Host "Error transferring table '$tableName': $_"
    }
    finally {
        $sqlConnection.Close()
    }
}

$accessConnection.Close()

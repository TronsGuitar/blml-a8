# Create a DataTable to hold provider information
$dt = New-Object System.Data.DataTable

# Get the registered OLE DB providers using OleDbEnumerator
$oleDbEnumerator = [System.Data.OleDb.OleDbEnumerator]::GetRootEnumerator()

# Load the data into the DataTable
$dt.Load($oleDbEnumerator)

# Show the provider names, descriptions, and types
$dt | Format-Table SOURCES_NAME, SOURCES_DESCRIPTION, SOURCES_TYPE -AutoSize

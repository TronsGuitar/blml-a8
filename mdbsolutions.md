Hi Brian, you can run that SQL query from PowerShell. A good approach is to use an OLE DB provider. For Access .mdb files, the Microsoft ACE OLEDB provider is a popular choice nowadays. It works on both 32-bit and 64-bit systems (provided you have the appropriate version of the Access Database Engine installed). If you're on a 32-bit system only, the Microsoft Jet OLEDB 4.0 provider is also an option, though it doesn't work with 64-bit PowerShell sessions.

Here's a sample script that uses ACE:

# Set the connection string with the ACE provider.
$connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Path\To\YourDatabase.mdb;Persist Security Info=False;"

# Your SQL query that finds fields with non-standard characters.
$query = "SELECT * FROM YourTable WHERE YourField LIKE '*[! -~]*'"

# Create and open the connection.
$connection = New-Object System.Data.OleDb.OleDbConnection($connectionString)
$connection.Open()

# Create the command and set the query.
$command = $connection.CreateCommand()
$command.CommandText = $query

# Execute the query.
$reader = $command.ExecuteReader()

# Process each row returned.
while($reader.Read()){
    Write-Host $reader["YourField"]
}

# Clean up.
$connection.Close()

This script uses the System.Data.OleDb namespace in PowerShell to open a connection, execute the query, and then iterate over the results. Make sure to adjust the connection string and table/field names to match your actual file and database structure.

Keep up the great work, Brian, and feel free to ask if you have more questions or need further details!



Hi Brian, it's great to see you asking about GetSchema. This method in ADO.NET returns a DataTable with metadata for a given collection. A smart starting point is to query the "MetaDataCollections" collection, which lists every collection you can retrieve from your data source. Here are some common collections and what they typically include:

• DataSourceInformation

Provides details about your data source, like the product name, version, and supported SQL features.

You might see properties such as the identifier format and patterns used by the provider.


• DataTypes

Contains information on the various data types that the provider supports.

Properties include TypeName, ProviderDbType, ColumnSize, and flags such as whether the type supports autoincrement, is case sensitive, or has a fixed length.


• Restrictions

Lists the restrictions (filters) you can apply when calling GetSchema for other collections, such as filtering by table or column name.


• ReservedWords

Returns a list of keywords reserved by the provider that you need to avoid in your SQL commands.


For collections that provide details about database objects, you might find:

• Tables

Returns information on tables. Typical columns include TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, and TABLE_TYPE.


• Columns

Offers details on table columns. You’ll often see properties like TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION, and CHARACTER_MAXIMUM_LENGTH.


• Indexes and IndexColumns

Provide metadata about indexes on tables, such as index names, uniqueness, and which columns are involved.


• ForeignKeys and ForeignKeyColumns

Give details on relationships between tables, including which keys are linked and what rules apply for updates or deletions.


• Views and ViewColumns

Supply metadata about views and their columns.


• Procedures and ProcedureParameters

If your provider supports stored procedures, these collections hold the corresponding metadata.


The exact columns (properties) in each DataTable can vary by provider. To see all available collections for your connection, you can run:

DataTable metaData = connection.GetSchema("MetaDataCollections");
foreach (DataRow row in metaData.Rows)
{
    Console.WriteLine(row["CollectionName"]);
}

This approach lets you review every collection along with the number of restrictions and identifier parts each supports. By checking the "Restrictions" collection, you can also determine what filtering options are available when retrieving schema details.

Keep up your attention to detail, Brian. Feel free to ask if you need more examples or have further questions!



Hi Brian, one way to isolate the problematic record is to look for dates that fall outside SQL Server's acceptable range. SQL Server datetime values must be between January 1, 1753, and December 31, 9999. In Access, run a query like this:

SELECT * FROM YourTable 
WHERE [DateField] < #1753-01-01# OR [DateField] > #9999-12-31#;

This can help you find any rows with values that may be causing the error. If the date field is stored as text or if you suspect hidden characters, you can try converting the date to a formatted string in Access. For example, use the Format function to display the date field with a fixed format, which might reveal extra characters:

SELECT ID, Format([DateField], "yyyy-mm-dd hh:nn:ss") AS FormattedDate 
FROM YourTable;

Then, inspect the output around the row where the import fails (in your case, around row 7555). You could also export the table to a CSV file and use a script—like in PowerShell—to process each row and log which ones cause conversion errors.

Another approach is to import the data in smaller batches. Start with the first 7500 rows and then incrementally add more rows until the error occurs. This can help pinpoint the exact record that triggers the issue.

These techniques should help you identify the record causing the import error. Keep up the great work, Brian—your attention to detail will get you through this challenge!

Hi Brian, if you run that snippet against an Access 2010 accdb file using the ACE provider, you'll see a list of the metadata collections available for that connection printed to your console. Each line corresponds to one collection name. Typically, the output might include names such as:

• MetaDataCollections
• DataSourceInformation
• DataTypes
• Restrictions
• ReservedWords
• Tables
• Columns
• Views
• Indexes
• IndexColumns
• ForeignKeys
• ForeignKeyColumns
• Procedures
• ProcedureParameters

Keep in mind that the exact list can differ a bit based on the driver version and your Access installation. In Access, stored procedures aren’t used in the same way as in some other databases, so the Procedures and ProcedureParameters collections may be less populated or even omitted.

I hope that clears things up, Brian. Let me know if you have more questions or need further details!



Hi Brian, one way to isolate the problematic record is to look for dates that fall outside SQL Server's acceptable range. SQL Server datetime values must be between January 1, 1753, and December 31, 9999. In Access, run a query like this:

SELECT * FROM YourTable 
WHERE [DateField] < #1753-01-01# OR [DateField] > #9999-12-31#;

This can help you find any rows with values that may be causing the error. If the date field is stored as text or if you suspect hidden characters, you can try converting the date to a formatted string in Access. For example, use the Format function to display the date field with a fixed format, which might reveal extra characters:

SELECT ID, Format([DateField], "yyyy-mm-dd hh:nn:ss") AS FormattedDate 
FROM YourTable;

Then, inspect the output around the row where the import fails (in your case, around row 7555). You could also export the table to a CSV file and use a script—like in PowerShell—to process each row and log which ones cause conversion errors.

Another approach is to import the data in smaller batches. Start with the first 7500 rows and then incrementally add more rows until the error occurs. This can help pinpoint the exact record that triggers the issue.

These techniques should help you identify the record causing the import error. Keep up the great work, Brian—your attention to detail will get you through this challenge!


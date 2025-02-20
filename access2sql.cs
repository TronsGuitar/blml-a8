using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Collections.Generic;

class AccessToSqlServer
{
    static void Main()
    {
        Console.WriteLine("=== Access to SQL Server Converter ===");

        // Get Access file path from user
        Console.Write("Enter the full path of the Access (.accdb) database: ");
        string accessFilePath = Console.ReadLine().Trim();

        while (!File.Exists(accessFilePath))
        {
            Console.Write("File not found! Please enter a valid Access (.accdb) file path: ");
            accessFilePath = Console.ReadLine().Trim();
        }

        // Get output folder path for SQL and ASP.NET files
        Console.Write("Enter the output directory for SQL scripts and ASP.NET files: ");
        string outputFolder = Console.ReadLine().Trim();

        while (!Directory.Exists(outputFolder))
        {
            Console.Write("Directory not found! Please enter a valid output directory path: ");
            outputFolder = Console.ReadLine().Trim();
        }

        string outputSqlFile = Path.Combine(outputFolder, "output.sql");
        string outputAspNetFolder = Path.Combine(outputFolder, "AspNetForms");

        Directory.CreateDirectory(outputAspNetFolder); // Ensure directory exists

        Console.WriteLine("\nProcessing...\n");

        try
        {
            // Connect to Access Database
            string connString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={accessFilePath};Persist Security Info=False;";
            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                conn.Open();
                Console.WriteLine("Connected to Access Database.");

                // Extract Tables and Generate SQL
                string tableSql = ExtractTables(conn);
                
GrantReadPermission(conn);
                // Extract Queries
                string querySql = ExtractQueries(conn);

                // Extract Forms (Generate ASP.NET Files)
                ExtractForms(conn, outputAspNetFolder);

                // Save SQL Output
                File.WriteAllText(outputSqlFile, tableSql + "\n\n" + querySql);
                Console.WriteLine($"✅ SQL Script Generated: {outputSqlFile}");
                Console.WriteLine($"✅ ASP.NET Forms saved in: {outputAspNetFolder}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error: " + ex.Message);
        }

        Console.WriteLine("\nExtraction Complete. Press any key to exit.");
        Console.ReadKey();
    }
static void EnableReadAccessToMSysObjects(OleDbConnection conn)
{
    try
    {
        string sql = "UPDATE MSysObjects SET Flags = 0 WHERE Name = 'MSysObjects'";
        using (OleDbCommand cmd = new OleDbCommand(sql, conn))
        {
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("✅ Flags updated: Read access to MSysObjects enabled.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error updating MSysObjects flags: {ex.Message}");
    }
}
    //Set accessApp = CreateObject("Access.Application")
//accessApp.OpenCurrentDatabase "C:\path\to\your\database.accdb"
//accessApp.DoCmd.RunSQL "GRANT SELECT ON MSysObjects TO Admin;"
//accessApp.Quit
    //or open acccd and cnt g and run
    //CurrentProject.Connection.Execute "GRANT SELECT ON MSysObjects TO Admin;"
    static void RunVbaScript()
{
    try
    {
        string scriptPath = "C:\\path\\to\\EnablePermissions.vbs";
        System.Diagnostics.Process.Start("wscript.exe", scriptPath);
        Console.WriteLine("✅ VBA script executed to grant permissions.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error executing VBA script: {ex.Message}");
    }
}
    static string ExtractTables(OleDbConnection conn)
    {
        StringBuilder sqlBuilder = new StringBuilder();
        DataTable tables = conn.GetSchema("Tables");

        foreach (DataRow row in tables.Rows)
        {
            string tableName = row["TABLE_NAME"].ToString();
            if (!tableName.StartsWith("MSys")) // Ignore system tables
            {
                Console.WriteLine($"Extracting Table: {tableName}");
                sqlBuilder.Append($"CREATE TABLE [{tableName}] (\n");

                DataTable columns = conn.GetSchema("Columns", new string[] { null, null, tableName, null });

                foreach (DataRow col in columns.Rows)
                {
                    string colName = col["COLUMN_NAME"].ToString();
                    string dataType = col["DATA_TYPE"].ToString();
                    string sqlType = ConvertAccessTypeToSqlServer(dataType);

                    sqlBuilder.Append($"  [{colName}] {sqlType},\n");
                }

                sqlBuilder.Append(");\n\n");
            }
        }

        return sqlBuilder.ToString();
    }

    static string ConvertAccessTypeToSqlServer(string accessType)
    {
        // Map Access data types to SQL Server equivalents
        Dictionary<string, string> typeMapping = new Dictionary<string, string>()
        {
            {"3", "INT"},
            {"6", "FLOAT"},
            {"7", "DATETIME"},
            {"11", "BIT"},
            {"130", "NVARCHAR(255)"}
        };

        return typeMapping.ContainsKey(accessType) ? typeMapping[accessType] : "NVARCHAR(MAX)";
    }
    
static void GrantReadPermission(OleDbConnection conn)
{
    try
    {
        string sql = "GRANT SELECT ON MSysObjects TO Admin";
        using (OleDbCommand cmd = new OleDbCommand(sql, conn))
        {
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("✅ Read permission granted on MSysObjects.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Unable to grant permission: {ex.Message}");
    }
}

    
static string ExtractQueries(OleDbConnection conn)
{
    StringBuilder sqlBuilder = new StringBuilder();

    // Use MSysObjects to get query names (Type = 5 means saved queries)
    string query = "SELECT Name FROM MSysObjects WHERE Type = 5 AND Name NOT LIKE 'MSys%'";

    using (OleDbCommand cmd = new OleDbCommand(query, conn))
    using (OleDbDataReader reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            string queryName = reader["Name"].ToString();
            Console.WriteLine($"Extracting Query: {queryName}");

            // Retrieve SQL for the query (Try using Direct Query Execution)
            string sqlQueryText = GetQueryDefinition(conn, queryName);
            
            if (!string.IsNullOrEmpty(sqlQueryText))
            {
                sqlBuilder.Append($"-- Query: {queryName}\n");
                sqlBuilder.Append(sqlQueryText + ";\n\n");
            }
        }
    }

    return sqlBuilder.ToString();
}

    static string GetQueryDefinition(OleDbConnection conn, string queryName)
{
    string querySql = "";

    try
    {
        // Open connection using DAO (Database Access Objects)
        Type accessType = Type.GetTypeFromProgID("DAO.DBEngine.120");
        dynamic dbEngine = Activator.CreateInstance(accessType);
        dynamic db = dbEngine.OpenDatabase(conn.DataSource);
        dynamic queryDef = db.QueryDefs[queryName];

        querySql = queryDef.SQL; // Get SQL definition of the query

        // Cleanup
        queryDef.Close();
        db.Close();
        dbEngine = null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error retrieving SQL for query {queryName}: {ex.Message}");
    }

    return querySql;
}


static void ExtractForms(OleDbConnection conn, string outputAspNetFolder)
{
    StringBuilder formNames = new StringBuilder();
    string query = "SELECT Name FROM MSysObjects WHERE Type = -32768 AND Name NOT LIKE 'MSys%'";

    try
    {
        using (OleDbCommand cmd = new OleDbCommand(query, conn))
        using (OleDbDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                string formName = reader["Name"].ToString();
                Console.WriteLine($"Extracting Form: {formName}");

                // Generate ASP.NET Razor Form
                string cshtmlContent = GenerateAspNetForm(formName);
                string csFile = Path.Combine(outputAspNetFolder, formName + ".cshtml");
                File.WriteAllText(csFile, cshtmlContent);

                formNames.AppendLine(formName);
            }
        }

        Console.WriteLine($"✅ Forms Extracted:\n{formNames.ToString()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error extracting forms: {ex.Message}");
    }
}


    static string GenerateAspNetForm(string formName)
    {
        return @$"
@page
@model {formName}Model

<h2>{formName}</h2>

<form method='post'>
    <div class='form-group'>
        <label for='field1'>Field 1:</label>
        <input type='text' class='form-control' id='field1' name='field1'>
    </div>

    <button type='submit' class='btn btn-primary'>Submit</button>
</form>
";
    }
}

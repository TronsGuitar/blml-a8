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

        // Skip system tables and forms (Type = -32768)
        if (!tableName.StartsWith("MSys") && !IsForm(conn, tableName))
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
static bool IsForm(OleDbConnection conn, string objectName)
{
    bool isForm = false;
    string query = "SELECT COUNT(*) FROM MSysObjects WHERE Type = -32768 AND Name = ?";

    using (OleDbCommand cmd = new OleDbCommand(query, conn))
    {
        cmd.Parameters.AddWithValue("?", objectName);
        int count = Convert.ToInt32(cmd.ExecuteScalar());
        isForm = count > 0;
    }

    return isForm;
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
    string query = "SELECT Name FROM MSysObjects WHERE Type = 5 AND Name NOT LIKE 'MSys%'";

    try
    {
        using (OleDbCommand cmd = new OleDbCommand(query, conn))
        using (OleDbDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                string queryName = reader["Name"].ToString();
                Console.WriteLine($"Extracting Query: {queryName}");

                // Get SQL Definition of the Query
                string querySql = GetQueryDefinition(conn, queryName);
                
                if (!string.IsNullOrEmpty(querySql))
                {
                    // Convert Access SQL to SQL Server Compatible SQL
                    string convertedSql = ConvertAccessQueryToSqlServer(queryName, querySql);

                    sqlBuilder.Append($"-- Query: {queryName}\n");
                    sqlBuilder.Append(convertedSql + ";\n\n");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error extracting queries: {ex.Message}");
    }

    return sqlBuilder.ToString();
}


 static string GetQueryDefinition(OleDbConnection conn, string queryName)
{
    string querySql = "";

    try
    {
        // Open DAO connection
        Type accessType = Type.GetTypeFromProgID("DAO.DBEngine.120");
        dynamic dbEngine = Activator.CreateInstance(accessType);
        dynamic db = dbEngine.OpenDatabase(conn.DataSource);
        dynamic queryDef = db.QueryDefs[queryName];

        querySql = queryDef.SQL; // Get SQL definition

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
static string ConvertAccessQueryToSqlServer(string queryName, string accessSql)
{
    // Remove Access-specific brackets and replace with SQL Server compatible format
    string convertedSql = accessSql.Replace("[", "").Replace("]", "").Replace(";", "");

    // Check if it's a SELECT query (create a view) or an action query (create a stored procedure)
    if (convertedSql.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
    {
        return $"CREATE VIEW {queryName} AS \n{convertedSql}";
    }
    else
    {
        return $"CREATE PROCEDURE {queryName} AS \nBEGIN\n{convertedSql}\nEND";
    }
}


static void ExtractForms(OleDbConnection conn, string outputAspNetFolder)
{
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

                // Get Form Fields and VBA References
                List<string> formFields = GetFormFields(conn, formName);
                Dictionary<string, string> vbaFunctions = GetVBAFunctions(conn, formName);

                // Generate ASP.NET Razor Page with Fields
                string cshtmlContent = GenerateAspNetForm(formName, formFields, vbaFunctions);
                string csFile = Path.Combine(outputAspNetFolder, formName + ".cshtml");
                File.WriteAllText(csFile, cshtmlContent);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error extracting forms: {ex.Message}");
    }
}

static List<string> GetFormFields(OleDbConnection conn, string formName)
{
    List<string> fields = new List<string>();
    
    string query = $"SELECT Name FROM MSysObjects WHERE ParentId IN (SELECT Id FROM MSysObjects WHERE Name = ?)";

    using (OleDbCommand cmd = new OleDbCommand(query, conn))
    {
        cmd.Parameters.AddWithValue("?", formName);
        using (OleDbDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                fields.Add(reader["Name"].ToString());
            }
        }
    }

    return fields;
}

static Dictionary<string, string> GetVBAFunctions(OleDbConnection conn, string formName)
{
    Dictionary<string, string> vbaReferences = new Dictionary<string, string>();

    string query = $"SELECT Name, EventProc FROM MSysObjects WHERE ParentId = (SELECT Id FROM MSysObjects WHERE Name = '{formName}')";

    using (OleDbCommand cmd = new OleDbCommand(query, conn))
    using (OleDbDataReader reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            string controlName = reader["Name"].ToString();
            string vbaFunction = reader["EventProc"] != DBNull.Value ? reader["EventProc"].ToString() : "None";

            vbaReferences[controlName] = vbaFunction;
        }
    }

    return vbaReferences;
}


static string GenerateAspNetForm(string formName, List<string> fields, Dictionary<string, string> vbaFunctions)
{
    StringBuilder formHtml = new StringBuilder();

    formHtml.AppendLine($"@page\n@model {formName}Model\n\n<h2>{formName}</h2>\n");
    formHtml.AppendLine("<form method='post'>");

    foreach (string field in fields)
    {
        string vbaFunction = vbaFunctions.ContainsKey(field) ? vbaFunctions[field] : "None";

        formHtml.AppendLine($@"
        <div class='form-group'>
            <label for='{field}'>{field}</label>
            <input type='text' class='form-control' id='{field}' name='{field}' onblur='CallVBA(\"{vbaFunction}\")'>
        </div>");
    }

    formHtml.AppendLine("<button type='submit' class='btn btn-primary'>Submit</button>");
    formHtml.AppendLine("</form>");

    return formHtml.ToString();
}


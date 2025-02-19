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

    static string ExtractQueries(OleDbConnection conn)
    {
        StringBuilder sqlBuilder = new StringBuilder();
        string query = "SELECT Name, SQL FROM MSysQueries";

        using (OleDbCommand cmd = new OleDbCommand(query, conn))
        using (OleDbDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                string queryName = reader["Name"].ToString();
                string querySql = reader["SQL"].ToString();
                Console.WriteLine($"Extracting Query: {queryName}");

                // Convert Access SQL to SQL Server compatible syntax
                querySql = querySql.Replace("[", "").Replace("]", "").Replace(";", "");

                sqlBuilder.Append($"-- Query: {queryName}\n");
                sqlBuilder.Append(querySql + ";\n\n");
            }
        }

        return sqlBuilder.ToString();
    }

    static void ExtractForms(OleDbConnection conn, string outputAspNetFolder)
    {
        DataTable forms = conn.GetSchema("Tables");

        foreach (DataRow row in forms.Rows)
        {
            string formName = row["TABLE_NAME"].ToString();
            if (formName.StartsWith("Form_")) // Assume forms are named as "Form_*"
            {
                Console.WriteLine($"Extracting Form: {formName}");

                string cshtmlContent = GenerateAspNetForm(formName);
                string csFile = Path.Combine(outputAspNetFolder, formName + ".cshtml");
                File.WriteAllText(csFile, cshtmlContent);
            }
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

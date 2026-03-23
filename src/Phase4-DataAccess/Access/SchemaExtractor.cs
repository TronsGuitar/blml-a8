using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using BLML.Phase4DataAccess.Models;

namespace BLML.Phase4DataAccess.Access
{
    public class SchemaExtractor
    {
        private readonly string _pythonPath;
        private readonly string _scriptPath;

        public SchemaExtractor(string scriptPath, string pythonPath = "python")
        {
            _scriptPath = scriptPath;
            _pythonPath = pythonPath;
        }

        public List<TableMetadata> ExtractFullSchema(string dbPath)
        {
            var output = RunPythonScript(dbPath);
            if (string.IsNullOrEmpty(output)) return new List<TableMetadata>();

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var rawTables = JsonSerializer.Deserialize<List<RawTableSchema>>(output, options);
                
                var tables = new List<TableMetadata>();
                if (rawTables == null) return tables;

                foreach (var raw in rawTables)
                {
                    var table = new TableMetadata
                    {
                        Name = raw.Name,
                        PrimaryKeyColumns = raw.PrimaryKeys ?? new List<string>()
                    };

                    foreach (var col in raw.Columns)
                    {
                        table.Columns.Add(new ColumnMetadata
                        {
                            Name = col.Name,
                            DataType = col.Type,
                            IsNullable = col.Nullable,
                            MaxLength = col.Length
                        });
                    }
                    tables.Add(table);
                }
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing schema JSON: {ex.Message}");
                return new List<TableMetadata>();
            }
        }

        private string RunPythonScript(string dbPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = $"\"{_scriptPath}\" \"{dbPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return string.Empty;

            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Python Error: {error}");
            }

            return result;
        }

        private class RawTableSchema
        {
            public string Name { get; set; } = string.Empty;
            public List<RawColumnSchema> Columns { get; set; } = new();
            public List<string> PrimaryKeys { get; set; } = new();
        }

        private class RawColumnSchema
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public bool Nullable { get; set; }
            public int Length { get; set; }
        }
    }
}

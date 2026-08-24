using System.Text;
using BLML.Phase5ASPtoAngular.Analysis;

namespace BLML.Phase5ASPtoAngular.Backend.ApiGenerator
{
    public class ServiceMethodSpec
    {
        public string MethodName { get; set; } = string.Empty;
        public DatabaseCallSite Site { get; set; } = null!;
        public IReadOnlyList<string> ResultFields { get; set; } = Array.Empty<string>();
        /// <summary>True for INSERT/UPDATE/DELETE (returns affected-row count); false for SELECT (returns a list).</summary>
        public bool IsMutation { get; set; }
    }

    /// <summary>
    /// Turns a <see cref="DatabaseCallSite"/> into an ADO.NET service method that is
    /// ALWAYS parameterized - regardless of whether the original ASP code built its
    /// SQL by safe literal concatenation or by splicing raw request/session state into
    /// the string (<see cref="DatabaseCallSite.BuiltByUnsafeConcatenation"/>). The
    /// point of running the original page through DatabaseCallAnalyzer at all is that
    /// downstream generation never has to make that judgment call per-site: every `?`
    /// placeholder becomes a bound SqlParameter, full stop.
    /// </summary>
    public class ServiceGenerator
    {
        public string GenerateServiceClass(string serviceName, string dtoName, IEnumerable<ServiceMethodSpec> methods, string @namespace = "BLML.Api.Services")
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.Data.SqlClient;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine($"using BLML.Api.Dtos;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {serviceName}");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly string _connectionString;");
            sb.AppendLine();
            sb.AppendLine($"        public {serviceName}(string connectionString)");
            sb.AppendLine("        {");
            sb.AppendLine("            _connectionString = connectionString;");
            sb.AppendLine("        }");

            foreach (var method in methods)
            {
                sb.AppendLine();
                AppendMethod(sb, method, dtoName);
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private void AppendMethod(StringBuilder sb, ServiceMethodSpec method, string dtoName)
        {
            var (parameterizedSql, paramNames) = Parameterize(method.Site);
            var parameterList = string.Join(", ", paramNames.Select(p => $"object {p}"));

            if (method.IsMutation)
            {
                sb.AppendLine($"        public async Task<int> {method.MethodName}Async({parameterList})");
                sb.AppendLine("        {");
                sb.AppendLine("            using var connection = new SqlConnection(_connectionString);");
                sb.AppendLine("            await connection.OpenAsync();");
                sb.AppendLine($"            using var command = new SqlCommand(\"{Escape(parameterizedSql)}\", connection);");
                foreach (var p in paramNames)
                {
                    sb.AppendLine($"            command.Parameters.AddWithValue(\"@{p}\", {p});");
                }
                sb.AppendLine("            return await command.ExecuteNonQueryAsync();");
                sb.AppendLine("        }");
                return;
            }

            sb.AppendLine($"        public async Task<List<{dtoName}>> {method.MethodName}Async({parameterList})");
            sb.AppendLine("        {");
            sb.AppendLine($"            var results = new List<{dtoName}>();");
            sb.AppendLine("            using var connection = new SqlConnection(_connectionString);");
            sb.AppendLine("            await connection.OpenAsync();");
            sb.AppendLine($"            using var command = new SqlCommand(\"{Escape(parameterizedSql)}\", connection);");
            foreach (var p in paramNames)
            {
                sb.AppendLine($"            command.Parameters.AddWithValue(\"@{p}\", {p});");
            }
            sb.AppendLine("            using var reader = await command.ExecuteReaderAsync();");
            sb.AppendLine("            while (await reader.ReadAsync())");
            sb.AppendLine("            {");
            sb.AppendLine($"                results.Add(new {dtoName}");
            sb.AppendLine("                {");
            foreach (var field in method.ResultFields)
            {
                var prop = char.ToUpperInvariant(field[0]) + field[1..];
                sb.AppendLine($"                    {prop} = reader[\"{field}\"],");
            }
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("            return results;");
            sb.AppendLine("        }");
        }

        /// <summary>Replaces each `?` in the reconstructed SQL with a distinct `@pN` and returns the matching bind-parameter names.</summary>
        private static (string sql, List<string> paramNames) Parameterize(DatabaseCallSite site)
        {
            var paramNames = new List<string>();
            var sb = new StringBuilder();
            int placeholderIndex = 0;

            foreach (var ch in site.SqlText)
            {
                if (ch == '?')
                {
                    var name = SanitizeParamName(
                        placeholderIndex < site.ConcatenatedParameterExpressions.Count
                            ? site.ConcatenatedParameterExpressions[placeholderIndex]
                            : $"p{placeholderIndex}");
                    paramNames.Add(name);
                    sb.Append('@').Append(name);
                    placeholderIndex++;
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return (sb.ToString(), paramNames);
        }

        private static string SanitizeParamName(string raw)
        {
            var cleaned = new string(raw.Where(char.IsLetterOrDigit).ToArray());
            return string.IsNullOrEmpty(cleaned) ? "value" : char.IsDigit(cleaned[0]) ? "p" + cleaned : cleaned;
        }

        private static string Escape(string sql) => sql.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

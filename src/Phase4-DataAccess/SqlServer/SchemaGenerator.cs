using BLML.Phase4DataAccess.Models;
using System.Text;

namespace BLML.Phase4DataAccess.SqlServer
{
    public class SchemaGenerator
    {
        public string GenerateCreateScript(TableMetadata table)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [{table.Name}] (");
            
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var col = table.Columns[i];
                var sqlDataType = MapToSqlType(col.DataType, col.MaxLength);
                var nullability = col.IsNullable ? "NULL" : "NOT NULL";
                var comma = (i < table.Columns.Count - 1 || table.PrimaryKeyColumns.Count > 0) ? "," : "";
                
                sb.AppendLine($"    [{col.Name}] {sqlDataType} {nullability}{comma}");
            }

            if (table.PrimaryKeyColumns.Count > 0)
            {
                var keys = string.Join(", ", table.PrimaryKeyColumns.ConvertAll(k => $"[{k}]"));
                sb.AppendLine($"    CONSTRAINT PK_{table.Name} PRIMARY KEY ({keys})");
            }

            sb.AppendLine(");");
            return sb.ToString();
        }

        private static string MapToSqlType(string genericType, int? length)
        {
            return genericType.ToLowerInvariant() switch
            {
                "int" or "integer" or "long" => "INT",
                "text" or "string" or "varchar" => length.HasValue ? $"NVARCHAR({length})" : "NVARCHAR(MAX)",
                "memo" => "NVARCHAR(MAX)",
                "datetime" or "date" => "DATETIME2",
                "boolean" or "bit" or "yes/no" => "BIT",
                "double" or "float" => "FLOAT",
                "decimal" or "currency" or "money" => "DECIMAL(18,2)",
                "binary" or "ole object" => "VARBINARY(MAX)",
                _ => "NVARCHAR(MAX)"
            };
        }
    }
}

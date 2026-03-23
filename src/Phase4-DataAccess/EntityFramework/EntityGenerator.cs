using BLML.Phase4DataAccess.Models;
using System.Text;

namespace BLML.Phase4DataAccess.EntityFramework
{
    public class EntityGenerator
    {
        public string GenerateEntity(TableMetadata table, string @namespace = "BLML.Models")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {table.Name}");
            sb.AppendLine("    {");

            foreach (var col in table.Columns)
            {
                var type = MapToCSharpType(col.DataType);
                if (col.IsNullable && type != "string" && type != "byte[]")
                {
                    type += "?";
                }
                sb.AppendLine($"        public {type} {col.Name} {{ get; set; }}");
            }

            foreach (var rel in table.Relationships)
            {
                sb.AppendLine($"        public virtual {rel.ToTable} {rel.NavigationPropertyName} {{ get; set; }} = null!;");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string MapToCSharpType(string dbType)
        {
            return dbType.ToLowerInvariant() switch
            {
                "int" or "integer" or "long" => "int",
                "text" or "varchar" or "string" or "memo" => "string",
                "datetime" or "date" or "timestamp" => "DateTime",
                "bit" or "boolean" or "yes/no" => "bool",
                "double" or "float" or "numeric" => "double",
                "currency" or "decimal" or "money" => "decimal",
                "binary" or "ole object" or "image" => "byte[]",
                _ => "string"
            };
        }
    }
}

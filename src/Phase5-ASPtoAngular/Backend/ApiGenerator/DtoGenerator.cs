using System.Text;

namespace BLML.Phase5ASPtoAngular.Backend.ApiGenerator
{
    /// <summary>
    /// Classic ASP has no schema to read a shape from - a page just does
    /// `rs("FieldName")` and trusts whatever the query returned. DtoGenerator turns
    /// the field names DatabaseCallAnalyzer.FindFieldReferences found into a C# record,
    /// typed `object?` with an explicit TODO rather than guessing a type, so nothing
    /// downstream silently assumes a wrong type.
    /// </summary>
    public class DtoGenerator
    {
        public string GenerateDto(string dtoName, IReadOnlyList<string> fieldNames, string @namespace = "BLML.Api.Dtos")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {dtoName}");
            sb.AppendLine("    {");
            foreach (var field in fieldNames)
            {
                var propertyName = ToPascalCase(field);
                sb.AppendLine($"        // TODO: verify type - inferred from ASP `rs(\"{field}\")` usage, no schema was available.");
                sb.AppendLine($"        public object? {propertyName} {{ get; set; }}");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string ToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToUpperInvariant(name[0]) + name[1..];
        }
    }
}

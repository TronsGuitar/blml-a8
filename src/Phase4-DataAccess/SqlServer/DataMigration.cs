using System.Text;

namespace BLML.Phase4DataAccess.SqlServer
{
    public class DataMigration
    {
        public string GenerateBulkCopyCode(string tableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using Microsoft.Data.SqlClient;");
            sb.AppendLine();
            sb.AppendLine($"public void Migrate{tableName}(string sourceConn, string destConn)");
            sb.AppendLine("{");
            sb.AppendLine($"    using var source = new SqlConnection(sourceConn);");
            sb.AppendLine($"    using var dest = new SqlConnection(destConn);");
            sb.AppendLine("    source.Open();");
            sb.AppendLine("    dest.Open();");
            sb.AppendLine();
            sb.AppendLine($"    var cmd = new SqlCommand(\"SELECT * FROM {tableName}\", source);");
            sb.AppendLine("    using var reader = cmd.ExecuteReader();");
            sb.AppendLine();
            sb.AppendLine($"    using var bulkCopy = new SqlBulkCopy(dest);");
            sb.AppendLine($"    bulkCopy.DestinationTableName = \"{tableName}\";");
            sb.AppendLine("    bulkCopy.WriteToServer(reader);");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}

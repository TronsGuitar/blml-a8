using System.Text;
using BLML.Phase4DataAccess.Models;
using BLML.Phase4DataAccess.SqlServer;

namespace BLML.Phase5ASPtoAngular.Database
{
    /// <summary>
    /// Generates the SQL Server schema and Access-to-SQL-Server data migration scripts
    /// for tables discovered by DatabaseCallAnalyzer, delegating to Phase 4's
    /// SchemaGenerator/DataMigration (already implemented and tested) rather than
    /// duplicating T-SQL generation for a second migration path.
    /// </summary>
    public class MigrationScripts
    {
        private readonly SchemaGenerator _schemaGenerator = new();
        private readonly DataMigration _dataMigration = new();

        public string GenerateCreateScripts(IEnumerable<TableMetadata> tables)
        {
            var sb = new StringBuilder();
            foreach (var table in tables)
            {
                sb.AppendLine(_schemaGenerator.GenerateCreateScript(table));
            }
            return sb.ToString();
        }

        public string GenerateBulkCopyScripts(IEnumerable<TableMetadata> tables)
        {
            var sb = new StringBuilder();
            foreach (var table in tables)
            {
                sb.AppendLine(_dataMigration.GenerateBulkCopyCode(table.Name));
            }
            return sb.ToString();
        }
    }
}

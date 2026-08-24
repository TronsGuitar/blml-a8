using System.Text;
using BLML.Phase4DataAccess.EntityFramework;
using BLML.Phase4DataAccess.Models;

namespace BLML.Phase5ASPtoAngular.Database
{
    /// <summary>
    /// Bridges Phase 5's ASP-derived table/field data (DatabaseCallAnalyzer's
    /// discovered tables and rs("Field") references - the only schema information
    /// available, since classic ASP/Access has none to read directly) into Phase 4's
    /// already-implemented EF Core generators (EntityGenerator, DbContextGenerator)
    /// rather than reimplementing entity/DbContext generation a second time.
    /// </summary>
    public class EFCoreGenerator
    {
        private readonly EntityGenerator _entityGenerator = new();
        private readonly DbContextGenerator _dbContextGenerator = new();

        /// <summary>Field types are unknown (ASP has no schema) - every column defaults to nullable string, same "no schema" stance DtoGenerator takes on the API side.</summary>
        public TableMetadata BuildTableMetadata(string tableName, IReadOnlyList<string> fieldNames, IReadOnlyList<string>? primaryKeyColumns = null)
        {
            var table = new TableMetadata { Name = tableName };
            table.Columns.AddRange(fieldNames.Select(f => new ColumnMetadata { Name = f, DataType = "string", IsNullable = true }));
            if (primaryKeyColumns != null) table.PrimaryKeyColumns.AddRange(primaryKeyColumns);
            return table;
        }

        public string GenerateEntities(IEnumerable<TableMetadata> tables, string @namespace = "BLML.Api.Models")
        {
            var sb = new StringBuilder();
            foreach (var table in tables)
            {
                sb.AppendLine(_entityGenerator.GenerateEntity(table, @namespace));
            }
            return sb.ToString();
        }

        public string GenerateDbContext(string contextClassName, List<TableMetadata> tables, string @namespace = "BLML.Api.Data") =>
            _dbContextGenerator.GenerateDbContext(contextClassName, tables, @namespace);
    }
}

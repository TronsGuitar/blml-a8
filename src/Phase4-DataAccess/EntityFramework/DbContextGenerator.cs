using BLML.Phase4DataAccess.Models;
using System.Collections.Generic;
using System.Text;

namespace BLML.Phase4DataAccess.EntityFramework
{
    public class DbContextGenerator
    {
        public string GenerateDbContext(string className, List<TableMetadata> tables, string @namespace = "BLML.Data")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className} : DbContext");
            sb.AppendLine("    {");
            sb.AppendLine($"        public {className}(DbContextOptions<{className}> options) : base(options) {{ }}");
            sb.AppendLine();

            foreach (var table in tables)
            {
                sb.AppendLine($"        public DbSet<{table.Name}> {table.Name}s {{ get; set; }} = null!;");
            }

            sb.AppendLine();
            sb.AppendLine("        protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sb.AppendLine("        {");
            foreach (var table in tables)
            {
                sb.AppendLine($"            modelBuilder.Entity<{table.Name}>(entity =>");
                sb.AppendLine("            {");
                sb.AppendLine($"                entity.ToTable(\"{table.Name}\");");
                if (table.PrimaryKeyColumns.Count > 0)
                {
                    var keys = string.Join(", ", table.PrimaryKeyColumns.ConvertAll(k => $"e.{k}"));
                    sb.AppendLine($"                entity.HasKey(e => new {{ {keys} }});");
                }
                sb.AppendLine("            });");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}

using System;
using System.Text;

namespace BLML.Phase4DataAccess.EntityFramework
{
    public class DbContextGenerator
    {
        public string GenerateDbContext(string className, string[] dbSets)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine($"public class {className} : DbContext");
            sb.AppendLine("{");
            sb.AppendLine($"    public {className}(DbContextOptions<{className}> options) : base(options) {{ }}");
            foreach(var set in dbSets)
            {
                sb.AppendLine($"    public DbSet<{set}> {set}s {{ get; set; }}");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}

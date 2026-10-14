using System.Text;
using BLML.Phase4DataAccess.Models;

namespace BLML.Phase5ASPtoAngular.Database
{
    /// <summary>
    /// Generates a repository interface/implementation pair over an EF Core DbContext
    /// for a discovered table - the one piece of Phase 5's database story Phase 4
    /// doesn't already provide (Phase 4 stops at entities/DbContext/schema scripts).
    /// This gives the migrated app a path off ServiceGenerator's direct ADO.NET
    /// translation (which intentionally preserves the original SQL/behavior exactly)
    /// toward ordinary EF Core data access, per ProjectPlan.md item 77.
    /// </summary>
    public class RepositoryGenerator
    {
        public string GenerateRepositoryInterface(TableMetadata table, string @namespace = "BLML.Api.Data")
        {
            var idType = table.PrimaryKeyColumns.Count == 1 ? "int" : "object";
            var sb = new StringBuilder();
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine($"using BLML.Api.Models;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public interface I{table.Name}Repository");
            sb.AppendLine("    {");
            sb.AppendLine($"        Task<List<{table.Name}>> GetAllAsync();");
            sb.AppendLine($"        Task<{table.Name}?> GetByIdAsync({idType} id);");
            sb.AppendLine($"        Task AddAsync({table.Name} entity);");
            sb.AppendLine($"        Task UpdateAsync({table.Name} entity);");
            sb.AppendLine($"        Task DeleteAsync({idType} id);");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string GenerateRepositoryImplementation(TableMetadata table, string dbContextClassName, string @namespace = "BLML.Api.Data")
        {
            var idColumn = table.PrimaryKeyColumns.FirstOrDefault() ?? "Id";
            var idType = table.PrimaryKeyColumns.Count == 1 ? "int" : "object";
            var dbSetName = table.Name + "s";

            var sb = new StringBuilder();
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine($"using BLML.Api.Models;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {table.Name}Repository : I{table.Name}Repository");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly {dbContextClassName} _context;");
            sb.AppendLine();
            sb.AppendLine($"        public {table.Name}Repository({dbContextClassName} context)");
            sb.AppendLine("        {");
            sb.AppendLine("            _context = context;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public async Task<List<{table.Name}>> GetAllAsync() => await _context.{dbSetName}.ToListAsync();");
            sb.AppendLine();
            sb.AppendLine($"        public async Task<{table.Name}?> GetByIdAsync({idType} id) =>");
            sb.AppendLine($"            await _context.{dbSetName}.FirstOrDefaultAsync(e => e.{idColumn}.Equals(id));");
            sb.AppendLine();
            sb.AppendLine($"        public async Task AddAsync({table.Name} entity)");
            sb.AppendLine("        {");
            sb.AppendLine($"            _context.{dbSetName}.Add(entity);");
            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public async Task UpdateAsync({table.Name} entity)");
            sb.AppendLine("        {");
            sb.AppendLine($"            _context.{dbSetName}.Update(entity);");
            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public async Task DeleteAsync({idType} id)");
            sb.AppendLine("        {");
            sb.AppendLine("            var entity = await GetByIdAsync(id);");
            sb.AppendLine("            if (entity is null) return;");
            sb.AppendLine($"            _context.{dbSetName}.Remove(entity);");
            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}

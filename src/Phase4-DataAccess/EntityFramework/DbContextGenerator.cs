using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLML.Phase4DataAccess.EntityFramework
{
    public class DbContextGenerator
    {
        public string GenerateDbContext(DbContextGenerationOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.Namespace))
            {
                throw new ArgumentException("A namespace is required.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.ContextName))
            {
                throw new ArgumentException("A DbContext name is required.", nameof(options));
            }

            var entityNamespace = string.IsNullOrWhiteSpace(options.EntityNamespace)
                ? options.Namespace
                : options.EntityNamespace;
            var tables = options.Tables ?? Array.Empty<DbContextTableDefinition>();
            var builder = new StringBuilder();

            builder.AppendLine("using System;");
            builder.AppendLine("using System.IO;");
            builder.AppendLine("using Microsoft.EntityFrameworkCore;");
            builder.AppendLine("using Microsoft.Extensions.Configuration;");

            if (!string.Equals(entityNamespace, options.Namespace, StringComparison.Ordinal))
            {
                builder.AppendLine($"using {entityNamespace};");
            }

            builder.AppendLine();
            builder.AppendLine($"namespace {options.Namespace}");
            builder.AppendLine("{");
            builder.AppendLine($"    public class {options.ContextName} : DbContext");
            builder.AppendLine("    {");
            builder.AppendLine("        private readonly IConfiguration? _configuration;");
            builder.AppendLine();
            builder.AppendLine($"        public {options.ContextName}(DbContextOptions<{options.ContextName}> options, IConfiguration? configuration = null)");
            builder.AppendLine("            : base(options)");
            builder.AppendLine("        {");
            builder.AppendLine("            _configuration = configuration;");
            builder.AppendLine("        }");

            if (tables.Count > 0)
            {
                builder.AppendLine();

                foreach (var table in tables)
                {
                    ValidateTable(table);
                    builder.AppendLine($"        public DbSet<{GetEntityName(table)}> {GetDbSetName(table)} => Set<{GetEntityName(table)}>();");
                }
            }

            builder.AppendLine();
            builder.AppendLine("        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (optionsBuilder.IsConfigured)");
            builder.AppendLine("            {");
            builder.AppendLine("                return;");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            var configuration = _configuration ?? new ConfigurationBuilder()");
            builder.AppendLine("                .SetBasePath(Directory.GetCurrentDirectory())");
            builder.AppendLine("                .AddJsonFile(\"appsettings.json\", optional: true)");
            builder.AppendLine("                .AddJsonFile($\"appsettings.{Environment.GetEnvironmentVariable(\"ASPNETCORE_ENVIRONMENT\")}.json\", optional: true)");
            builder.AppendLine("                .AddEnvironmentVariables()");
            builder.AppendLine("                .Build();");
            builder.AppendLine();
            builder.AppendLine($"            var connectionString = Environment.GetEnvironmentVariable(\"{EscapeCSharpString(options.EnvironmentVariableName)}\")");
            builder.AppendLine($"                ?? configuration.GetConnectionString(\"{EscapeCSharpString(options.ConnectionStringName)}\")");
            builder.AppendLine($"                ?? configuration[\"ConnectionStrings:{EscapeCSharpString(options.ConnectionStringName)}\"];");
            builder.AppendLine();
            builder.AppendLine("            if (string.IsNullOrWhiteSpace(connectionString))");
            builder.AppendLine("            {");
            builder.AppendLine($"                throw new InvalidOperationException(\"A connection string named '{EscapeCSharpString(options.ConnectionStringName)}' or environment variable '{EscapeCSharpString(options.EnvironmentVariableName)}' is required.\");");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            optionsBuilder.UseSqlServer(connectionString);");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        protected override void OnModelCreating(ModelBuilder modelBuilder)");
            builder.AppendLine("        {");
            builder.AppendLine("            base.OnModelCreating(modelBuilder);");

            if (tables.Count > 0)
            {
                builder.AppendLine();

                foreach (var table in tables)
                {
                    AppendTableMapping(builder, table);
                    builder.AppendLine();
                }
            }

            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        public RepositoryScaffolding GenerateRepositoryScaffolding(RepositoryGenerationOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.Namespace))
            {
                throw new ArgumentException("A namespace is required.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.ContextName))
            {
                throw new ArgumentException("A DbContext name is required.", nameof(options));
            }

            var interfaceBuilder = new StringBuilder();
            interfaceBuilder.AppendLine("using System.Linq;");
            interfaceBuilder.AppendLine("using System.Threading;");
            interfaceBuilder.AppendLine("using System.Threading.Tasks;");
            interfaceBuilder.AppendLine();
            interfaceBuilder.AppendLine($"namespace {options.Namespace}");
            interfaceBuilder.AppendLine("{");
            interfaceBuilder.AppendLine("    public interface IRepository<TEntity> where TEntity : class");
            interfaceBuilder.AppendLine("    {");
            interfaceBuilder.AppendLine("        IQueryable<TEntity> Query();");
            interfaceBuilder.AppendLine("        ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken cancellationToken = default);");
            interfaceBuilder.AppendLine("        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);");
            interfaceBuilder.AppendLine("        void Update(TEntity entity);");
            interfaceBuilder.AppendLine("        void Remove(TEntity entity);");
            interfaceBuilder.AppendLine("    }");
            interfaceBuilder.AppendLine("}");

            var repositoryBuilder = new StringBuilder();
            repositoryBuilder.AppendLine("using System.Linq;");
            repositoryBuilder.AppendLine("using System.Threading;");
            repositoryBuilder.AppendLine("using System.Threading.Tasks;");
            repositoryBuilder.AppendLine("using Microsoft.EntityFrameworkCore;");
            repositoryBuilder.AppendLine();
            repositoryBuilder.AppendLine($"namespace {options.Namespace}");
            repositoryBuilder.AppendLine("{");
            repositoryBuilder.AppendLine("    public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class");
            repositoryBuilder.AppendLine("    {");
            repositoryBuilder.AppendLine($"        private readonly {options.ContextName} _context;");
            repositoryBuilder.AppendLine("        private readonly DbSet<TEntity> _dbSet;");
            repositoryBuilder.AppendLine();
            repositoryBuilder.AppendLine($"        public EfRepository({options.ContextName} context)");
            repositoryBuilder.AppendLine("        {");
            repositoryBuilder.AppendLine("            _context = context;");
            repositoryBuilder.AppendLine("            _dbSet = context.Set<TEntity>();");
            repositoryBuilder.AppendLine("        }");
            repositoryBuilder.AppendLine();
            repositoryBuilder.AppendLine("        public IQueryable<TEntity> Query() => _dbSet;");
            repositoryBuilder.AppendLine();
            repositoryBuilder.AppendLine("        public ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)");
            repositoryBuilder.AppendLine("        {");
            repositoryBuilder.AppendLine("            return _dbSet.FindAsync(keyValues, cancellationToken);");
            repositoryBuilder.AppendLine("        }");
            repositoryBuilder.AppendLine();
            repositoryBuilder.AppendLine("        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)");
            repositoryBuilder.AppendLine("        {");
            repositoryBuilder.AppendLine("            return _dbSet.AddAsync(entity, cancellationToken).AsTask();");
            repositoryBuilder.AppendLine("        }");
            repositoryBuilder.AppendLine();
            repositoryBuilder.AppendLine("        public void Update(TEntity entity)");
            repositoryBuilder.AppendLine("        {");
            repositoryBuilder.AppendLine("            _dbSet.Update(entity);");
            repositoryBuilder.AppendLine("        }");
            repositoryBuilder.AppendLine();
            repositoryBuilder.AppendLine("        public void Remove(TEntity entity)");
            repositoryBuilder.AppendLine("        {");
            repositoryBuilder.AppendLine("            _dbSet.Remove(entity);");
            repositoryBuilder.AppendLine("        }");
            repositoryBuilder.AppendLine("    }");
            repositoryBuilder.AppendLine("}");

            var unitOfWorkBuilder = new StringBuilder();
            unitOfWorkBuilder.AppendLine("using System;");
            unitOfWorkBuilder.AppendLine("using System.Threading;");
            unitOfWorkBuilder.AppendLine("using System.Threading.Tasks;");
            unitOfWorkBuilder.AppendLine();
            unitOfWorkBuilder.AppendLine($"namespace {options.Namespace}");
            unitOfWorkBuilder.AppendLine("{");
            unitOfWorkBuilder.AppendLine("    public interface IUnitOfWork : IDisposable");
            unitOfWorkBuilder.AppendLine("    {");
            unitOfWorkBuilder.AppendLine("        IRepository<TEntity> Repository<TEntity>() where TEntity : class;");
            unitOfWorkBuilder.AppendLine("        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);");
            unitOfWorkBuilder.AppendLine("    }");
            unitOfWorkBuilder.AppendLine();
            unitOfWorkBuilder.AppendLine("    public class EfUnitOfWork : IUnitOfWork");
            unitOfWorkBuilder.AppendLine("    {");
            unitOfWorkBuilder.AppendLine($"        private readonly {options.ContextName} _context;");
            unitOfWorkBuilder.AppendLine();
            unitOfWorkBuilder.AppendLine($"        public EfUnitOfWork({options.ContextName} context)");
            unitOfWorkBuilder.AppendLine("        {");
            unitOfWorkBuilder.AppendLine("            _context = context;");
            unitOfWorkBuilder.AppendLine("        }");
            unitOfWorkBuilder.AppendLine();
            unitOfWorkBuilder.AppendLine("        public IRepository<TEntity> Repository<TEntity>() where TEntity : class");
            unitOfWorkBuilder.AppendLine("        {");
            unitOfWorkBuilder.AppendLine("            return new EfRepository<TEntity>(_context);");
            unitOfWorkBuilder.AppendLine("        }");
            unitOfWorkBuilder.AppendLine();
            unitOfWorkBuilder.AppendLine("        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)");
            unitOfWorkBuilder.AppendLine("        {");
            unitOfWorkBuilder.AppendLine("            return _context.SaveChangesAsync(cancellationToken);");
            unitOfWorkBuilder.AppendLine("        }");
            unitOfWorkBuilder.AppendLine();
            unitOfWorkBuilder.AppendLine("        public void Dispose()");
            unitOfWorkBuilder.AppendLine("        {");
            unitOfWorkBuilder.AppendLine("            _context.Dispose();");
            unitOfWorkBuilder.AppendLine("        }");
            unitOfWorkBuilder.AppendLine("    }");
            unitOfWorkBuilder.AppendLine("}");

            return new RepositoryScaffolding
            {
                RepositoryInterfaceCode = interfaceBuilder.ToString(),
                RepositoryImplementationCode = repositoryBuilder.ToString(),
                UnitOfWorkCode = unitOfWorkBuilder.ToString()
            };
        }

        private static void AppendTableMapping(StringBuilder builder, DbContextTableDefinition table)
        {
            builder.AppendLine($"            modelBuilder.Entity<{GetEntityName(table)}>(entity =>");
            builder.AppendLine("            {");
            builder.AppendLine($"                entity.ToTable(\"{EscapeCSharpString(table.TableName)}\");");

            if (table.KeyColumns.Count == 1)
            {
                builder.AppendLine($"                entity.HasKey(e => e.{table.KeyColumns[0]});");
            }
            else if (table.KeyColumns.Count > 1)
            {
                builder.AppendLine($"                entity.HasKey(e => new {{ {string.Join(", ", table.KeyColumns.Select(column => $"e.{column}"))} }});");
            }

            foreach (var relationship in table.Relationships)
            {
                ValidateRelationship(relationship);

                builder.AppendLine($"                entity.HasOne(e => e.{relationship.NavigationProperty})");

                if (string.IsNullOrWhiteSpace(relationship.PrincipalNavigationProperty))
                {
                    builder.AppendLine("                    .WithMany()");
                }
                else
                {
                    builder.AppendLine($"                    .WithMany(p => p.{relationship.PrincipalNavigationProperty})");
                }

                builder.AppendLine($"                    .HasForeignKey(e => e.{relationship.ForeignKeyProperty});");
            }

            builder.AppendLine("            });");
        }

        private static void ValidateTable(DbContextTableDefinition table)
        {
            ArgumentNullException.ThrowIfNull(table);

            if (string.IsNullOrWhiteSpace(table.TableName))
            {
                throw new ArgumentException("Each table requires a table name.", nameof(table));
            }
        }

        private static void ValidateRelationship(DbContextRelationshipDefinition relationship)
        {
            ArgumentNullException.ThrowIfNull(relationship);

            if (string.IsNullOrWhiteSpace(relationship.NavigationProperty))
            {
                throw new ArgumentException("Each relationship requires a navigation property.", nameof(relationship));
            }

            if (string.IsNullOrWhiteSpace(relationship.ForeignKeyProperty))
            {
                throw new ArgumentException("Each relationship requires a foreign-key property.", nameof(relationship));
            }
        }

        private static string GetEntityName(DbContextTableDefinition table)
        {
            return string.IsNullOrWhiteSpace(table.EntityName)
                ? ToPascalCase(table.TableName)
                : table.EntityName;
        }

        private static string GetDbSetName(DbContextTableDefinition table)
        {
            if (!string.IsNullOrWhiteSpace(table.DbSetName))
            {
                return table.DbSetName;
            }

            var entityName = GetEntityName(table);
            return entityName.EndsWith("s", StringComparison.Ordinal) ? entityName : $"{entityName}s";
        }

        private static string ToPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Entity";
            }

            var parts = value
                .Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant());

            return string.Concat(parts);
        }

        private static string EscapeCSharpString(string value)
        {
            return value.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal);
        }
    }

    public sealed class DbContextGenerationOptions
    {
        public string Namespace { get; init; } = "Generated.Data";

        public string ContextName { get; init; } = "ApplicationDbContext";

        public string EntityNamespace { get; init; } = "Generated.Data.Entities";

        public string ConnectionStringName { get; init; } = "DefaultConnection";

        public string EnvironmentVariableName { get; init; } = "BLML_CONNECTION_STRING";

        public IReadOnlyList<DbContextTableDefinition> Tables { get; init; } = Array.Empty<DbContextTableDefinition>();
    }

    public sealed class DbContextTableDefinition
    {
        public string TableName { get; init; } = string.Empty;

        public string EntityName { get; init; } = string.Empty;

        public string DbSetName { get; init; } = string.Empty;

        public IReadOnlyList<string> KeyColumns { get; init; } = Array.Empty<string>();

        public IReadOnlyList<DbContextRelationshipDefinition> Relationships { get; init; } = Array.Empty<DbContextRelationshipDefinition>();
    }

    public sealed class DbContextRelationshipDefinition
    {
        public string NavigationProperty { get; init; } = string.Empty;

        public string ForeignKeyProperty { get; init; } = string.Empty;

        public string PrincipalNavigationProperty { get; init; } = string.Empty;
    }

    public sealed class RepositoryGenerationOptions
    {
        public string Namespace { get; init; } = "Generated.Data.Repositories";

        public string ContextName { get; init; } = "ApplicationDbContext";
    }

    public sealed class RepositoryScaffolding
    {
        public string RepositoryInterfaceCode { get; init; } = string.Empty;

        public string RepositoryImplementationCode { get; init; } = string.Empty;

        public string UnitOfWorkCode { get; init; } = string.Empty;
    }
}

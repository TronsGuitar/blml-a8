using BLML.Phase4DataAccess.EntityFramework;

namespace BLML.Tests;

public class Phase4DataAccessTodoTests
{
    [Fact]
    public void DbContextGenerator_GenerateDbContext_ShouldGenerateDbContextDbSetsAndConnectionConfiguration()
    {
        var generator = new DbContextGenerator();
        var code = generator.GenerateDbContext(new DbContextGenerationOptions
        {
            Namespace = "Generated.Data",
            ContextName = "LegacyAccessContext",
            EntityNamespace = "Generated.Data.Entities",
            ConnectionStringName = "LegacyAccess",
            EnvironmentVariableName = "BLML_SQL_CONNECTION",
            Tables =
            [
                new DbContextTableDefinition
                {
                    TableName = "Customers",
                    EntityName = "Customer",
                    DbSetName = "Customers",
                    KeyColumns = ["CustomerId"]
                },
                new DbContextTableDefinition
                {
                    TableName = "Orders",
                    EntityName = "Order",
                    DbSetName = "Orders",
                    KeyColumns = ["OrderId"]
                }
            ]
        });

        Assert.Contains("public class LegacyAccessContext : DbContext", code);
        Assert.Contains("public DbSet<Customer> Customers => Set<Customer>();", code);
        Assert.Contains("public DbSet<Order> Orders => Set<Order>();", code);
        Assert.Contains("Environment.GetEnvironmentVariable(\"BLML_SQL_CONNECTION\")", code);
        Assert.Contains("configuration.GetConnectionString(\"LegacyAccess\")", code);
        Assert.Contains("optionsBuilder.UseSqlServer(connectionString);", code);
    }

    [Fact]
    public void DbContextGenerator_GenerateDbContext_ShouldGenerateKeysAndRelationships()
    {
        var generator = new DbContextGenerator();
        var code = generator.GenerateDbContext(new DbContextGenerationOptions
        {
            Namespace = "Generated.Data",
            ContextName = "LegacyAccessContext",
            Tables =
            [
                new DbContextTableDefinition
                {
                    TableName = "OrderLines",
                    EntityName = "OrderLine",
                    DbSetName = "OrderLines",
                    KeyColumns = ["OrderId", "LineNumber"],
                    Relationships =
                    [
                        new DbContextRelationshipDefinition
                        {
                            NavigationProperty = "Order",
                            ForeignKeyProperty = "OrderId",
                            PrincipalNavigationProperty = "OrderLines"
                        }
                    ]
                }
            ]
        });

        Assert.Contains("entity.ToTable(\"OrderLines\");", code);
        Assert.Contains("entity.HasKey(e => new { e.OrderId, e.LineNumber });", code);
        Assert.Contains("entity.HasOne(e => e.Order)", code);
        Assert.Contains(".WithMany(p => p.OrderLines)", code);
        Assert.Contains(".HasForeignKey(e => e.OrderId);", code);
    }

    [Fact]
    public void DbContextGenerator_GenerateRepositoryScaffolding_ShouldGenerateRepositoryAndUnitOfWorkCode()
    {
        var generator = new DbContextGenerator();
        var scaffolding = generator.GenerateRepositoryScaffolding(new RepositoryGenerationOptions
        {
            Namespace = "Generated.Data.Repositories",
            ContextName = "LegacyAccessContext"
        });

        Assert.Contains("public interface IRepository<TEntity> where TEntity : class", scaffolding.RepositoryInterfaceCode);
        Assert.Contains("public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class", scaffolding.RepositoryImplementationCode);
        Assert.Contains("private readonly LegacyAccessContext _context;", scaffolding.RepositoryImplementationCode);
        Assert.Contains("public interface IUnitOfWork : IDisposable", scaffolding.UnitOfWorkCode);
        Assert.Contains("return new EfRepository<TEntity>(_context);", scaffolding.UnitOfWorkCode);
    }

    [Fact]
    public void Phase4Readme_ShouldDescribeDbContextGeneratorProgress()
    {
        var readmePath = Path.Combine(GetRepoRoot(), "src", "Phase4-DataAccess", "README.md");
        var content = File.ReadAllText(readmePath);

        Assert.Contains("DbContextGenerator.cs", content);
        Assert.Contains("The Entity Framework generator now supports", content);
        Assert.Contains("generating an EF Core `DbContext` source file", content);
        Assert.Contains("OnConfiguring(...)", content);
        Assert.Contains("SchemaGenerator.cs", content);
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ReadMe.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}

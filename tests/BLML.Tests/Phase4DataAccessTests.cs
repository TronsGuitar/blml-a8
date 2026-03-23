using Xunit;
using BLML.Phase4DataAccess.Models;
using BLML.Phase4DataAccess.EntityFramework;
using BLML.Phase4DataAccess.SqlServer;
using BLML.Phase4DataAccess.ADO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Tests
{
    public class Phase4DataAccessTests
    {
        [Fact]
        public void EntityGenerator_ShouldGeneratePocoClass()
        {
            var table = new TableMetadata
            {
                Name = "Customers",
                Columns = new List<ColumnMetadata>
                {
                    new ColumnMetadata { Name = "CustomerId", DataType = "int", IsNullable = false },
                    new ColumnMetadata { Name = "Name", DataType = "string", IsNullable = true },
                    new ColumnMetadata { Name = "BirthDate", DataType = "datetime", IsNullable = true }
                }
            };

            var generator = new EntityGenerator();
            var output = generator.GenerateEntity(table);

            Assert.Contains("public class Customers", output);
            Assert.Contains("public int CustomerId { get; set; }", output);
            Assert.Contains("public string Name { get; set; }", output);
            Assert.Contains("public DateTime? BirthDate { get; set; }", output);
        }

        [Fact]
        public void DbContextGenerator_ShouldGenerateContextClass()
        {
            var tables = new List<TableMetadata>
            {
                new TableMetadata { Name = "Customers", PrimaryKeyColumns = new List<string> { "CustomerId" } },
                new TableMetadata { Name = "Orders", PrimaryKeyColumns = new List<string> { "OrderId" } }
            };

            var generator = new DbContextGenerator();
            var output = generator.GenerateDbContext("AppDbContext", tables);

            Assert.Contains("public class AppDbContext : DbContext", output);
            Assert.Contains("public DbSet<Customers> Customerss { get; set; }", output);
            Assert.Contains("public DbSet<Orders> Orderss { get; set; }", output);
            Assert.Contains("entity.ToTable(\"Customers\");", output);
            Assert.Contains("entity.HasKey(e => new { e.CustomerId });", output);
        }

        [Fact]
        public void SqlSchemaGenerator_ShouldGenerateCreateScript()
        {
            var table = new TableMetadata
            {
                Name = "Products",
                Columns = new List<ColumnMetadata>
                {
                    new ColumnMetadata { Name = "Id", DataType = "int", IsNullable = false },
                    new ColumnMetadata { Name = "Price", DataType = "decimal", IsNullable = false }
                },
                PrimaryKeyColumns = new List<string> { "Id" }
            };

            var generator = new SchemaGenerator();
            var output = generator.GenerateCreateScript(table);

            Assert.Contains("CREATE TABLE [Products]", output);
            Assert.Contains("[Id] INT NOT NULL,", output);
            Assert.Contains("[Price] DECIMAL(18,2) NOT NULL,", output);
            Assert.Contains("CONSTRAINT PK_Products PRIMARY KEY ([Id])", output);
        }

        [Fact]
        public void AdoConverter_ShouldConvertAdoToAdoNet()
        {
            var converter = new AdoConverter();
            
            var openStmt = converter.ConvertConnectionOpen("\"connectionString\"");
            Assert.Contains("new System.Data.SqlClient.SqlConnection", openStmt.ToFullString());
            
            var fieldAccess = converter.ConvertFieldValue("reader", "Name");
            Assert.Equal("reader[\"Name\"]", fieldAccess.ToFullString());
            
            var closeStmt = converter.ConvertClose("cn");
            Assert.Equal("cn.Close();", closeStmt.ToFullString());
        }
    }
}

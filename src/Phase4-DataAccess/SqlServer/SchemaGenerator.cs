namespace BLML.Phase4DataAccess.SqlServer
{
    public class SchemaGenerator
    {
        public string GenerateCreateScript(string tableName, string[] columns)
        {
            // Simple T-SQL generator
            return $"CREATE TABLE [{tableName}] ( ... );";
        }
    }
}

namespace BLML.Phase5ASPtoAngular.Database
{
    public class RepositoryGenerator
    {
        public string GenerateRepository(string entity)
        {
            return $"public class {entity}Repository : I{entity}Repository {{ }}";
        }
    }
}

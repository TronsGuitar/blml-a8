namespace BLML.Phase5ASPtoAngular.Backend.ApiGenerator
{
    public class ServiceGenerator
    {
        public string GenerateService(string name)
        {
            return $"public class {name}Service {{ }}";
        }
    }
}

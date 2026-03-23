namespace BLML.Phase5ASPtoAngular.Backend.ApiGenerator
{
    public class DtoGenerator
    {
        public string GenerateDto(string name)
        {
            return $"public class {name}Dto {{ }}";
        }
    }
}

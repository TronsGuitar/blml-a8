namespace BLML.Phase5ASPtoAngular.Backend.ApiGenerator
{
    public class ControllerGenerator
    {
        public string GenerateController(string name)
        {
            return $"public class {name}Controller : ControllerBase {{ }}";
        }
    }
}

namespace BLML.Phase5ASPtoAngular.Frontend
{
    public class ComponentGenerator
    {
        public string GenerateComponent(string name)
        {
            return $"@Component({{\n  selector: 'app-{name.ToLower()}',\n  templateUrl: './{name.ToLower()}.component.html'\n}})";
        }
    }
}

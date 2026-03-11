namespace BLML.Phase3FormsUI.Models;

public sealed class Vb6FormDefinition
{
    public string Name { get; set; } = "Form1";

    public Dictionary<string, string> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<Vb6ControlDefinition> Controls { get; } = new();

    public IEnumerable<Vb6ControlDefinition> GetAllControls()
    {
        foreach (var control in Controls)
        {
            yield return control;

            foreach (var descendant in control.Descendants())
            {
                yield return descendant;
            }
        }
    }
}

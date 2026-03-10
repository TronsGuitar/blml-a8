using System;
using System.Collections.Generic;

namespace BLML.Phase3FormsUI.Models;

public sealed class Vb6ControlDefinition
{
    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Guid { get; set; }

    public Dictionary<string, string> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<Vb6ControlDefinition> Children { get; } = new();

    public IEnumerable<Vb6ControlDefinition> Descendants()
    {
        foreach (var child in Children)
        {
            yield return child;

            foreach (var descendant in child.Descendants())
            {
                yield return descendant;
            }
        }
    }
}

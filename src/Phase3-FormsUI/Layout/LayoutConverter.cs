using BLML.Phase3FormsUI.Models;
using System.Globalization;

namespace BLML.Phase3FormsUI.Layout;

public sealed class LayoutConverter
{
    public sealed class LayoutRowPlan
    {
        public int RowIndex { get; init; }

        public IReadOnlyList<string> ControlNames { get; init; } = Array.Empty<string>();
    }

    public int ConvertTwipsToPixels(int twips)
    {
        return (int)Math.Round(twips / 15d, MidpointRounding.AwayFromZero);
    }

    public IReadOnlyList<LayoutRowPlan> BuildRowPlan(IEnumerable<Vb6ControlDefinition> controls, int verticalToleranceTwips = 120)
    {
        var orderedControls = controls
            .Select(control => new
            {
                Control = control,
                Top = ReadNumericProperty(control, "Top"),
                Left = ReadNumericProperty(control, "Left")
            })
            .OrderBy(item => item.Top)
            .ThenBy(item => item.Left)
            .ToList();

        var rows = new List<List<string>>();
        var rowAnchors = new List<int>();

        foreach (var item in orderedControls)
        {
            var rowIndex = rowAnchors.FindIndex(anchor => Math.Abs(anchor - item.Top) <= verticalToleranceTwips);
            if (rowIndex < 0)
            {
                rowAnchors.Add(item.Top);
                rows.Add(new List<string> { item.Control.Name });
            }
            else
            {
                rows[rowIndex].Add(item.Control.Name);
            }
        }

        return rows
            .Select((controlsInRow, index) => new LayoutRowPlan
            {
                RowIndex = index,
                ControlNames = controlsInRow
            })
            .ToArray();
    }

    private static int ReadNumericProperty(Vb6ControlDefinition control, string propertyName)
    {
        if (control.Properties.TryGetValue(propertyName, out var value) &&
            int.TryParse(value.Trim().Trim('"'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return 0;
    }
}

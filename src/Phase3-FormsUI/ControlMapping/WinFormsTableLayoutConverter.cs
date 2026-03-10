using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLML.Phase3FormsUI.ControlMapping;

public sealed class WinFormsTableLayoutConverter
{
    public sealed class ControlLayoutInfo
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }

    public IReadOnlyList<ControlLayoutInfo> ExtractControls(IEnumerable<string> lines)
    {
        var createRegex = new Regex(@"this\.(?<name>\w+)\s*=\s*new\s*System\.Windows\.Forms\.(?<type>\w+)\s*\(\)");
        var locationRegex = new Regex(@"this\.(?<name>\w+)\.Location\s*=\s*new\s*System\.Drawing\.Point\((?<x>\d+),\s*(?<y>\d+)\)");
        var sizeRegex = new Regex(@"this\.(?<name>\w+)\.Size\s*=\s*new\s*System\.Drawing\.Size\((?<width>\d+),\s*(?<height>\d+)\)");

        var controls = new Dictionary<string, ControlLayoutInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var createMatch = createRegex.Match(line);
            if (createMatch.Success)
            {
                controls[createMatch.Groups["name"].Value] = new ControlLayoutInfo
                {
                    Name = createMatch.Groups["name"].Value,
                    Type = createMatch.Groups["type"].Value
                };
                continue;
            }

            var locationMatch = locationRegex.Match(line);
            if (locationMatch.Success && controls.TryGetValue(locationMatch.Groups["name"].Value, out var locatedControl))
            {
                locatedControl.X = int.Parse(locationMatch.Groups["x"].Value);
                locatedControl.Y = int.Parse(locationMatch.Groups["y"].Value);
                continue;
            }

            var sizeMatch = sizeRegex.Match(line);
            if (sizeMatch.Success && controls.TryGetValue(sizeMatch.Groups["name"].Value, out var sizedControl))
            {
                sizedControl.Width = int.Parse(sizeMatch.Groups["width"].Value);
                sizedControl.Height = int.Parse(sizeMatch.Groups["height"].Value);
            }
        }

        return controls.Values.OrderBy(control => control.Y).ThenBy(control => control.X).ToArray();
    }

    public IReadOnlyList<string> RebuildWithTableLayout(IReadOnlyList<string> lines, IReadOnlyList<ControlLayoutInfo> controls)
    {
        if (controls.Count == 0)
        {
            return lines.ToArray();
        }

        var placements = BuildPlacements(controls);
        var controlNames = controls.Select(control => control.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = lines
            .Where(line => !IsLocationOrSizeLine(line) && !IsDirectFormAddLine(line, controlNames))
            .ToList();

        var declarationIndex = result.FindIndex(line => line.Contains("private System.ComponentModel.IContainer components", StringComparison.Ordinal));
        if (declarationIndex >= 0 && !result.Any(line => line.Contains("tableLayoutPanelMain", StringComparison.Ordinal)))
        {
            result.Insert(declarationIndex + 1, "        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;");
        }

        var insertIndex = result.FindIndex(line => line.Contains("this.ResumeLayout", StringComparison.Ordinal));
        if (insertIndex < 0)
        {
            insertIndex = result.FindIndex(line => line.Contains("this.PerformLayout", StringComparison.Ordinal));
        }

        if (insertIndex < 0)
        {
            insertIndex = result.Count;
        }

        var rowCount = placements.Max(placement => placement.Row) + 1;
        var columnCount = placements.Max(placement => placement.Column) + 1;
        var generatedLines = new List<string>
        {
            "            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();",
            $"            this.tableLayoutPanelMain.ColumnCount = {columnCount};",
            $"            this.tableLayoutPanelMain.RowCount = {rowCount};",
            "            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;"
        };

        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            generatedLines.Add($"            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, {100.0 / columnCount:0.####}F));");
        }

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            generatedLines.Add($"            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, {100.0 / rowCount:0.####}F));");
        }

        foreach (var placement in placements.OrderBy(item => item.Row).ThenBy(item => item.Column))
        {
            generatedLines.Add($"            this.tableLayoutPanelMain.Controls.Add(this.{placement.Control.Name}, {placement.Column}, {placement.Row});");
            generatedLines.Add($"            this.{placement.Control.Name}.Dock = System.Windows.Forms.DockStyle.Fill;");
        }

        generatedLines.Add("            this.Controls.Add(this.tableLayoutPanelMain);");
        result.InsertRange(insertIndex, generatedLines);
        return result;
    }

    public void ConvertFiles(string inputDesigner, string outputDesigner, string inputResx, string outputResx)
    {
        var lines = File.ReadAllLines(inputDesigner);
        var controls = ExtractControls(lines);
        var rebuiltDesigner = RebuildWithTableLayout(lines, controls);
        File.WriteAllLines(outputDesigner, rebuiltDesigner);
        File.Copy(inputResx, outputResx, overwrite: true);
    }

    private static bool IsLocationOrSizeLine(string line)
    {
        return line.Contains(".Location =", StringComparison.Ordinal) || line.Contains(".Size =", StringComparison.Ordinal);
    }

    private static bool IsDirectFormAddLine(string line, ISet<string> controlNames)
    {
        var match = Regex.Match(line, @"this\.Controls\.Add\(this\.(?<name>\w+)\);", RegexOptions.CultureInvariant);
        return match.Success && controlNames.Contains(match.Groups["name"].Value);
    }

    private static IReadOnlyList<ControlPlacement> BuildPlacements(IReadOnlyList<ControlLayoutInfo> controls)
    {
        const int tolerance = 10;
        var rowAnchors = new List<int>();
        var columnAnchors = new List<int>();
        var placements = new List<ControlPlacement>();

        foreach (var control in controls.OrderBy(item => item.Y).ThenBy(item => item.X))
        {
            var row = FindOrAddAnchor(rowAnchors, control.Y, tolerance);
            var column = FindOrAddAnchor(columnAnchors, control.X, tolerance);
            placements.Add(new ControlPlacement(control, row, column));
        }

        return placements;
    }

    private static int FindOrAddAnchor(ICollection<int> anchors, int value, int tolerance)
    {
        var indexedAnchors = anchors.Select((anchor, index) => new { anchor, index }).ToList();
        var existing = indexedAnchors.FirstOrDefault(item => Math.Abs(item.anchor - value) <= tolerance);
        if (existing is not null)
        {
            return existing.index;
        }

        anchors.Add(value);
        return anchors.Count - 1;
    }

    private sealed record ControlPlacement(ControlLayoutInfo Control, int Row, int Column);
}

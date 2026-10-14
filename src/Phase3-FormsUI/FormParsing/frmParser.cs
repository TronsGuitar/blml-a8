using BLML.Phase3FormsUI.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace BLML.Phase3FormsUI.FormParsing;

public static class FrmParser
{
    private static readonly Dictionary<string, string> Vb6ToCSharpControls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["VB.CommandButton"] = "System.Windows.Forms.Button",
        ["VB.TextBox"] = "System.Windows.Forms.TextBox",
        ["VB.Label"] = "System.Windows.Forms.Label",
        ["VB.CheckBox"] = "System.Windows.Forms.CheckBox",
        ["VB.OptionButton"] = "System.Windows.Forms.RadioButton",
        ["VB.ListBox"] = "System.Windows.Forms.ListBox",
        ["VB.ComboBox"] = "System.Windows.Forms.ComboBox",
        ["VB.Frame"] = "System.Windows.Forms.GroupBox",
        ["VB.PictureBox"] = "System.Windows.Forms.PictureBox",
        ["VB.HScrollBar"] = "System.Windows.Forms.HScrollBar",
        ["VB.VScrollBar"] = "System.Windows.Forms.VScrollBar",
        ["VB.Timer"] = "System.Windows.Forms.Timer",

        // Advanced/ActiveX controls (ProgIDs as VB6 .frm files actually declare them -
        // these come from separate OCX libraries, not the VB.* set above).
        ["TabDlg.SSTab"] = "System.Windows.Forms.TabControl", // tabctl32.ocx
        ["MSComctlLib.TreeView"] = "System.Windows.Forms.TreeView", // comctl32.ocx / mscomctl.ocx
        ["MSComctlLib.ListView"] = "System.Windows.Forms.ListView", // comctl32.ocx / mscomctl.ocx
        ["MSFlexGridLib.MSFlexGrid"] = "System.Windows.Forms.DataGridView", // msflxgrd.ocx
        ["RichTextLib.RichTextBox"] = "System.Windows.Forms.RichTextBox", // richtx32.ocx
        // VB6's CommonDialog (comdlg32.ocx) is one non-visual control whose ShowOpen/
        // ShowSave/ShowColor/ShowFont/ShowPrint methods each show a different dialog;
        // .NET has no single equivalent type, since each of those is its own class
        // (OpenFileDialog/SaveFileDialog/ColorDialog/FontDialog/PrintDialog). Mapping
        // the control declaration to OpenFileDialog covers the single most common use
        // (ShowOpen) - call sites using the other Show* methods need a manual follow-up
        // to the matching dialog type, since a per-declaration mapping can't know in
        // advance which Show* method(s) a given CommonDialog instance will actually use.
        ["MSComDlg.CommonDialog"] = "System.Windows.Forms.OpenFileDialog"
    };

    private static readonly Regex BeginRegex = new(@"^Begin\s+(?<Type>[\w\.]+)\s+(?<Name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static Vb6FormDefinition ParseFile(string inputFilePath)
    {
        return ParseContent(File.ReadAllText(inputFilePath));
    }

    public static Vb6FormDefinition ParseContent(string vb6FormContent)
    {
        var form = new Vb6FormDefinition();
        var controlStack = new Stack<Vb6ControlDefinition>();

        foreach (var rawLine in SplitLines(vb6FormContent))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var beginMatch = BeginRegex.Match(line);
            if (beginMatch.Success)
            {
                var type = beginMatch.Groups["Type"].Value;
                var name = beginMatch.Groups["Name"].Value;

                if (type.Equals("VB.Form", StringComparison.OrdinalIgnoreCase))
                {
                    form.Name = name;
                    controlStack.Clear();
                    continue;
                }

                var control = new Vb6ControlDefinition
                {
                    Type = type,
                    Name = name
                };

                if (controlStack.Count == 0)
                {
                    form.Controls.Add(control);
                }
                else
                {
                    controlStack.Peek().Children.Add(control);
                }

                controlStack.Push(control);
                continue;
            }

            if (line.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                if (controlStack.Count > 0)
                {
                    controlStack.Pop();
                }

                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex < 0)
            {
                continue;
            }

            var propertyName = line[..equalsIndex].Trim();
            var propertyValue = line[(equalsIndex + 1)..].Trim();

            if (controlStack.Count == 0)
            {
                form.Properties[propertyName] = propertyValue;
            }
            else
            {
                controlStack.Peek().Properties[propertyName] = propertyValue;
            }
        }

        return form;
    }

    public static string ConvertToIntermediateFormat(Vb6FormDefinition form)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"C# Form: {form.Name}");

        foreach (var formProperty in form.Properties.OrderBy(property => property.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"FormProperty: {formProperty.Key} = {formProperty.Value}");
        }

        if (form.Properties.Count > 0 && form.Controls.Count > 0)
        {
            builder.AppendLine();
        }

        foreach (var control in form.Controls)
        {
            WriteControl(builder, control, depth: 0);
        }

        return builder.ToString();
    }

    public static void ParseAndConvertToCSharp(string inputFilePath, string outputFilePath)
    {
        var form = ParseFile(inputFilePath);
        var converted = ConvertToIntermediateFormat(form);

        var outputDirectory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.WriteAllText(outputFilePath, converted);
    }

    public static string MapToCSharpControlType(string vb6Type)
    {
        return Vb6ToCSharpControls.TryGetValue(vb6Type, out var csharpType)
            ? csharpType
            : vb6Type;
    }

    private static void WriteControl(StringBuilder builder, Vb6ControlDefinition control, int depth)
    {
        var indent = new string(' ', depth * 2);
        builder.AppendLine($"{indent}Control: {MapToCSharpControlType(control.Type)} Name: {control.Name}");

        foreach (var property in control.Properties.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"{indent}  {property.Key} = {property.Value}");
        }

        builder.AppendLine();

        foreach (var child in control.Children)
        {
            WriteControl(builder, child, depth + 1);
        }
    }

    private static IEnumerable<string> SplitLines(string content)
    {
        return content.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
    }
}

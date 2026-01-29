using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class FrmParser
{
    // Mapping of VB6 controls to C# equivalents
    private static readonly Dictionary<string, string> Vb6ToCSharpControls = new Dictionary<string, string>
    {
        { "VB.CommandButton", "System.Windows.Forms.Button" },
        { "VB.TextBox", "System.Windows.Forms.TextBox" },
        { "VB.Label", "System.Windows.Forms.Label" },
        { "VB.CheckBox", "System.Windows.Forms.CheckBox" },
        { "VB.OptionButton", "System.Windows.Forms.RadioButton" },
        { "VB.ListBox", "System.Windows.Forms.ListBox" },
        { "VB.ComboBox", "System.Windows.Forms.ComboBox" },
        { "VB.Frame", "System.Windows.Forms.GroupBox" },
        { "VB.PictureBox", "System.Windows.Forms.PictureBox" },
        { "VB.HScrollBar", "System.Windows.Forms.HScrollBar" },
        { "VB.VScrollBar", "System.Windows.Forms.VScrollBar" },
        { "VB.Timer", "System.Windows.Forms.Timer" }
    };

    public static void ParseAndConvertToCSharp(string inputFilePath, string outputFilePath)
    {
        var controls = new List<ControlData>();
        ControlData currentControl = null;

        foreach (var line in File.ReadLines(inputFilePath))
        {
            var trimmedLine = line.Trim();

            // Identify a new control
            if (trimmedLine.StartsWith("Begin "))
            {
                var match = Regex.Match(trimmedLine, @"Begin\s+(\w+\.\w+)\s+(\w+)");
                if (match.Success)
                {
                    var vb6Type = match.Groups[1].Value;
                    var controlName = match.Groups[2].Value;
                    currentControl = new ControlData
                    {
                        Type = Vb6ToCSharpControls.ContainsKey(vb6Type) ? Vb6ToCSharpControls[vb6Type] : vb6Type,
                        Name = controlName
                    };
                    controls.Add(currentControl);
                }
            }
            // End of control declaration
            else if (trimmedLine == "End")
            {
                currentControl = null;
            }
            // Capture properties
            else if (currentControl != null && trimmedLine.Contains('='))
            {
                var parts = trimmedLine.Split('=');
                var propName = parts[0].Trim();
                var propValue = parts[1].Trim();
                currentControl.Properties[propName] = propValue;
            }
        }

        // Generate .frmx file
        using (var writer = new StreamWriter(outputFilePath))
        {
            writer.WriteLine("C# Form");
            foreach (var control in controls)
            {
                writer.WriteLine($"Control: {control.Type} Name: {control.Name}");
                foreach (var prop in control.Properties)
                {
                    writer.WriteLine($"  {prop.Key} = {prop.Value}");
                }
                writer.WriteLine();
            }
        }

        Console.WriteLine($".frmx file generated at {outputFilePath}");
    }

    private class ControlData
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
    }

    static void Main(string[] args)
    {
        string inputFilePath = "path_to_your_form.frm";
        string outputFilePath = "path_to_your_output.frmx";
        
        ParseAndConvertToCSharp(inputFilePath, outputFilePath);
    }
}
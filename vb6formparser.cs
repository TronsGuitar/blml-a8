using System;
using System.Collections.Generic;
using System.Text;

static string ConvertVB6FormToCSharp(string vb6FormContent)
{
    // Classes to represent controls and properties
    class Control
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    List<Control> controls = new List<Control>();
    Control currentControl = null;
    string formName = "Form1"; // Default form name
    Dictionary<string, string> formProperties = new Dictionary<string, string>();

    // Split the content into lines
    var lines = vb6FormContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

    for (int i = 0; i < lines.Length; i++)
    {
        string line = lines[i].Trim();

        // Skip empty lines
        if (string.IsNullOrWhiteSpace(line))
            continue;

        if (line.StartsWith("Begin VB."))
        {
            // Start of a control
            int firstSpace = line.IndexOf(' ');
            int secondSpace = line.IndexOf(' ', firstSpace + 1);

            string typePart = line.Substring(6, firstSpace - 6).Trim(); // Control type
            string controlType = typePart;
            string controlName = line.Substring(secondSpace + 1).Trim();

            currentControl = new Control()
            {
                Type = controlType,
                Name = controlName
            };
        }
        else if (line.StartsWith("End"))
        {
            // End of a control
            if (currentControl != null)
            {
                controls.Add(currentControl);
                currentControl = null;
            }
        }
        else if (currentControl != null)
        {
            // Inside a control, reading properties
            int eqIndex = line.IndexOf('=');
            if (eqIndex > -1)
            {
                string propName = line.Substring(0, eqIndex).Trim();
                string propValue = line.Substring(eqIndex + 1).Trim().Trim('"');

                currentControl.Properties[propName] = propValue;
            }
        }
        else if (line.StartsWith("Begin"))
        {
            // Begin Form
            int firstSpace = line.IndexOf(' ');
            string formType = line.Substring(firstSpace + 1, line.IndexOf(' ', firstSpace + 1) - firstSpace - 1).Trim();
            formName = line.Substring(line.LastIndexOf(' ') + 1).Trim();
        }
        else
        {
            // Form properties
            int eqIndex = line.IndexOf('=');
            if (eqIndex > -1)
            {
                string propName = line.Substring(0, eqIndex).Trim();
                string propValue = line.Substring(eqIndex + 1).Trim().Trim('"');

                formProperties[propName] = propValue;
            }
        }
    }

    // Now generate the C# code
    StringBuilder csharpCode = new StringBuilder();

    csharpCode.AppendLine("using System;");
    csharpCode.AppendLine("using System.Windows.Forms;");
    csharpCode.AppendLine();
    csharpCode.AppendLine($"public class {formName} : Form");
    csharpCode.AppendLine("{");

    // Declare controls
    foreach (var control in controls)
    {
        string csharpControlType = MapControlType(control.Type);
        csharpCode.AppendLine($"    private {csharpControlType} {control.Name};");
    }

    csharpCode.AppendLine();
    csharpCode.AppendLine($"    public {formName}()");
    csharpCode.AppendLine("    {");
    csharpCode.AppendLine("        InitializeComponent();");
    csharpCode.AppendLine("    }");
    csharpCode.AppendLine();
    csharpCode.AppendLine("    private void InitializeComponent()");
    csharpCode.AppendLine("    {");

    // Initialize controls
    foreach (var control in controls)
    {
        string csharpControlType = MapControlType(control.Type);
        csharpCode.AppendLine($"        this.{control.Name} = new {csharpControlType}();");

        foreach (var prop in control.Properties)
        {
            string propName = MapPropertyName(prop.Key);
            string propValue = prop.Value;

            // Map properties as needed
            if (propName == "Text")
            {
                csharpCode.AppendLine($"        this.{control.Name}.{propName} = \"{propValue}\";");
            }
            else if (propName == "Size")
            {
                // Convert VB6 units to pixels if necessary
                csharpCode.AppendLine($"        this.{control.Name}.{propName} = new System.Drawing.Size({propValue}, {propValue});");
            }
            else if (propName == "Location")
            {
                csharpCode.AppendLine($"        this.{control.Name}.{propName} = new System.Drawing.Point({propValue}, {propValue});");
            }
            else
            {
                csharpCode.AppendLine($"        this.{control.Name}.{propName} = {propValue};");
            }
        }

        csharpCode.AppendLine($"        this.Controls.Add(this.{control.Name});");
    }

    csharpCode.AppendLine("    }");
    csharpCode.AppendLine("}");

    return csharpCode.ToString();

    // Local functions for control and property mapping
    string MapControlType(string vb6ControlType)
    {
        switch (vb6ControlType)
        {
            case "CommandButton":
                return "Button";
            case "TextBox":
                return "TextBox";
            case "Label":
                return "Label";
            // Add other control mappings as needed
            default:
                return "Control";
        }
    }

    string MapPropertyName(string vb6PropertyName)
    {
        switch (vb6PropertyName)
        {
            case "Caption":
                return "Text";
            case "Left":
            case "Top":
                return "Location";
            case "Width":
            case "Height":
                return "Size";
            case "Visible":
                return "Visible";
            // Add other property mappings as needed
            default:
                return vb6PropertyName;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VB6ToCSConverter
{
    class Program
    {
        // Mapping of VB6 control types to C# WinForms types.
        // If a control is not in this dictionary, it will be declared as dynamic.
        static Dictionary<string, string> controlMapping = new Dictionary<string, string>
        {
            {"VB.CommandButton", "Button"},
            {"VB.TextBox", "TextBox"},
            {"VB.Label", "Label"},
            {"VB.CheckBox", "CheckBox"},
            {"VB.ComboBox", "ComboBox"},
            {"VB.Frame", "GroupBox"},
            {"VB.ListBox", "ListBox"},
            {"VB.PictureBox", "PictureBox"},
            {"VB.Timer", "Timer"},
            {"VB.OptionButton", "RadioButton"},
            {"VB.Form", "Form"}
        };

        // Rough conversion factor from twips to pixels (1 pixel ~15 twips)
        const int TwipsPerPixel = 15;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: VB6ToCSConverter <path to VB6 frm file>");
                return;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            string formName = "MyForm";
            string formCaption = "MyForm";
            int clientWidth = 300, clientHeight = 200;
            List<ControlInfo> controls = new List<ControlInfo>();

            bool inForm = false;
            bool inControl = false;
            ControlInfo currentControl = null;

            // Simple parser for the VB6 form file.
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("Begin VB.Form"))
                {
                    inForm = true;
                    // Format: Begin VB.Form Form1
                    string[] parts = trimmed.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                        formName = parts[2];
                }
                else if (inForm && trimmed.StartsWith("Caption"))
                {
                    // Example: Caption = "My Form"
                    int idx = trimmed.IndexOf("=");
                    if (idx > 0)
                    {
                        formCaption = trimmed.Substring(idx + 1).Trim().Trim('"');
                    }
                }
                else if (inForm && trimmed.StartsWith("ClientWidth"))
                {
                    int idx = trimmed.IndexOf("=");
                    if (idx > 0 && int.TryParse(trimmed.Substring(idx + 1).Trim(), out int width))
                    {
                        clientWidth = width / TwipsPerPixel;
                    }
                }
                else if (inForm && trimmed.StartsWith("ClientHeight"))
                {
                    int idx = trimmed.IndexOf("=");
                    if (idx > 0 && int.TryParse(trimmed.Substring(idx + 1).Trim(), out int height))
                    {
                        clientHeight = height / TwipsPerPixel;
                    }
                }
                else if (inForm && trimmed.StartsWith("Begin") && trimmed.Contains("VB."))
                {
                    // Start of a control block, e.g., "Begin VB.CommandButton cmdOK"
                    string[] parts = trimmed.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        inControl = true;
                        currentControl = new ControlInfo();
                        currentControl.VBType = parts[1];
                        currentControl.Name = parts[2];
                    }
                }
                else if (inControl && trimmed.StartsWith("End"))
                {
                    // End of control block.
                    if (currentControl != null)
                    {
                        controls.Add(currentControl);
                        currentControl = null;
                    }
                    inControl = false;
                }
                else if (inControl && currentControl != null)
                {
                    // Assume property assignment, e.g., "Caption = \"OK\"" or "Left = 1320"
                    int idx = trimmed.IndexOf("=");
                    if (idx > 0)
                    {
                        string propName = trimmed.Substring(0, idx).Trim();
                        string propValue = trimmed.Substring(idx + 1).Trim().Trim('"');
                        currentControl.Properties[propName] = propValue;
                    }
                }
                else if (inForm && trimmed.StartsWith("End"))
                {
                    // End of form block.
                    inForm = false;
                }
            }

            // Generate the C# form class as a string.
            StringBuilder csCode = new StringBuilder();
            csCode.AppendLine("using System;");
            csCode.AppendLine("using System.Drawing;");
            csCode.AppendLine("using System.Windows.Forms;");
            csCode.AppendLine();
            csCode.AppendLine("namespace ConvertedForms");
            csCode.AppendLine("{");
            csCode.AppendLine($"    public class {formName} : Form");
            csCode.AppendLine("    {");

            // Field declarations for each control.
            foreach (var ctrl in controls)
            {
                string csType;
                if (!controlMapping.TryGetValue(ctrl.VBType, out csType))
                    csType = "dynamic";
                csCode.AppendLine($"        private {csType} {ctrl.Name};");
            }
            csCode.AppendLine();

            // Constructor.
            csCode.AppendLine($"        public {formName}()");
            csCode.AppendLine("        {");
            csCode.AppendLine("            InitializeComponent();");
            csCode.AppendLine("        }");
            csCode.AppendLine();

            // InitializeComponent method.
            csCode.AppendLine("        private void InitializeComponent()");
            csCode.AppendLine("        {");
            csCode.AppendLine($"            this.ClientSize = new Size({clientWidth}, {clientHeight});");
            csCode.AppendLine($"            this.Text = \"{formCaption}\";");
            csCode.AppendLine();

            // Process each control, instantiate, set properties, and add to the form.
            foreach (var ctrl in controls)
            {
                string csType;
                if (!controlMapping.TryGetValue(ctrl.VBType, out csType))
                    csType = "dynamic";

                csCode.AppendLine($"            // {ctrl.Name} ({ctrl.VBType})");
                csCode.AppendLine($"            this.{ctrl.Name} = new {GetControlInitialization(csType)};");
                
                // Process common properties.
                foreach (var prop in ctrl.Properties)
                {
                    if (prop.Key.Equals("Caption", StringComparison.OrdinalIgnoreCase) ||
                        prop.Key.Equals("Text", StringComparison.OrdinalIgnoreCase))
                    {
                        csCode.AppendLine($"            this.{ctrl.Name}.Text = \"{prop.Value}\";");
                    }
                    else if (prop.Key.Equals("Left", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(prop.Value, out int left))
                            ctrl.Left = left / TwipsPerPixel;
                    }
                    else if (prop.Key.Equals("Top", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(prop.Value, out int top))
                            ctrl.Top = top / TwipsPerPixel;
                    }
                    else if (prop.Key.Equals("Width", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(prop.Value, out int width))
                            ctrl.Width = width / TwipsPerPixel;
                    }
                    else if (prop.Key.Equals("Height", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(prop.Value, out int height))
                            ctrl.Height = height / TwipsPerPixel;
                    }
                    // Additional properties can be handled here.
                }
                if (ctrl.Left != 0 || ctrl.Top != 0)
                    csCode.AppendLine($"            this.{ctrl.Name}.Location = new Point({ctrl.Left}, {ctrl.Top});");
                if (ctrl.Width != 0 && ctrl.Height != 0)
                    csCode.AppendLine($"            this.{ctrl.Name}.Size = new Size({ctrl.Width}, {ctrl.Height});");
                csCode.AppendLine($"            this.Controls.Add(this.{ctrl.Name});");
                csCode.AppendLine();
            }
            csCode.AppendLine("        }");
            csCode.AppendLine();

            // Generate stub event handlers if a control has an event property "Event_Click".
            foreach (var ctrl in controls)
            {
                // This sample assumes that if a control has an "Event_Click" property, it needs a Click event handler.
                if (ctrl.VBType == "VB.CommandButton" && ctrl.Properties.ContainsKey("Event_Click"))
                {
                    csCode.AppendLine($"        private void {ctrl.Name}_Click(object sender, EventArgs e)");
                    csCode.AppendLine("        {");
                    csCode.AppendLine("            // TODO: Implement event handler logic (converted from VB6 code)");
                    csCode.AppendLine("        }");
                    csCode.AppendLine();
                }
            }

            csCode.AppendLine("    }");
            csCode.AppendLine("}");

            // Output the generated C# code to the console.
            Console.WriteLine(csCode.ToString());
        }

        // Helper method to create the proper instantiation string for known WinForms controls.
        static string GetControlInitialization(string csType)
        {
            if (csType == "dynamic")
                return "new Control()";
            else
                return $"new {csType}()";
        }
    }

    // Class to hold information about a VB6 control.
    class ControlInfo
    {
        public string VBType { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

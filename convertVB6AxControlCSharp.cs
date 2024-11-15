using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

static string ConvertVB6FormToCSharp(string vb6FormContent)
{
    // Classes to represent controls and properties
    class ControlInfo
    {
        public string Type { get; set; } // VB6 Control Type
        public string Name { get; set; }
        public string ProgID { get; set; } // ProgID of ActiveX control
        public string WrapperAssembly { get; set; } // Path to generated wrapper DLL
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    List<ControlInfo> controls = new List<ControlInfo>();
    ControlInfo currentControl = null;
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

        if (line.StartsWith("Begin "))
        {
            // Start of a control or form
            int firstSpace = line.IndexOf(' ');
            int secondSpace = line.IndexOf(' ', firstSpace + 1);

            string beginType = line.Substring(6, secondSpace - 6).Trim(); // 'VB.ControlType' or 'Library.ControlType'
            string controlName = line.Substring(secondSpace + 1).Trim();

            // Check if it's a control or the form itself
            if (line.StartsWith("Begin VB."))
            {
                // Standard VB control
                string controlType = beginType.Substring(3); // Remove 'VB.'
                currentControl = new ControlInfo()
                {
                    Type = controlType,
                    Name = controlName,
                    ProgID = "VB." + controlType
                };
            }
            else if (beginType.Contains("."))
            {
                // ActiveX control from a library
                string[] parts = beginType.Split('.');
                string libraryName = parts[0];
                string controlType = parts[1];
                currentControl = new ControlInfo()
                {
                    Type = controlType,
                    Name = controlName,
                    ProgID = beginType
                };
            }
            else
            {
                // Begin Form
                formName = controlName;
            }
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

    // Generate wrapper assemblies for ActiveX controls
    foreach (var control in controls)
    {
        if (!control.ProgID.StartsWith("VB."))
        {
            // ActiveX control, generate wrapper assembly
            string ocxPath = GetOCXPathFromProgID(control.ProgID);
            if (!string.IsNullOrEmpty(ocxPath))
            {
                control.WrapperAssembly = GenerateAxWrapper(ocxPath);
            }
            else
            {
                Console.WriteLine($"Could not find OCX path for ProgID {control.ProgID}");
            }
        }
    }

    // Now generate the C# code
    StringBuilder csharpCode = new StringBuilder();

    csharpCode.AppendLine("using System;");
    csharpCode.AppendLine("using System.Windows.Forms;");
    csharpCode.AppendLine("using System.Runtime.InteropServices;");
    csharpCode.AppendLine();

    // Include namespaces for wrapper assemblies
    foreach (var control in controls)
    {
        if (!string.IsNullOrEmpty(control.WrapperAssembly))
        {
            string namespaceName = Path.GetFileNameWithoutExtension(control.WrapperAssembly);
            csharpCode.AppendLine($"using {namespaceName};");
        }
    }

    csharpCode.AppendLine($"public class {formName} : Form");
    csharpCode.AppendLine("{");

    // Declare controls
    foreach (var control in controls)
    {
        string csharpControlType;
        if (!string.IsNullOrEmpty(control.WrapperAssembly))
        {
            // Use the generated wrapper control type
            string wrapperNamespace = Path.GetFileNameWithoutExtension(control.WrapperAssembly);
            csharpControlType = $"Ax{control.Type}";
            csharpCode.AppendLine($"    private {wrapperNamespace}.{csharpControlType} {control.Name};");
        }
        else
        {
            // Standard control
            csharpControlType = MapVB6ControlToCSharp(control.Type);
            csharpCode.AppendLine($"    private {csharpControlType} {control.Name};");
        }
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
        string csharpControlType;
        if (!string.IsNullOrEmpty(control.WrapperAssembly))
        {
            // Use the generated wrapper control type
            string wrapperNamespace = Path.GetFileNameWithoutExtension(control.WrapperAssembly);
            csharpControlType = $"{wrapperNamespace}.Ax{control.Type}";
            csharpCode.AppendLine($"        this.{control.Name} = new {csharpControlType}();");
            csharpCode.AppendLine($"        ((System.ComponentModel.ISupportInitialize)(this.{control.Name})).BeginInit();");
        }
        else
        {
            // Standard control
            csharpControlType = MapVB6ControlToCSharp(control.Type);
            csharpCode.AppendLine($"        this.{control.Name} = new {csharpControlType}();");
        }

        // Set properties
        foreach (var prop in control.Properties)
        {
            string propName = MapPropertyName(prop.Key);
            string propValue = prop.Value;

            // Handle property types accordingly
            if (IsStringProperty(propName))
            {
                csharpCode.AppendLine($"        this.{control.Name}.{propName} = \"{propValue}\";");
            }
            else if (IsNumericProperty(propName))
            {
                csharpCode.AppendLine($"        this.{control.Name}.{propName} = {propValue};");
            }
            else if (IsPointProperty(propName))
            {
                // Assuming propValue is in VB6 twips, convert to pixels
                int pixels = TwipsToPixels(int.Parse(propValue));
                csharpCode.AppendLine($"        this.{control.Name}.{propName} = {pixels};");
            }
            else
            {
                csharpCode.AppendLine($"        // TODO: Handle property {propName}");
            }
        }

        if (!string.IsNullOrEmpty(control.WrapperAssembly))
        {
            csharpCode.AppendLine($"        ((System.ComponentModel.ISupportInitialize)(this.{control.Name})).EndInit();");
        }

        csharpCode.AppendLine($"        this.Controls.Add(this.{control.Name});");
    }

    csharpCode.AppendLine("    }");
    csharpCode.AppendLine("}");

    return csharpCode.ToString();

    // Method to get OCX path from ProgID
    string GetOCXPathFromProgID(string progID)
    {
        try
        {
            string clsid = Registry.ClassesRoot.OpenSubKey($@"{progID}\CLSID")?.GetValue("") as string;
            if (!string.IsNullOrEmpty(clsid))
            {
                string inprocServer32 = Registry.ClassesRoot.OpenSubKey($@"CLSID\{clsid}\InprocServer32")?.GetValue("") as string;
                return inprocServer32;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving OCX path for ProgID {progID}: {ex.Message}");
        }
        return null;
    }

    // Method to call aximp.exe and generate wrapper assembly
    string GenerateAxWrapper(string ocxPath)
    {
        try
        {
            string aximpPath = GetAxImpPath();
            if (string.IsNullOrEmpty(aximpPath))
            {
                Console.WriteLine("aximp.exe not found.");
                return null;
            }

            string outputDir = Path.GetDirectoryName(ocxPath);
            string outputName = Path.GetFileNameWithoutExtension(ocxPath);
            string outputPath = Path.Combine(outputDir, "Ax" + outputName + ".dll");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = aximpPath,
                Arguments = $"\"{ocxPath}\" /out:\"{outputPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proc = Process.Start(psi))
            {
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                Console.WriteLine(output);

                if (proc.ExitCode == 0 && File.Exists(outputPath))
                {
                    return outputPath;
                }
                else
                {
                    Console.WriteLine($"aximp.exe failed for {ocxPath}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running aximp.exe on {ocxPath}: {ex.Message}");
        }
        return null;
    }

    // Helper method to find aximp.exe
    string GetAxImpPath()
    {
        string[] possiblePaths = new[]
        {
            // .NET Framework SDK paths
            @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\aximp.exe",
            @"C:\Program Files\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\aximp.exe",
            // Visual Studio paths
            @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\SDK\ScopeCppSDK\SDK\bin\NETFX 4.8 Tools\aximp.exe"
            // Add other possible paths as needed
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        // Search in PATH environment variable
        string pathEnv = Environment.GetEnvironmentVariable("PATH");
        foreach (var dir in pathEnv.Split(';'))
        {
            string aximpExe = Path.Combine(dir, "aximp.exe");
            if (File.Exists(aximpExe))
                return aximpExe;
        }

        return null;
    }

    string MapVB6ControlToCSharp(string vb6ControlType)
    {
        // Map standard VB6 controls to C# equivalents
        switch (vb6ControlType)
        {
            case "CommandButton":
                return "Button";
            case "TextBox":
                return "TextBox";
            case "Label":
                return "Label";
            // Add other standard controls as needed
            default:
                return "Control";
        }
    }

    string MapPropertyName(string vb6PropertyName)
    {
        // Map VB6 property names to C# property names
        switch (vb6PropertyName)
        {
            case "Caption":
                return "Text";
            case "Left":
                return "Left";
            case "Top":
                return "Top";
            case "Width":
                return "Width";
            case "Height":
                return "Height";
            // Add other property mappings as needed
            default:
                return vb6PropertyName;
        }
    }

    bool IsStringProperty(string propName)
    {
        // List of properties that are strings
        var stringProps = new HashSet<string> { "Text", "Name" };
        return stringProps.Contains(propName);
    }

    bool IsNumericProperty(string propName)
    {
        // List of properties that are numeric
        var numericProps = new HashSet<string> { "TabIndex" };
        return numericProps.Contains(propName);
    }

    bool IsPointProperty(string propName)
    {
        // Properties that are positions or sizes
        var pointProps = new HashSet<string> { "Left", "Top", "Width", "Height" };
        return pointProps.Contains(propName);
    }

    int TwipsToPixels(int twips)
    {
        // 1 pixel = 15 twips (approximately)
        return twips / 15;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BLML.Phase3FormsUI.FormParsing;
using BLML.Phase3FormsUI.Models;
using Microsoft.Win32;

namespace BLML.Phase3FormsUI.ControlMapping;

public sealed class ActiveXFormCodeGenerator
{
    private readonly Func<string, string?> _ocxPathResolver;
    private readonly Func<string, string?> _wrapperGenerator;

    public ActiveXFormCodeGenerator(Func<string, string?>? ocxPathResolver = null, Func<string, string?>? wrapperGenerator = null)
    {
        _ocxPathResolver = ocxPathResolver ?? TryResolveOcxPath;
        _wrapperGenerator = wrapperGenerator ?? GenerateAxWrapper;
    }

    public string ConvertToCSharp(string vb6FormContent)
    {
        var form = FrmParser.ParseContent(vb6FormContent);
        return ConvertToCSharp(form);
    }

    public string ConvertToCSharp(Vb6FormDefinition form)
    {
        var resolutions = form.GetAllControls().ToDictionary(control => control.Name, ResolveControlResolution, StringComparer.OrdinalIgnoreCase);
        var builder = new StringBuilder();

        builder.AppendLine("using System;");
        builder.AppendLine("using System.Windows.Forms;");

        foreach (var wrapperNamespace in resolutions.Values
                     .Where(resolution => !string.IsNullOrWhiteSpace(resolution.WrapperNamespace))
                     .Select(resolution => resolution.WrapperNamespace!)
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(name => name, StringComparer.Ordinal))
        {
            builder.AppendLine($"using {wrapperNamespace};");
        }

        builder.AppendLine();
        builder.AppendLine($"public class {form.Name} : Form");
        builder.AppendLine("{");

        foreach (var control in form.GetAllControls())
        {
            builder.AppendLine($"    private {resolutions[control.Name].TypeName} {control.Name};");
        }

        builder.AppendLine();
        builder.AppendLine($"    public {form.Name}()");
        builder.AppendLine("    {");
        builder.AppendLine("        InitializeComponent();");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private void InitializeComponent()");
        builder.AppendLine("    {");

        foreach (var control in form.Controls)
        {
            AppendControl(builder, control, resolutions, "this.Controls");
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    public string? TryResolveOcxPath(string progId)
    {
        try
        {
            var clsid = Registry.ClassesRoot.OpenSubKey($@"{progId}\CLSID")?.GetValue(string.Empty) as string;
            if (string.IsNullOrWhiteSpace(clsid))
            {
                return null;
            }

            return Registry.ClassesRoot.OpenSubKey($@"CLSID\{clsid}\InprocServer32")?.GetValue(string.Empty) as string;
        }
        catch
        {
            return null;
        }
    }

    public string? GenerateAxWrapper(string ocxPath)
    {
        var axImpPath = FindAxImpPath();
        if (string.IsNullOrWhiteSpace(axImpPath))
        {
            return null;
        }

        var outputDirectory = Path.GetDirectoryName(ocxPath);
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            return null;
        }

        var outputPath = Path.Combine(outputDirectory, $"Ax{Path.GetFileNameWithoutExtension(ocxPath)}.dll");
        var startInfo = new ProcessStartInfo
        {
            FileName = axImpPath,
            Arguments = $"\"{ocxPath}\" /out:\"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return null;
        }

        process.WaitForExit();
        return process.ExitCode == 0 && File.Exists(outputPath) ? outputPath : null;
    }

    private void AppendControl(StringBuilder builder, Vb6ControlDefinition control, IReadOnlyDictionary<string, ControlResolution> resolutions, string parentControlsExpression)
    {
        var resolution = resolutions[control.Name];
        builder.AppendLine($"        this.{control.Name} = new {resolution.TypeName}();");

        foreach (var property in control.Properties.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            AppendProperty(builder, control, property.Key, property.Value);
        }

        builder.AppendLine($"        {parentControlsExpression}.Add(this.{control.Name});");

        foreach (var child in control.Children)
        {
            AppendControl(builder, child, resolutions, $"this.{control.Name}.Controls");
        }
    }

    private ControlResolution ResolveControlResolution(Vb6ControlDefinition control)
    {
        if (control.Type.StartsWith("VB.", StringComparison.OrdinalIgnoreCase))
        {
            return new ControlResolution(MapStandardControlType(control.Type), null);
        }

        var ocxPath = _ocxPathResolver(control.Type);
        if (!string.IsNullOrWhiteSpace(ocxPath))
        {
            var wrapperPath = _wrapperGenerator(ocxPath);
            if (!string.IsNullOrWhiteSpace(wrapperPath))
            {
                var wrapperNamespace = Path.GetFileNameWithoutExtension(wrapperPath);
                var controlTypeName = control.Type.Split('.').Last();
                return new ControlResolution($"{wrapperNamespace}.Ax{controlTypeName}", wrapperNamespace);
            }
        }

        return new ControlResolution("Control", null);
    }

    private static string MapStandardControlType(string vb6ControlType)
    {
        return vb6ControlType switch
        {
            "VB.CommandButton" => "Button",
            "VB.TextBox" => "TextBox",
            "VB.Label" => "Label",
            "VB.Frame" => "GroupBox",
            _ => "Control"
        };
    }

    private static void AppendProperty(StringBuilder builder, Vb6ControlDefinition control, string propertyName, string propertyValue)
    {
        var mappedProperty = propertyName switch
        {
            "Caption" => "Text",
            _ => propertyName
        };

        var normalizedValue = propertyValue.Trim().Trim('"');
        if (mappedProperty is "Left" or "Top" or "Width" or "Height" or "TabIndex")
        {
            if (int.TryParse(normalizedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
            {
                builder.AppendLine($"        this.{control.Name}.{mappedProperty} = {numericValue / 15};");
            }

            return;
        }

        if (mappedProperty.Equals("Text", StringComparison.OrdinalIgnoreCase))
        {
            builder.AppendLine($"        this.{control.Name}.{mappedProperty} = \"{normalizedValue.Replace("\"", "\\\"", StringComparison.Ordinal)}\";");
        }
    }

    private static string? FindAxImpPath()
    {
        var possiblePaths = new[]
        {
            @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\aximp.exe",
            @"C:\Program Files\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\aximp.exe",
            @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\SDK\ScopeCppSDK\SDK\bin\NETFX 4.8 Tools\aximp.exe"
        };

        return possiblePaths.FirstOrDefault(File.Exists);
    }

    private sealed record ControlResolution(string TypeName, string? WrapperNamespace);
}

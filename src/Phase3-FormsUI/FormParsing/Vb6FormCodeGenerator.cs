using BLML.Phase3FormsUI.Models;
using System.Globalization;
using System.Text;

namespace BLML.Phase3FormsUI.FormParsing;

public static class Vb6FormCodeGenerator
{
    public static string ConvertToCSharp(string vb6FormContent)
    {
        var form = FrmParser.ParseContent(vb6FormContent);
        var builder = new StringBuilder();
        var eventHandlers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        builder.AppendLine("using System;");
        builder.AppendLine("using System.Windows.Forms;");
        builder.AppendLine();
        builder.AppendLine($"public class {form.Name} : Form");
        builder.AppendLine("{");

        foreach (var control in form.GetAllControls())
        {
            builder.AppendLine($"    private {MapControlType(control.Type)} {control.Name};");
        }

        builder.AppendLine();
        builder.AppendLine($"    public {form.Name}()");
        builder.AppendLine("    {");
        builder.AppendLine("        InitializeComponent();");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private void InitializeComponent()");
        builder.AppendLine("    {");

        foreach (var formProperty in form.Properties)
        {
            AppendFormPropertyAssignment(builder, formProperty.Key, formProperty.Value);
        }

        foreach (var control in form.Controls)
        {
            AppendControlInitialization(builder, control, "this.Controls", eventHandlers);
        }

        builder.AppendLine("    }");

        if (eventHandlers.Count > 0)
        {
            builder.AppendLine();

            foreach (var eventHandler in eventHandlers.OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"    private void {eventHandler}(object? sender, EventArgs e)");
                builder.AppendLine("    {");
                builder.AppendLine("    }");
                builder.AppendLine();
            }
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string MapControlType(string vb6ControlType)
    {
        var mappedType = FrmParser.MapToCSharpControlType(vb6ControlType);
        var lastSegmentIndex = mappedType.LastIndexOf(".", StringComparison.Ordinal);
        return lastSegmentIndex >= 0 ? mappedType[(lastSegmentIndex + 1)..] : mappedType;
    }

    private static void AppendControlInitialization(StringBuilder builder, Vb6ControlDefinition control, string parentControlsExpression, ISet<string> eventHandlers)
    {
        var csharpControlType = MapControlType(control.Type);
        builder.AppendLine($"        this.{control.Name} = new {csharpControlType}();");

        foreach (var property in control.Properties.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (TryAppendEventSubscription(builder, control, property.Key, eventHandlers))
            {
                continue;
            }

            AppendPropertyAssignment(builder, control, property.Key, property.Value);
        }

        builder.AppendLine($"        {parentControlsExpression}.Add(this.{control.Name});");

        foreach (var child in control.Children)
        {
            AppendControlInitialization(builder, child, $"this.{control.Name}.Controls", eventHandlers);
        }
    }

    private static void AppendFormPropertyAssignment(StringBuilder builder, string propertyName, string propertyValue)
    {
        var mappedPropertyName = MapPropertyName(propertyName);
        var normalizedValue = propertyValue.Trim().Trim('"');

        if (mappedPropertyName.Equals("Text", StringComparison.OrdinalIgnoreCase))
        {
            builder.AppendLine($"        this.{mappedPropertyName} = \"{Escape(normalizedValue)}\";");
        }
    }

    private static void AppendPropertyAssignment(StringBuilder builder, Vb6ControlDefinition control, string propertyName, string propertyValue)
    {
        var mappedPropertyName = MapPropertyName(propertyName);
        var normalizedValue = propertyValue.Trim().Trim('"');

        switch (mappedPropertyName)
        {
            case "Text":
            case "Name":
                builder.AppendLine($"        this.{control.Name}.{mappedPropertyName} = \"{Escape(normalizedValue)}\";");
                break;
            case "Left":
            case "Top":
            case "Width":
            case "Height":
            case "TabIndex":
                if (int.TryParse(normalizedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
                {
                    builder.AppendLine($"        this.{control.Name}.{mappedPropertyName} = {numericValue};");
                }
                break;
            case "Visible":
                if (TryMapBoolean(normalizedValue, out var booleanValue))
                {
                    builder.AppendLine($"        this.{control.Name}.{mappedPropertyName} = {booleanValue.ToString().ToLowerInvariant()};");
                }
                break;
            default:
                if (propertyValue.StartsWith('"') && propertyValue.EndsWith('"'))
                {
                    builder.AppendLine($"        this.{control.Name}.{mappedPropertyName} = \"{Escape(normalizedValue)}\";");
                }
                else if (!string.IsNullOrWhiteSpace(normalizedValue) && normalizedValue.All(char.IsLetterOrDigit))
                {
                    builder.AppendLine($"        this.{control.Name}.{mappedPropertyName} = {normalizedValue};");
                }
                break;
        }
    }

    private static bool TryAppendEventSubscription(StringBuilder builder, Vb6ControlDefinition control, string propertyName, ISet<string> eventHandlers)
    {
        var eventName = propertyName switch
        {
            "Click" => "Click",
            "DoubleClick" => "DoubleClick",
            "Change" => "TextChanged",
            _ => null
        };

        if (eventName is null)
        {
            return false;
        }

        var handlerName = $"{control.Name}_{eventName}";
        builder.AppendLine($"        this.{control.Name}.{eventName} += this.{handlerName};");
        eventHandlers.Add(handlerName);
        return true;
    }

    private static string MapPropertyName(string vb6PropertyName)
    {
        return vb6PropertyName switch
        {
            "Caption" => "Text",
            _ => vb6PropertyName
        };
    }

    private static bool TryMapBoolean(string value, out bool result)
    {
        if (bool.TryParse(value, out result))
        {
            return true;
        }

        if (value.Equals("0", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        if (value.Equals("-1", StringComparison.OrdinalIgnoreCase) || value.Equals("1", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        result = default;
        return false;
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}

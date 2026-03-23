using BLML.Phase3FormsUI.Models;
using System.Text;
using System.Xml;

namespace BLML.Phase3FormsUI.FormParsing;

/// <summary>
/// Generates .frmx (Form XML) intermediate files from parsed VB6 form definitions.
/// The .frmx format is a structured XML representation that preserves the full
/// form hierarchy, control properties, and layout information from the original .frm file,
/// while mapping VB6 control types to their C#/WinForms equivalents.
/// </summary>
public static class FrmxGenerator
{
    /// <summary>
    /// Generates .frmx XML content from a parsed VB6 form definition.
    /// </summary>
    public static string Generate(Vb6FormDefinition form)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("FormDefinition");
            writer.WriteAttributeString("xmlns", "frmx", null, "urn:blml:frmx:v1");

            WriteFormElement(writer, form);

            writer.WriteEndElement(); // FormDefinition
            writer.WriteEndDocument();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses a .frm file and generates the .frmx XML content.
    /// </summary>
    public static string GenerateFromFile(string inputFilePath)
    {
        var form = FrmParser.ParseFile(inputFilePath);
        return Generate(form);
    }

    /// <summary>
    /// Parses a .frm file and writes the .frmx output to the specified path.
    /// </summary>
    public static void ConvertFile(string inputFilePath, string outputFilePath)
    {
        var frmxContent = GenerateFromFile(inputFilePath);
        File.WriteAllText(outputFilePath, frmxContent);
    }

    private static void WriteFormElement(XmlWriter writer, Vb6FormDefinition form)
    {
        writer.WriteStartElement("Form");
        writer.WriteAttributeString("Name", form.Name);

        // Write form-level properties
        if (form.Properties.Count > 0)
        {
            writer.WriteStartElement("Properties");
            foreach (var property in form.Properties.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                WritePropertyElement(writer, property.Key, property.Value);
            }
            writer.WriteEndElement(); // Properties
        }

        // Write controls
        if (form.Controls.Count > 0)
        {
            writer.WriteStartElement("Controls");
            foreach (var control in form.Controls)
            {
                WriteControlElement(writer, control);
            }
            writer.WriteEndElement(); // Controls
        }

        writer.WriteEndElement(); // Form
    }

    private static void WriteControlElement(XmlWriter writer, Vb6ControlDefinition control)
    {
        writer.WriteStartElement("Control");
        writer.WriteAttributeString("Type", control.Type);
        writer.WriteAttributeString("MappedType", FrmParser.MapToCSharpControlType(control.Type));
        writer.WriteAttributeString("Name", control.Name);

        if (!string.IsNullOrEmpty(control.Guid))
        {
            writer.WriteAttributeString("Guid", control.Guid);
        }

        // Write control properties
        if (control.Properties.Count > 0)
        {
            writer.WriteStartElement("Properties");
            foreach (var property in control.Properties.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                WritePropertyElement(writer, property.Key, property.Value);
            }
            writer.WriteEndElement(); // Properties
        }

        // Write nested child controls
        if (control.Children.Count > 0)
        {
            writer.WriteStartElement("Controls");
            foreach (var child in control.Children)
            {
                WriteControlElement(writer, child);
            }
            writer.WriteEndElement(); // Controls
        }

        writer.WriteEndElement(); // Control
    }

    private static void WritePropertyElement(XmlWriter writer, string name, string value)
    {
        writer.WriteStartElement("Property");
        writer.WriteAttributeString("Name", name);

        // Clean up the value - strip VB6 inline comments like "'True" or "'CenterScreen"
        var cleanValue = value;
        var commentIndex = cleanValue.IndexOf("'", StringComparison.Ordinal);
        if (commentIndex > 0)
        {
            cleanValue = cleanValue[..commentIndex].Trim();
        }

        // Strip surrounding quotes
        if (cleanValue.StartsWith('"') && cleanValue.EndsWith('"') && cleanValue.Length >= 2)
        {
            writer.WriteAttributeString("Type", "String");
            writer.WriteString(cleanValue[1..^1]);
        }
        else if (int.TryParse(cleanValue, out _))
        {
            writer.WriteAttributeString("Type", "Integer");
            writer.WriteString(cleanValue);
        }
        else if (cleanValue.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
        {
            writer.WriteAttributeString("Type", "Hex");
            writer.WriteString(cleanValue);
        }
        else
        {
            writer.WriteAttributeString("Type", "Raw");
            writer.WriteString(cleanValue);
        }

        writer.WriteEndElement(); // Property
    }
}

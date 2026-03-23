using BLML.Phase3FormsUI.Models;
using System.Text;

namespace BLML.Phase3FormsUI.FormParsing;

/// <summary>
/// Orchestrates the full conversion of a VB6 .frm file into its C# equivalents.
/// 
/// A VB6 .frm file contains two sections:
///   1. The form definition (GUI): control hierarchy, properties, layout (between Begin VB.Form and the outer End)
///   2. The code-behind: event handlers, methods, variable declarations (after the form definition)
///
/// This converter produces:
///   - A .frmx file: structured XML intermediate format of the form definition
///   - A .Designer.cs file: WinForms InitializeComponent() with control declarations and layout
///   - The code section is returned separately for the Phase1/Phase2 pipeline to convert
/// </summary>
public static class FormFileConverter
{
    /// <summary>
    /// Result of converting a .frm file.
    /// </summary>
    public sealed class FormConversionResult
    {
        /// <summary>The .frmx XML content.</summary>
        public string FrmxContent { get; set; } = string.Empty;

        /// <summary>The WinForms Designer.cs content (InitializeComponent).</summary>
        public string DesignerCsContent { get; set; } = string.Empty;

        /// <summary>The VB6 code section extracted from the .frm file (everything after the form definition).</summary>
        public string CodeSection { get; set; } = string.Empty;

        /// <summary>The parsed form definition model.</summary>
        public Vb6FormDefinition FormDefinition { get; set; } = new();

        /// <summary>The form name extracted from the definition.</summary>
        public string FormName => FormDefinition.Name;

        /// <summary>Whether the form has any controls.</summary>
        public bool HasControls => FormDefinition.Controls.Count > 0;
    }

    /// <summary>
    /// Converts a .frm file, producing the frmx XML, Designer.cs, and extracted code section.
    /// </summary>
    public static FormConversionResult ConvertFrmContent(string frmContent)
    {
        var result = new FormConversionResult();

        // Parse the form definition (GUI section)
        result.FormDefinition = FrmParser.ParseContent(frmContent);

        // Generate .frmx XML
        result.FrmxContent = FrmxGenerator.Generate(result.FormDefinition);

        // Generate WinForms Designer.cs
        result.DesignerCsContent = Vb6FormCodeGenerator.ConvertToCSharp(frmContent);

        // Extract the code section (everything after the form definition ends)
        result.CodeSection = ExtractCodeSection(frmContent);

        return result;
    }

    /// <summary>
    /// Converts a .frm file and writes all output files to the specified directory.
    /// </summary>
    /// <param name="inputFrmPath">Path to the input .frm file.</param>
    /// <param name="outputDirectory">Directory to write output files.</param>
    /// <returns>The conversion result.</returns>
    public static FormConversionResult ConvertFile(string inputFrmPath, string outputDirectory)
    {
        var frmContent = File.ReadAllText(inputFrmPath);
        var result = ConvertFrmContent(frmContent);
        var baseName = Path.GetFileNameWithoutExtension(inputFrmPath);

        Directory.CreateDirectory(outputDirectory);

        // Write .frmx
        var frmxPath = Path.Combine(outputDirectory, $"{baseName}.frmx");
        File.WriteAllText(frmxPath, result.FrmxContent);

        // Write .Designer.cs
        var designerPath = Path.Combine(outputDirectory, $"{baseName}.Designer.cs");
        File.WriteAllText(designerPath, result.DesignerCsContent);

        return result;
    }

    /// <summary>
    /// Extracts the VB6 code section from a .frm file.
    /// The code section starts after the outermost Begin/End block and the Attribute lines.
    /// </summary>
    public static string ExtractCodeSection(string frmContent)
    {
        var lines = frmContent.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        var codeStartIndex = -1;
        var nestingDepth = 0;
        var foundFormBlock = false;
        var pastAttributes = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();

            if (!foundFormBlock)
            {
                // Look for the outermost Begin VB.Form
                if (trimmed.StartsWith("Begin ", StringComparison.OrdinalIgnoreCase))
                {
                    foundFormBlock = true;
                    nestingDepth = 1;
                }
                continue;
            }

            if (nestingDepth > 0)
            {
                if (trimmed.StartsWith("Begin ", StringComparison.OrdinalIgnoreCase))
                {
                    nestingDepth++;
                }
                else if (trimmed.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    nestingDepth--;
                    if (nestingDepth == 0)
                    {
                        // Form block ended, now skip Attribute lines
                        codeStartIndex = i + 1;
                        continue;
                    }
                }
                continue;
            }

            // We're past the form block - skip Attribute lines and VERSION
            if (codeStartIndex >= 0 && !pastAttributes)
            {
                if (string.IsNullOrWhiteSpace(trimmed) ||
                    trimmed.StartsWith("Attribute ", StringComparison.OrdinalIgnoreCase))
                {
                    codeStartIndex = i + 1;
                    continue;
                }

                pastAttributes = true;
                codeStartIndex = i;
            }
        }

        if (codeStartIndex < 0 || codeStartIndex >= lines.Length)
        {
            return string.Empty;
        }

        var codeLines = lines[codeStartIndex..];
        return string.Join(Environment.NewLine, codeLines);
    }
}

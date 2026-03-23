using System;
using System.IO;
using BLML.Phase3FormsUI.FormParsing;
using BLML.Phase3FormsUI.Models;
using Xunit;

namespace BLML.Tests;

public class Phase3FormConversionTests
{
    private const string SimpleFormFrm = """
        VERSION 5.00
        Begin VB.Form TestForm
           Caption         =   "Test Form Title"
           ClientHeight    =   3000
           ClientWidth     =   5000
           Begin VB.CommandButton cmdSave
              Caption         =   "Save"
              Height          =   495
              Left            =   120
              TabIndex        =   0
              Top             =   120
              Width           =   1215
           End
           Begin VB.TextBox txtName
              Height          =   375
              Left            =   120
              TabIndex        =   1
              Top             =   720
              Width           =   3000
           End
        End
        Attribute VB_Name = "TestForm"
        Option Explicit
        
        Private Sub cmdSave_Click()
            MsgBox txtName.Text
        End Sub
        """;

    private const string NestedFormFrm = """
        VERSION 5.00
        Begin VB.Form frmNested
           Caption         =   "Nested Demo"
           Begin VB.Frame fraOptions
              Caption         =   "Options"
              Height          =   1200
              Left            =   120
              Top             =   120
              Width           =   3000
              Begin VB.CheckBox chkOption1
                 Caption         =   "Option 1"
                 Height          =   255
                 Left            =   120
                 Top             =   360
                 Width           =   1200
              End
              Begin VB.CheckBox chkOption2
                 Caption         =   "Option 2"
                 Height          =   255
                 Left            =   1440
                 Top             =   360
                 Width           =   1200
              End
           End
        End
        Attribute VB_Name = "frmNested"
        """;

    // ========================================
    // FrmxGenerator Tests
    // ========================================

    [Fact]
    public void FrmxGenerator_ShouldGenerateValidXml()
    {
        var form = FrmParser.ParseContent(SimpleFormFrm);
        var frmx = FrmxGenerator.Generate(form);

        Assert.Contains("<?xml", frmx);
        Assert.Contains("<FormDefinition", frmx);
        Assert.Contains("<Form Name=\"TestForm\"", frmx);
    }

    [Fact]
    public void FrmxGenerator_ShouldIncludeFormProperties()
    {
        var form = FrmParser.ParseContent(SimpleFormFrm);
        var frmx = FrmxGenerator.Generate(form);

        Assert.Contains("<Property Name=\"Caption\"", frmx);
        Assert.Contains("Test Form Title", frmx);
        Assert.Contains("<Property Name=\"ClientHeight\"", frmx);
        Assert.Contains("<Property Name=\"ClientWidth\"", frmx);
    }

    [Fact]
    public void FrmxGenerator_ShouldIncludeControls()
    {
        var form = FrmParser.ParseContent(SimpleFormFrm);
        var frmx = FrmxGenerator.Generate(form);

        Assert.Contains("Name=\"cmdSave\"", frmx);
        Assert.Contains("Name=\"txtName\"", frmx);
        Assert.Contains("MappedType=\"System.Windows.Forms.Button\"", frmx);
        Assert.Contains("MappedType=\"System.Windows.Forms.TextBox\"", frmx);
    }

    [Fact]
    public void FrmxGenerator_ShouldIncludeControlProperties()
    {
        var form = FrmParser.ParseContent(SimpleFormFrm);
        var frmx = FrmxGenerator.Generate(form);

        // Check that control properties like Height, Left, etc. are included
        Assert.Contains("<Property Name=\"Height\"", frmx);
        Assert.Contains("<Property Name=\"Left\"", frmx);
        Assert.Contains("<Property Name=\"TabIndex\"", frmx);
    }

    [Fact]
    public void FrmxGenerator_ShouldPreserveNestedControlHierarchy()
    {
        var form = FrmParser.ParseContent(NestedFormFrm);
        var frmx = FrmxGenerator.Generate(form);

        // Frame should contain nested checkboxes
        Assert.Contains("Name=\"fraOptions\"", frmx);
        Assert.Contains("Name=\"chkOption1\"", frmx);
        Assert.Contains("Name=\"chkOption2\"", frmx);
        Assert.Contains("MappedType=\"System.Windows.Forms.GroupBox\"", frmx);
        Assert.Contains("MappedType=\"System.Windows.Forms.CheckBox\"", frmx);
    }

    [Fact]
    public void FrmxGenerator_ShouldCategorizePropertyTypes()
    {
        var form = FrmParser.ParseContent(SimpleFormFrm);
        var frmx = FrmxGenerator.Generate(form);

        // String properties should be typed
        Assert.Contains("Type=\"String\"", frmx);
        // Integer properties should be typed
        Assert.Contains("Type=\"Integer\"", frmx);
    }

    // ========================================
    // FormFileConverter Tests
    // ========================================

    [Fact]
    public void FormFileConverter_ShouldProduceFrmxContent()
    {
        var result = FormFileConverter.ConvertFrmContent(SimpleFormFrm);

        Assert.NotEmpty(result.FrmxContent);
        Assert.Contains("<FormDefinition", result.FrmxContent);
        Assert.Contains("<Form Name=\"TestForm\"", result.FrmxContent);
    }

    [Fact]
    public void FormFileConverter_ShouldProduceDesignerCs()
    {
        var result = FormFileConverter.ConvertFrmContent(SimpleFormFrm);

        Assert.NotEmpty(result.DesignerCsContent);
        Assert.Contains("public class TestForm : Form", result.DesignerCsContent);
        Assert.Contains("private Button cmdSave", result.DesignerCsContent);
        Assert.Contains("private TextBox txtName", result.DesignerCsContent);
        Assert.Contains("InitializeComponent", result.DesignerCsContent);
    }

    [Fact]
    public void FormFileConverter_ShouldExtractCodeSection()
    {
        var result = FormFileConverter.ConvertFrmContent(SimpleFormFrm);

        Assert.NotEmpty(result.CodeSection);
        Assert.Contains("Option Explicit", result.CodeSection);
        Assert.Contains("cmdSave_Click", result.CodeSection);
        Assert.Contains("MsgBox", result.CodeSection);
    }

    [Fact]
    public void FormFileConverter_ShouldNotIncludeFormDefinitionInCodeSection()
    {
        var result = FormFileConverter.ConvertFrmContent(SimpleFormFrm);

        Assert.DoesNotContain("Begin VB.Form", result.CodeSection);
        Assert.DoesNotContain("Begin VB.CommandButton", result.CodeSection);
        Assert.DoesNotContain("Attribute VB_Name", result.CodeSection);
    }

    [Fact]
    public void FormFileConverter_ShouldSetFormName()
    {
        var result = FormFileConverter.ConvertFrmContent(SimpleFormFrm);

        Assert.Equal("TestForm", result.FormName);
    }

    [Fact]
    public void FormFileConverter_ShouldDetectControls()
    {
        var result = FormFileConverter.ConvertFrmContent(SimpleFormFrm);

        Assert.True(result.HasControls);
    }

    [Fact]
    public void FormFileConverter_ShouldHandleFormWithNoCode()
    {
        var formOnly = """
            VERSION 5.00
            Begin VB.Form EmptyForm
               Caption = "Empty"
               Begin VB.Label Label1
                  Caption = "Hello"
               End
            End
            Attribute VB_Name = "EmptyForm"
            """;

        var result = FormFileConverter.ConvertFrmContent(formOnly);

        Assert.NotEmpty(result.FrmxContent);
        Assert.NotEmpty(result.DesignerCsContent);
        Assert.Empty(result.CodeSection.Trim());
    }

    [Fact]
    public void FormFileConverter_ShouldHandleNestedControls()
    {
        var result = FormFileConverter.ConvertFrmContent(NestedFormFrm);

        // frmx should contain nested controls
        Assert.Contains("fraOptions", result.FrmxContent);
        Assert.Contains("chkOption1", result.FrmxContent);
        Assert.Contains("chkOption2", result.FrmxContent);

        // Designer.cs should reference nested controls
        Assert.Contains("fraOptions", result.DesignerCsContent);
        Assert.Contains("chkOption1", result.DesignerCsContent);
    }

    [Fact]
    public void FormFileConverter_ConvertFile_ShouldWriteOutputFiles()
    {
        var tempDir = CreateTempFolder();
        try
        {
            var inputPath = Path.Combine(tempDir, "TestForm.frm");
            File.WriteAllText(inputPath, SimpleFormFrm);

            var outputDir = Path.Combine(tempDir, "output");
            var result = FormFileConverter.ConvertFile(inputPath, outputDir);

            // Verify .frmx was written
            var frmxPath = Path.Combine(outputDir, "TestForm.frmx");
            Assert.True(File.Exists(frmxPath));
            var frmxContent = File.ReadAllText(frmxPath);
            Assert.Contains("<FormDefinition", frmxContent);

            // Verify .Designer.cs was written
            var designerPath = Path.Combine(outputDir, "TestForm.Designer.cs");
            Assert.True(File.Exists(designerPath));
            var designerContent = File.ReadAllText(designerPath);
            Assert.Contains("public class TestForm : Form", designerContent);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // ========================================
    // FrmxGenerator from file round-trip
    // ========================================

    [Fact]
    public void FrmxGenerator_ConvertFile_ShouldWriteValidFrmx()
    {
        var tempDir = CreateTempFolder();
        try
        {
            var inputPath = Path.Combine(tempDir, "RoundTrip.frm");
            File.WriteAllText(inputPath, NestedFormFrm);

            var outputPath = Path.Combine(tempDir, "RoundTrip.frmx");
            FrmxGenerator.ConvertFile(inputPath, outputPath);

            Assert.True(File.Exists(outputPath));
            var content = File.ReadAllText(outputPath);
            Assert.Contains("<FormDefinition", content);
            Assert.Contains("frmNested", content);
            Assert.Contains("fraOptions", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // ========================================
    // ExtractCodeSection edge cases
    // ========================================

    [Fact]
    public void ExtractCodeSection_ShouldHandleMultipleAttributeLines()
    {
        var frm = """
            VERSION 5.00
            Begin VB.Form MyForm
               Caption = "Test"
            End
            Attribute VB_Name = "MyForm"
            Attribute VB_GlobalNameSpace = False
            Attribute VB_Creatable = False
            Attribute VB_PredeclaredId = True
            Attribute VB_Exposed = False
            Option Explicit
            
            Private Sub Form_Load()
                MsgBox "Hello"
            End Sub
            """;

        var code = FormFileConverter.ExtractCodeSection(frm);

        Assert.Contains("Option Explicit", code);
        Assert.Contains("Form_Load", code);
        Assert.DoesNotContain("Attribute", code);
        Assert.DoesNotContain("Begin VB.Form", code);
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

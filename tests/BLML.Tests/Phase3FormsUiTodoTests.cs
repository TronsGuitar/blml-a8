using System;
using System.IO;
using BLML.Phase3FormsUI.ControlMapping;
using BLML.Phase3FormsUI.FormParsing;
using BLML.Phase3FormsUI.Resources;
using Xunit;

namespace BLML.Tests;

public class Phase3FormsUiTodoTests
{
    [Fact]
    public void FrmParser_ParseContent_ShouldPreserveNestedContainerHierarchy()
    {
        var form = FrmParser.ParseContent("""
            VERSION 5.00
            Begin VB.Form NestedForm
               Begin VB.Frame Frame1
                  Caption = "Container"
                  Begin VB.CommandButton Command1
                     Caption = "Run"
                  End
               End
            End
            """);

        var frame = Assert.Single(form.Controls);
        var button = Assert.Single(frame.Children);

        Assert.Equal("NestedForm", form.Name);
        Assert.Equal("VB.Frame", frame.Type);
        Assert.Equal("Frame1", frame.Name);
        Assert.Equal("Container", frame.Properties["Caption"].Trim('"'));
        Assert.Equal("VB.CommandButton", button.Type);
        Assert.Equal("Command1", button.Name);
    }

    [Fact]
    public void Vb6FormCodeGenerator_ConvertToCSharp_ShouldGenerateDesignerFriendlyOutputForContainerForms()
    {
        var code = Vb6FormCodeGenerator.ConvertToCSharp("""
            VERSION 5.00
            Begin VB.Form NestedForm
               Caption = "Nested Form"
               Begin VB.Frame Frame1
                  Caption = "Container"
                  Begin VB.CommandButton Command1
                     Caption = "Run"
                     Click = "[Event Procedure]"
                  End
               End
            End
            """);

        Assert.Contains("private GroupBox Frame1;", code);
        Assert.Contains("private Button Command1;", code);
        Assert.Contains("this.Text = \"Nested Form\";", code);
        Assert.Contains("this.Frame1.Controls.Add(this.Command1);", code);
        Assert.Contains("this.Command1.Click += this.Command1_Click;", code);
        Assert.Contains("private void Command1_Click(object? sender, EventArgs e)", code);
    }

    [Fact]
    public void ActiveXFormCodeGenerator_ConvertToCSharp_ShouldEmitWrapperBackedControlsWhenAxWrapperIsAvailable()
    {
        var generator = new ActiveXFormCodeGenerator(
            ocxPathResolver: progId => progId == "MSComctlLib.TreeView" ? @"C:\Controls\MSCOMCTL.OCX" : null,
            wrapperGenerator: ocxPath => @"C:\Controls\AxInterop.MSComctlLib.dll");

        var code = generator.ConvertToCSharp("""
            VERSION 5.00
            Begin VB.Form ActiveXForm
               Begin MSComctlLib.TreeView TreeView1
                  Left = 150
                  Top = 300
               End
            End
            """);

        Assert.Contains("using AxInterop.MSComctlLib;", code);
        Assert.Contains("private AxInterop.MSComctlLib.AxTreeView TreeView1;", code);
        Assert.Contains("this.TreeView1 = new AxInterop.MSComctlLib.AxTreeView();", code);
        Assert.Contains("this.TreeView1.Left = 10;", code);
        Assert.Contains("this.TreeView1.Top = 20;", code);
    }

    [Fact]
    public void WinFormsTableLayoutConverter_RebuildWithTableLayout_ShouldPreserveComplexLayoutMetadata()
    {
        var converter = new WinFormsTableLayoutConverter();
        var lines = new[]
        {
            "private System.ComponentModel.IContainer components = null;",
            "private System.Windows.Forms.Button button1;",
            "private System.Windows.Forms.TextBox textBox1;",
            "private void InitializeComponent()",
            "{",
            "    this.button1 = new System.Windows.Forms.Button();",
            "    this.textBox1 = new System.Windows.Forms.TextBox();",
            "    this.button1.Anchor = System.Windows.Forms.AnchorStyles.Top;",
            "    this.button1.Location = new System.Drawing.Point(10, 20);",
            "    this.button1.Size = new System.Drawing.Size(100, 30);",
            "    this.textBox1.Location = new System.Drawing.Point(200, 20);",
            "    this.textBox1.Size = new System.Drawing.Size(120, 20);",
            "    this.Controls.Add(this.button1);",
            "    this.Controls.Add(this.textBox1);",
            "    this.ResumeLayout(false);",
            "}"
        };

        var controls = converter.ExtractControls(lines);
        var rebuilt = string.Join("\n", converter.RebuildWithTableLayout(lines, controls));

        Assert.Contains("this.tableLayoutPanelMain.ColumnCount = 2;", rebuilt);
        Assert.Contains("this.tableLayoutPanelMain.RowCount = 1;", rebuilt);
        Assert.Contains("this.tableLayoutPanelMain.Controls.Add(this.button1, 0, 0);", rebuilt);
        Assert.Contains("this.tableLayoutPanelMain.Controls.Add(this.textBox1, 1, 0);", rebuilt);
        Assert.Contains("this.button1.Anchor = System.Windows.Forms.AnchorStyles.Top;", rebuilt);
        Assert.DoesNotContain("this.button1.Location = new System.Drawing.Point(10, 20);", rebuilt);
        Assert.DoesNotContain("this.textBox1.Location = new System.Drawing.Point(200, 20);", rebuilt);
    }

    [Fact]
    public void ResourceExtractor_ShouldExtractBinaryAssetsFromFrxReferences()
    {
        var tempFolder = CreateTempFolder();

        try
        {
            var resourcePath = Path.Combine(tempFolder, "SampleForm.frx");
            File.WriteAllBytes(resourcePath, new byte[] { 0x04, 0x00, 0x00, 0x00, 0x10, 0x20, 0x30, 0x40 });

            var extractor = new ResourceExtractor();
            var bytes = extractor.ExtractBinaryResource(tempFolder, "SampleForm.frx:0000");
            var exportPath = Path.Combine(tempFolder, "resource.bin");
            extractor.ExportBinaryResource(tempFolder, "SampleForm.frx:0000", exportPath);

            Assert.Equal(new byte[] { 0x10, 0x20, 0x30, 0x40 }, bytes);
            Assert.Equal(bytes, File.ReadAllBytes(exportPath));
        }
        finally
        {
            Directory.Delete(tempFolder, recursive: true);
        }
    }

    [Fact(Skip = "TODO: vb6binary.cs is intentionally isolated until a dedicated compatibility project restores its VB runtime dependencies.")]
    public void Vb6BinaryCompatibilityLayer_ShouldSupportBinaryRecordAccessInDedicatedCompatibilityProject()
    {
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

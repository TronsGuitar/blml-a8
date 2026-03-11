using BLML.Phase3FormsUI.ControlMapping;
using BLML.Phase3FormsUI.FormParsing;
using BLML.Phase3FormsUI.Layout;
using BLML.Phase3FormsUI.Models;
using BLML.Phase3FormsUI.Resources;

namespace BLML.Tests;

public class Phase3FormsUiServicesTests
{
    [Fact]
    public void FrmParser_ParseAndConvertToCSharp_ShouldMatchGoldenFile()
    {
        var tempFolder = CreateTempFolder();

        try
        {
            var inputPath = Path.Combine(tempFolder, "SampleForm.frm");
            var outputPath = Path.Combine(tempFolder, "SampleForm.frmx");
            File.Copy(GetTestDataPath("SampleForm.frm"), inputPath);

            FrmParser.ParseAndConvertToCSharp(inputPath, outputPath);

            var expected = File.ReadAllText(GetTestDataPath("SampleForm.expected.frmx"));
            var actual = File.ReadAllText(outputPath);
            Assert.Equal(NormalizeNewLines(expected), NormalizeNewLines(actual));
        }
        finally
        {
            Directory.Delete(tempFolder, recursive: true);
        }
    }

    [Fact]
    public void Vb6FormCodeGenerator_ConvertToCSharp_ShouldMatchGoldenFile()
    {
        var vb6Form = File.ReadAllText(GetTestDataPath("SampleForm.frm"));

        var actual = Vb6FormCodeGenerator.ConvertToCSharp(vb6Form);
        var expected = File.ReadAllText(GetTestDataPath("SampleForm.expected.cs"));

        Assert.Equal(NormalizeNewLines(expected), NormalizeNewLines(actual));
    }

    [Fact]
    public void LayoutConverter_BuildRowPlan_ShouldGroupControlsByTopCoordinate()
    {
        var converter = new LayoutConverter();
        var controls = new[]
        {
            CreateControl("Text1", left: 0, top: 0),
            CreateControl("Button1", left: 300, top: 60),
            CreateControl("Label1", left: 0, top: 500)
        };

        var rows = converter.BuildRowPlan(controls);

        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "Text1", "Button1" }, rows[0].ControlNames);
        Assert.Equal(new[] { "Label1" }, rows[1].ControlNames);
        Assert.Equal(10, converter.ConvertTwipsToPixels(150));
    }

    [Fact]
    public void ResourceExtractor_ShouldParseReferenceAndHexPayload()
    {
        var extractor = new ResourceExtractor();

        var parsed = extractor.TryParseResourceReference("SampleForm.frx:0010", out var reference);
        var payload = extractor.ParseHexPayload("0A 0B 0C 0D");

        Assert.True(parsed);
        Assert.NotNull(reference);
        Assert.Equal("SampleForm.frx", reference!.FileName);
        Assert.Equal(0x10, reference.Offset);
        Assert.Equal(new byte[] { 0x0A, 0x0B, 0x0C, 0x0D }, payload);
    }

    [Fact]
    public void WinFormsTableLayoutConverter_ShouldExtractDesignerControls()
    {
        var converter = new WinFormsTableLayoutConverter();
        var lines = new[]
        {
            "this.button1 = new System.Windows.Forms.Button();",
            "this.button1.Location = new System.Drawing.Point(10, 20);",
            "this.button1.Size = new System.Drawing.Size(100, 30);"
        };

        var controls = converter.ExtractControls(lines);

        var control = Assert.Single(controls);
        Assert.Equal("button1", control.Name);
        Assert.Equal("Button", control.Type);
        Assert.Equal(10, control.X);
        Assert.Equal(20, control.Y);
        Assert.Equal(100, control.Width);
        Assert.Equal(30, control.Height);
    }

    private static Vb6ControlDefinition CreateControl(string name, int left, int top)
    {
        var control = new Vb6ControlDefinition
        {
            Name = name,
            Type = "VB.TextBox"
        };

        control.Properties["Left"] = left.ToString();
        control.Properties["Top"] = top.ToString();
        return control;
    }

    private static string GetTestDataPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Phase3FormsUi", fileName);
    }

    private static string NormalizeNewLines(string value)
    {
        return value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

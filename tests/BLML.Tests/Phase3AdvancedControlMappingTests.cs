using BLML.Phase3FormsUI.FormParsing;
using Xunit;

namespace BLML.Tests;

public class Phase3AdvancedControlMappingTests
{
    [Theory]
    [InlineData("TabDlg.SSTab", "System.Windows.Forms.TabControl")]
    [InlineData("MSComctlLib.TreeView", "System.Windows.Forms.TreeView")]
    [InlineData("MSComctlLib.ListView", "System.Windows.Forms.ListView")]
    [InlineData("MSFlexGridLib.MSFlexGrid", "System.Windows.Forms.DataGridView")]
    [InlineData("MSComDlg.CommonDialog", "System.Windows.Forms.OpenFileDialog")]
    [InlineData("RichTextLib.RichTextBox", "System.Windows.Forms.RichTextBox")]
    public void MapToCSharpControlType_MapsAdvancedControlsToWinFormsEquivalents(string vb6Type, string expectedCSharpType)
    {
        Assert.Equal(expectedCSharpType, FrmParser.MapToCSharpControlType(vb6Type));
    }

    private const string FormWithAdvancedControlsFrm = """
        VERSION 5.00
        Begin VB.Form frmAdvanced
           Caption         =   "Advanced Controls Demo"
           Begin TabDlg.SSTab tabMain
              Height          =   3000
              Left            =   0
              Top             =   0
              Width           =   4000
           End
           Begin MSComctlLib.TreeView tvNav
              Height          =   2000
              Left            =   0
              Top             =   0
              Width           =   1500
           End
           Begin MSComctlLib.ListView lvItems
              Height          =   2000
              Left            =   1500
              Top             =   0
              Width           =   2500
           End
           Begin MSFlexGridLib.MSFlexGrid grdData
              Height          =   1500
              Left            =   0
              Top             =   2000
              Width           =   4000
           End
           Begin RichTextLib.RichTextBox rtfNotes
              Height          =   1000
              Left            =   0
              Top             =   3500
              Width           =   4000
           End
           Begin MSComDlg.CommonDialog dlgFile
              Left            =   3600
              Top             =   4500
           End
        End
        Attribute VB_Name = "frmAdvanced"
        """;

    [Fact]
    public void VB6FormCodeGenerator_ConvertsFormWithAllAdvancedControlsToWinFormsTypes()
    {
        var csharp = Vb6FormCodeGenerator.ConvertToCSharp(FormWithAdvancedControlsFrm);

        Assert.Contains("TabControl tabMain", csharp);
        Assert.Contains("TreeView tvNav", csharp);
        Assert.Contains("ListView lvItems", csharp);
        Assert.Contains("DataGridView grdData", csharp);
        Assert.Contains("RichTextBox rtfNotes", csharp);
        Assert.Contains("OpenFileDialog dlgFile", csharp);

        Assert.Contains("new TabControl()", csharp);
        Assert.Contains("new TreeView()", csharp);
        Assert.Contains("new ListView()", csharp);
        Assert.Contains("new DataGridView()", csharp);
        Assert.Contains("new RichTextBox()", csharp);
        Assert.Contains("new OpenFileDialog()", csharp);
    }

    [Fact]
    public void FrmParser_UnknownControlType_FallsBackToRawTypeRatherThanGuessing()
    {
        // A control type this table doesn't know about should pass through unchanged,
        // not silently map to something plausible-but-wrong.
        Assert.Equal("SomeThirdParty.WeirdControl", FrmParser.MapToCSharpControlType("SomeThirdParty.WeirdControl"));
    }
}

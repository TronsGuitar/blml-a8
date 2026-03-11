using BLML.Phase1Foundation.Parser;

namespace BLML.Tests;

public class Phase6AdvancedFeaturesTests
{
    [Fact]
    public void Parser_ShouldConvertPropertyGetAndLetIntoCSharpProperty()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Private mName As String
Public Property Get Name() As String
    Name = mName
End Property
Public Property Let Name(ByVal Value As String)
    mName = Value
End Property");

        Assert.Empty(result.Errors);
        Assert.Contains("public string Name", result.CSharpCode);
        Assert.Contains("get", result.CSharpCode);
        Assert.Contains("return mName;", result.CSharpCode);
        Assert.Contains("set", result.CSharpCode);
        Assert.Contains("mName = value;", result.CSharpCode);
        Assert.DoesNotContain("public string Name()", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldEmitOptionalDefaultValuesInGeneratedMethodSignatures()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Sub Retry(Optional ByVal attempts As Integer = 3, Optional ByVal userName As String = ""Guest"", Optional ByVal enabled As Boolean = True)
End Sub");

        Assert.Empty(result.Errors);
        Assert.Contains("public void Retry(int attempts = 3, string userName = \"Guest\", bool enabled = true)", result.CSharpCode);
    }
}

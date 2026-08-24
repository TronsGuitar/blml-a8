using BLML.Phase1Foundation.Parser;
using BLML.Phase6Advanced.COM;

namespace BLML.Tests;

public class Phase6LanguageFeaturesTests
{
    [Fact]
    public void Parser_ShouldConvertCreateObjectToLateBoundActivatorCreateInstance()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Sub Run()
    Set app = CreateObject(""Excel.Application"")
End Sub");

        Assert.Empty(result.Errors);
        Assert.Contains("System.Activator.CreateInstance(System.Type.GetTypeFromProgID(\"Excel.Application\"))", result.CSharpCode);
    }

    [Fact]
    public void TypeLibConverter_ConvertsCreateObjectCallToLateBoundExpression()
    {
        var converter = new TypeLibConverter();

        var expr = converter.ConvertCreateObjectCall("Excel.Application");

        Assert.Equal("System.Activator.CreateInstance(System.Type.GetTypeFromProgID(\"Excel.Application\"))", expr.ToString());
    }

    [Fact]
    public void TypeLibConverter_IsCreateObjectCall_MatchesCaseInsensitively()
    {
        Assert.True(TypeLibConverter.IsCreateObjectCall("CreateObject"));
        Assert.True(TypeLibConverter.IsCreateObjectCall("createobject"));
        Assert.False(TypeLibConverter.IsCreateObjectCall("GetObject"));
    }

    [Fact]
    public void Parser_ShouldConvertEnumWithExplicitAndImplicitValues()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Enum Color
    Red
    Green = 5
    Blue
End Enum");

        Assert.Empty(result.Errors);
        Assert.Contains("public enum Color", result.CSharpCode);
        Assert.Contains("Red", result.CSharpCode);
        Assert.Contains("Green = 5", result.CSharpCode);
        Assert.Contains("Blue", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldConvertDeclareFunctionToDllImport()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Declare Function GetTickCount Lib ""kernel32"" () As Long");

        Assert.Empty(result.Errors);
        Assert.Contains("[System.Runtime.InteropServices.DllImport(\"kernel32\")]", result.CSharpCode);
        Assert.Contains("public static extern long GetTickCount()", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldConvertDeclareWithAliasToDllImportEntryPoint()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Private Declare Function ShowWin Lib ""user32"" Alias ""ShowWindow"" (ByVal hwnd As Long, ByVal cmd As Long) As Long");

        Assert.Empty(result.Errors);
        Assert.Contains("DllImport(\"user32\", EntryPoint = \"ShowWindow\")", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldConvertParamArrayToParamsArray()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Sub LogAll(ParamArray values() As Variant)
End Sub");

        Assert.Empty(result.Errors);
        Assert.Contains("params object[] values", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldConvertNamedArgumentsToCSharpNamedArguments()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Sub Run()
    DoWork(count:=5, label:=""x"")
End Sub");

        Assert.Empty(result.Errors);
        Assert.Contains("count: 5", result.CSharpCode);
        Assert.Contains("label: \"x\"", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldExpandWithBlockMemberAccessToExplicitTarget()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Sub Configure()
    With customer
        .Name = ""Alice""
        .Save()
    End With
End Sub");

        Assert.Empty(result.Errors);
        Assert.Contains("customer.Name = \"Alice\"", result.CSharpCode);
        Assert.Contains("customer.Save()", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldCaptureWithTargetOnceWhenItIsNotASimpleIdentifier()
    {
        // GetCustomer() must be evaluated exactly once, not once per `.Member` reference -
        // otherwise a With block around a function call with side effects would silently
        // change behavior (calling it twice instead of once).
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Sub Configure()
    With GetCustomer()
        .Name = ""Alice""
        .Save()
    End With
End Sub");

        Assert.Empty(result.Errors);
        Assert.Contains("var __with0 = GetCustomer();", result.CSharpCode);
        Assert.Contains("__with0.Name = \"Alice\"", result.CSharpCode);
        Assert.Contains("__with0.Save()", result.CSharpCode);
        Assert.DoesNotContain("GetCustomer().Name", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldSupportNestedWithBlocks()
    {
        var parser = new VB6Parser();
        var result = parser.TranspileFile(@"
Public Sub Configure()
    With outer
        .Id = 1
        With inner
            .Id = 2
        End With
    End With
End Sub");

        Assert.Empty(result.Errors);
        Assert.Contains("outer.Id = 1", result.CSharpCode);
        Assert.Contains("inner.Id = 2", result.CSharpCode);
    }
}

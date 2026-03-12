using BLML.Phase1Foundation.Parser;

namespace BLML.Tests;

/// <summary>
/// Tests that exercise the VB6 parser against VB6 keywords used in realistic context,
/// covering control-flow, declarations, error-handling, and logical operators.
/// </summary>
public class VB6KeywordParserTests
{
    // ─── Logical / Boolean Operators ─────────────────────────────────────────

    [Fact]
    public void Parser_ShouldMapAndOperatorToCSharpLogicalAnd()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestAnd(x, y)
    Dim result
    If x > 0 And y > 0 Then
        result = 1
    End If
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("&&", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldMapOrOperatorToCSharpLogicalOr()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestOr(x, y)
    Dim result
    If x > 0 Or y > 0 Then
        result = 1
    End If
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("||", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldMapNotOperatorToCSharpLogicalNot()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestNot(flag)
    Dim result
    If Not flag Then
        result = 1
    End If
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("!", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldHandleCombinedAndOrNotInCondition()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestCombined(a, b, c)
    Dim result
    If a > 0 And b > 0 Or Not c Then
        result = 1
    End If
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("&&", result.CSharpCode);
        Assert.Contains("||", result.CSharpCode);
        Assert.Contains("!", result.CSharpCode);
    }

    // ─── Access Modifiers ────────────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParsePublicSubWithPublicModifier()
    {
        var parser = new VB6Parser();
        var code = @"
Public Sub MyPublicSub()
    Dim x
    x = 1
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("public void MyPublicSub()", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParsePrivateFunctionWithPrivateModifier()
    {
        var parser = new VB6Parser();
        var code = @"
Private Function MyPrivateFn(ByVal n As Integer) As Integer
    MyPrivateFn = n + 1
End Function";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("private", result.CSharpCode);
        Assert.Contains("MyPrivateFn", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParseFriendSubWithFriendModifier()
    {
        var parser = new VB6Parser();
        var code = @"
Friend Sub MyFriendSub()
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("MyFriendSub", result.CSharpCode);
    }

    // ─── Exit Sub / Exit Function ─────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseExitSubAsReturnStatement()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestExitSub(x)
    If x < 0 Then
        Exit Sub
    End If
    x = x + 1
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("return;", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParseExitFunctionAsReturnStatement()
    {
        var parser = new VB6Parser();
        var code = @"
Function TestExitFn(x) As Integer
    If x < 0 Then
        Exit Function
    End If
    TestExitFn = x
End Function";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("return;", result.CSharpCode);
    }

    // ─── Set Statement ────────────────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseSetKeywordForObjectAssignment()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestSet()
    Dim obj
    Set obj = Nothing
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("obj", result.CSharpCode);
    }

    // ─── Call Statement ───────────────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseCallKeywordAsMethodInvocation()
    {
        var parser = new VB6Parser();
        var code = @"
Sub Caller()
    Call DoWork()
End Sub

Sub DoWork()
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("DoWork()", result.CSharpCode);
    }

    // ─── Do…Loop Variants ────────────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseDoLoopUntilAsDoWhileWithNegation()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestDoLoopUntil()
    Dim x
    x = 0
    Do
        x = x + 1
    Loop Until x >= 10
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        // Do…Loop Until x >= 10 → do { } while (!(x >= 10))
        Assert.Contains("do", result.CSharpCode);
        Assert.Contains("while", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParseDoUntilLoopAsNegatedWhile()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestDoUntil()
    Dim x
    x = 0
    Do Until x >= 10
        x = x + 1
    Loop
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("while", result.CSharpCode);
    }

    // ─── For…Next with Step ──────────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseForNextWithPositiveStep()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestForStep()
    Dim i
    Dim total
    total = 0
    For i = 0 To 20 Step 2
        total = total + i
    Next i
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        // Step 2 → i += 2
        Assert.Contains("i += 2", result.CSharpCode);
    }

    // ─── ReDim / ReDim Preserve ──────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseReDimWithoutError()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestReDim()
    Dim arr() As String
    ReDim arr(10)
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Parser_ShouldParseReDimPreserveWithoutError()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestReDimPreserve()
    Dim arr() As Integer
    ReDim arr(5)
    ReDim Preserve arr(10)
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
    }

    // ─── On Error ─────────────────────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseOnErrorResumeNextWithoutError()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestOnErrorResumeNext()
    On Error Resume Next
    Dim x
    x = 1
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Parser_ShouldParseOnErrorGoToZeroWithoutError()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestOnErrorGoTo()
    On Error GoTo 0
    Dim x
    x = 1
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
    }

    // ─── Dim with Typed Declarations ─────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseDimWithIntegerType()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestDimInteger()
    Dim count As Integer
    count = 42
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("int count", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParseDimWithStringType()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestDimString()
    Dim name As String
    name = ""Alice""
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("string name", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParseDimWithBooleanType()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestDimBoolean()
    Dim flag As Boolean
    flag = True
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("bool flag", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParseDimWithDoubleType()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestDimDouble()
    Dim pi As Double
    pi = 3.14
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("double pi", result.CSharpCode);
    }

    // ─── Property Set ─────────────────────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParsePropertySetProcedure()
    {
        var parser = new VB6Parser();
        var code = @"
Private mObj As Object
Public Property Get TheObj() As Object
    Set TheObj = mObj
End Property
Public Property Set TheObj(ByVal Value As Object)
    Set mObj = Value
End Property";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("TheObj", result.CSharpCode);
    }

    // ─── Nested Control Structures ───────────────────────────────────────────

    [Fact]
    public void Parser_ShouldParseNestedForLoops()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestNestedFor()
    Dim i
    Dim j
    Dim result
    result = 0
    For i = 1 To 3
        For j = 1 To 3
            result = result + 1
        Next j
    Next i
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("for (int i = 1; i <= 3; i++)", result.CSharpCode);
        Assert.Contains("for (int j = 1; j <= 3; j++)", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParseIfInsideForLoop()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestIfInFor()
    Dim i
    Dim total
    total = 0
    For i = 1 To 10
        If i > 5 Then
            total = total + i
        End If
    Next i
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("for (int i = 1; i <= 10; i++)", result.CSharpCode);
        Assert.Contains("if (i > 5)", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParseWhileInsideIfBlock()
    {
        var parser = new VB6Parser();
        var code = @"
Sub TestWhileInIf(flag)
    Dim x
    x = 0
    If flag Then
        While x < 5
            x = x + 1
        Wend
    End If
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("if (flag)", result.CSharpCode);
        Assert.Contains("while (x < 5)", result.CSharpCode);
    }

    // ─── Module-level Public/Private Field Declarations ──────────────────────

    [Fact]
    public void Parser_ShouldParsePublicModuleVariableDeclaration()
    {
        var parser = new VB6Parser();
        var code = @"
Public MaxCount As Integer
Public Title As String";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("MaxCount", result.CSharpCode);
        Assert.Contains("Title", result.CSharpCode);
    }

    [Fact]
    public void Parser_ShouldParsePrivateModuleVariableDeclaration()
    {
        var parser = new VB6Parser();
        var code = @"
Private mCounter As Long
Private mName As String";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains("mCounter", result.CSharpCode);
        Assert.Contains("mName", result.CSharpCode);
    }

    // ─── Parameterized: VB6 Typed Dim Declarations → C# Types ───────────────

    [Theory]
    [InlineData("Integer", "int")]
    [InlineData("Long", "long")]
    [InlineData("String", "string")]
    [InlineData("Boolean", "bool")]
    [InlineData("Double", "double")]
    [InlineData("Single", "float")]
    public void Parser_ShouldMapVB6TypeToCSharpType(string vb6Type, string csharpType)
    {
        var parser = new VB6Parser();
        var code = $@"
Sub TestType()
    Dim v As {vb6Type}
End Sub";
        var result = parser.TranspileFile(code);

        Assert.Empty(result.Errors);
        Assert.Contains($"{csharpType} v", result.CSharpCode);
    }

    // ─── Parameterized: Exit keywords produce correct C# ─────────────────────

    [Theory]
    [InlineData("For", "break;")]
    [InlineData("Do", "break;")]
    [InlineData("Sub", "return;")]
    [InlineData("Function", "return;")]
    public void Parser_ShouldMapExitKeywordToCorrectCSharpStatement(string exitKind, string expectedOutput)
    {
        var parser = new VB6Parser();
        string code;
        if (exitKind == "For")
        {
            code = $@"
Sub TestExit()
    Dim i
    For i = 1 To 10
        Exit {exitKind}
    Next i
End Sub";
        }
        else if (exitKind == "Do")
        {
            code = $@"
Sub TestExit()
    Dim x
    x = 0
    Do While x < 10
        Exit {exitKind}
    Loop
End Sub";
        }
        else if (exitKind == "Sub")
        {
            code = $@"
Sub TestExit(x)
    If x < 0 Then
        Exit {exitKind}
    End If
End Sub";
        }
        else
        {
            code = $@"
Function TestExit(x) As Integer
    If x < 0 Then
        Exit {exitKind}
    End If
    TestExit = x
End Function";
        }

        var result = parser.TranspileFile(code);
        Assert.Empty(result.Errors);
        Assert.Contains(expectedOutput, result.CSharpCode);
    }
}

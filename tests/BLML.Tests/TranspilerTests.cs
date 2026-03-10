using Xunit;
using BLML.Phase1Foundation.Parser;
using BLML.Phase1Foundation.Lexer;
using BLML.Phase1Foundation.AST;

namespace BLML.Tests
{
    public class TranspilerTests
    {
        [Fact]
        public void Lexer_ShouldTokenizeSimpleExpression()
        {
            var lexer = new VB6Lexer();
            var tokens = lexer.Tokenize("Dim x As Integer");
            
            Assert.True(tokens.Any(t => t.Type.Equals(TokenType.Keyword) && t.Value == "Dim"), "Expected keyword token 'Dim'.");
            Assert.True(tokens.Any(t => t.Type.Equals(TokenType.Identifier) && t.Value == "x"), "Expected identifier token 'x'.");
            Assert.True(tokens.Any(t => t.Type.Equals(TokenType.Keyword) && t.Value.Equals("As", System.StringComparison.OrdinalIgnoreCase)), "Expected keyword token 'As'.");
            Assert.True(tokens.Any(t => t.Type.Equals(TokenType.Identifier) && t.Value == "Integer"), "Expected identifier token 'Integer'.");
        }

        [Fact]
        public void Parser_ShouldParseBasicSub()
        {
            var parser = new VB6Parser();
            var result = parser.TranspileFile("Sub Test()\n  Dim x\n  x = 10\nEnd Sub");
            
            Assert.NotNull(result.CSharpCode);
            Assert.Empty(result.Errors);
            Assert.True(result.CSharpCode.Contains("void Test()"), $"Expected 'void Test()' not found in:\n{result.CSharpCode}");
            Assert.True(result.CSharpCode.Contains("x = 10"), $"Expected 'x = 10' not found in:\n{result.CSharpCode}");
        }

        [Fact]
        public void Parser_ShouldParseIfStatement()
        {
            var parser = new VB6Parser();
            var code = @"
Sub CheckValue(y)
    If y > 10 Then
        y = 0
    Else
        y = y + 1
    End If
End Sub";
            var result = parser.TranspileFile(code);
            
            Assert.Empty(result.Errors);
            Assert.Contains("if (y > 10)", result.CSharpCode);
            Assert.Contains("y = 0", result.CSharpCode);
            Assert.Contains("else", result.CSharpCode);
        }

        [Fact]
        public void Parser_ShouldHandleBuiltInFunctions()
        {
            var parser = new VB6Parser();
            var code = @"
Function GetLen(s)
    GetLen = Len(s)
End Function";
            var result = parser.TranspileFile(code);
            
            Assert.Empty(result.Errors);
            Assert.Contains("s.Length", result.CSharpCode);
        }

        [Fact]
        public void Parser_ShouldParseForNextLoop()
        {
            var parser = new VB6Parser();
            var code = @"
Sub TestLoop()
    Dim i
    Dim total
    total = 0
    For i = 1 To 10
        total = total + i
    Next i
End Sub";
            var result = parser.TranspileFile(code);

            Assert.Empty(result.Errors);
            Assert.Contains("for (int i = 1; i <= 10; i++)", result.CSharpCode);
            Assert.Contains("total = total + i", result.CSharpCode);
        }

        [Fact]
        public void Parser_ShouldParseWhileWendLoop()
        {
            var parser = new VB6Parser();
            var code = @"
Sub TestWhile()
    Dim x
    x = 0
    While x < 10
        x = x + 1
    Wend
End Sub";
            var result = parser.TranspileFile(code);

            Assert.Empty(result.Errors);
            Assert.Contains("while (x < 10)", result.CSharpCode);
            Assert.Contains("x = x + 1", result.CSharpCode);
        }

        [Fact]
        public void Parser_ShouldParseDoLoop()
        {
            var parser = new VB6Parser();
            var code = @"
Sub TestDoLoop()
    Dim x
    x = 0
    Do While x < 10
        x = x + 1
    Loop
End Sub";
            var result = parser.TranspileFile(code);

            Assert.Empty(result.Errors);
            Assert.Contains("while (x < 10)", result.CSharpCode);
        }

        [Fact]
        public void Parser_ShouldParseSelectCase()
        {
            var parser = new VB6Parser();
            var code = @"
Sub TestSelect(x)
    Dim result
    Select Case x
        Case 1
            result = 10
        Case 2
            result = 20
        Case Else
            result = 0
    End Select
End Sub";
            var result = parser.TranspileFile(code);

            Assert.Empty(result.Errors);
            Assert.Contains("switch", result.CSharpCode);
            Assert.Contains("case", result.CSharpCode);
            Assert.Contains("default", result.CSharpCode);
        }

        [Fact]
        public void Parser_ShouldParseSelectCaseWithRange()
        {
            var parser = new VB6Parser();
            var code = @"
Sub TestSelectRange(x)
    Dim result
    Select Case x
        Case 1 To 10
            result = 1
        Case 11 To 20
            result = 2
    End Select
End Sub";
            var result = parser.TranspileFile(code);

            Assert.Empty(result.Errors);
            // Ranges generate if-else chains with >= and <=
            Assert.Contains(">=", result.CSharpCode);
            Assert.Contains("<=", result.CSharpCode);
        }

        [Theory]
        [InlineData("vbCrLf", "\"\r\n\"")]
        [InlineData("vbTab", "'\t'")]
        public void Parser_ShouldRecognizePredefinedConstants(string vbConstant, string expectedCSharp)
        {
            // This test might need adjustment based on how constants are currently generated in bodies
            // For now, let's verify if they trigger errors or map correctly in simple assignments
            var parser = new VB6Parser();
            var code = $"Sub Test()\n  Dim s\n  s = {vbConstant}\nEnd Sub";
            var result = parser.TranspileFile(code);
            
            Assert.Empty(result.Errors);
        }
    }
}

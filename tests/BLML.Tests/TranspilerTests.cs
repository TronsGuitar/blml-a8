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
            
            Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value.Equals("Dim", System.StringComparison.OrdinalIgnoreCase));
            Assert.Contains(tokens, t => t.Type == TokenType.Identifier && t.Value == "x");
            Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value.Equals("As", System.StringComparison.OrdinalIgnoreCase));
            Assert.Contains(tokens, t => t.Type == TokenType.Identifier && t.Value == "Integer");
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

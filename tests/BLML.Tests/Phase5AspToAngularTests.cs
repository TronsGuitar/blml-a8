using Xunit;
using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;

namespace BLML.Tests
{
    public class Phase5AspToAngularTests
    {
        [Fact]
        public void AspLexer_SplitsHtmlCodeAndOutputExpressionRegions()
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize("<html><% x = 1 %><%= x %></html>");

            Assert.Equal(4, regions.Count);
            Assert.Equal(AspRegionType.Html, regions[0].Type);
            Assert.Equal("<html>", regions[0].Text);
            Assert.Equal(AspRegionType.CodeBlock, regions[1].Type);
            Assert.Equal("x = 1", regions[1].Text);
            Assert.Equal(AspRegionType.OutputExpression, regions[2].Type);
            Assert.Equal(AspRegionType.Html, regions[3].Type);
            Assert.Equal("</html>", regions[3].Text);
        }

        [Fact]
        public void AspLexer_DoesNotTreatPercentGreaterThanInsideStringAsBlockTerminator()
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize("<% s = \"50%>done\" %>");

            var code = Assert.Single(regions.Where(r => r.Type == AspRegionType.CodeBlock));
            Assert.Equal("s = \"50%>done\"", code.Text);
        }

        [Fact]
        public void AspLexer_DetectsDirectiveAndServerComment()
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize("<%@ Language=\"VBScript\" %><%-- a comment --%>");

            Assert.Equal(AspRegionType.Directive, regions[0].Type);
            Assert.Equal(AspRegionType.ServerComment, regions[1].Type);
        }

        [Fact]
        public void AspLexer_DistinguishesIncludeDirectiveFromOrdinaryHtmlComment()
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize("<!--#include file=\"header.asp\"--><!-- just a comment -->");

            Assert.Equal(2, regions.Count);
            Assert.Equal(AspRegionType.Include, regions[0].Type);
            Assert.Equal("header.asp", regions[0].IncludePath);
            Assert.False(regions[0].IncludeIsVirtual);
            Assert.Equal(AspRegionType.Html, regions[1].Type);
        }

        [Fact]
        public void AspParser_NestsHtmlInsideIfElseBlocksInsteadOfFlatteningAfterThem()
        {
            var page = new AspParser().Parse("<% If x = 1 Then %>A<% Else %>B<% End If %>");

            var ifStmt = Assert.IsType<IfStatementNode>(Assert.Single(page.Statements));
            var trueHtml = Assert.IsType<HtmlOutputStatementNode>(Assert.Single(ifStmt.TrueBlock.Statements));
            Assert.Equal("A", trueHtml.Html);
            var elseHtml = Assert.IsType<HtmlOutputStatementNode>(Assert.Single(ifStmt.ElseBlock!.Statements));
            Assert.Equal("B", elseHtml.Html);
        }

        [Fact]
        public void AspParser_ParsesSingleLineIfWithNoEndIf()
        {
            var page = new AspParser().Parse("<% If loggedIn Then Response.Write \"hi\" %>");

            var single = Assert.IsType<SingleLineIfStatementNode>(Assert.Single(page.Statements));
            var call = Assert.IsType<CallStatementNode>(single.ThenStatement);
            var invocation = Assert.IsType<InvocationExpressionNode>(call.Invocation);
            Assert.Single(invocation.Arguments);
        }

        [Fact]
        public void AspParser_ParsesClassicRecordsetLoopWithHtmlNestedInWhileBody()
        {
            // The exact pattern documented in ProjectPlan.md's "Data Binding Patterns" section.
            var asp = "<% While Not rs.EOF %><tr><td><%=rs(\"Name\")%></td></tr><% rs.MoveNext %><% Wend %>";
            var page = new AspParser().Parse(asp);

            var whileStmt = Assert.IsType<WhileStatementNode>(Assert.Single(page.Statements));
            Assert.Contains(whileStmt.Body.Statements, s => s is HtmlOutputStatementNode html && html.Html.Contains("<tr>"));
            Assert.Contains(whileStmt.Body.Statements, s => s is AspOutputExpressionStatementNode);
            Assert.Contains(whileStmt.Body.Statements, s => s is CallStatementNode);
        }

        [Fact]
        public void AspParser_ParsesParenlessStatementCallWithMultipleArguments()
        {
            var page = new AspParser().Parse("<% Response.Write \"hello\", \"world\" %>");

            var call = Assert.IsType<CallStatementNode>(Assert.Single(page.Statements));
            var invocation = Assert.IsType<InvocationExpressionNode>(call.Invocation);
            Assert.Equal(2, invocation.Arguments.Count);
        }

        [Fact]
        public void AspParser_ParsesForEachOverServerVariablesStyleCollection()
        {
            var page = new AspParser().Parse("<% For Each item In items %><%=item%><% Next %>");

            var forEach = Assert.IsType<ForEachStatementNode>(Assert.Single(page.Statements));
            Assert.Equal("item", forEach.LoopVariable);
            Assert.Single(forEach.Body.Statements);
        }

        [Fact]
        public void AspParser_DoesNotRunTogetherStatementsFromSeparateCodeBlocks()
        {
            // Without a synthetic break between adjacent <% %> tags, this would misparse
            // as a parenless call `rs.MoveNext(x)` followed by a stray `= 1`.
            var page = new AspParser().Parse("<% rs.MoveNext %><% x = 1 %>");

            Assert.Equal(2, page.Statements.Count);
            Assert.IsType<CallStatementNode>(page.Statements[0]);
            Assert.IsType<AssignmentNode>(page.Statements[1]);
        }

        [Fact]
        public void AspParser_ParsesDimWithMultipleNames()
        {
            var page = new AspParser().Parse("<% Dim a, b, c %>");

            var group = Assert.IsType<VariableDeclarationGroupNode>(Assert.Single(page.Statements));
            Assert.Equal(new[] { "a", "b", "c" }, group.Declarations.Select(d => d.Name));
        }

        [Fact]
        public void AspParser_ExtractsDirectiveAttributes()
        {
            var page = new AspParser().Parse("<%@ Language=\"VBScript\" CodePage=\"65001\" %>");

            var directive = Assert.Single(page.Directives);
            Assert.Equal("VBScript", directive.Attributes["Language"]);
            Assert.Equal("65001", directive.Attributes["CodePage"]);
        }

        [Fact]
        public void IncludeFileResolver_SplicesFileRelativeInclude()
        {
            var root = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var headerPath = Path.Combine(root, "header.asp");
                File.WriteAllText(headerPath, "<b>Header</b>");
                var mainPath = Path.Combine(root, "main.asp");
                File.WriteAllText(mainPath, "<!--#include file=\"header.asp\"-->Body");

                var page = new AspParser().Parse(File.ReadAllText(mainPath), mainPath, root);

                var html = Assert.IsType<HtmlOutputStatementNode>(Assert.Single(page.Statements));
                Assert.Equal("<b>Header</b>Body", html.Html);
                Assert.Single(page.ResolvedIncludePaths);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void IncludeFileResolver_DetectsCircularIncludes()
        {
            var root = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var aPath = Path.Combine(root, "a.asp");
                var bPath = Path.Combine(root, "b.asp");
                File.WriteAllText(aPath, "<!--#include file=\"b.asp\"-->");
                File.WriteAllText(bPath, "<!--#include file=\"a.asp\"-->");

                var resolver = new IncludeFileResolver(root);
                var result = resolver.ResolveIncludes(File.ReadAllText(aPath), aPath);

                Assert.Contains(result.Warnings, w => w.Contains("Circular include"));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}

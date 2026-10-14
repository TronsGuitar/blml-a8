namespace BLML.Phase5ASPtoAngular.AspParser
{
    /// <summary>
    /// Top-level entry point for classic ASP parsing: lexes a page into regions,
    /// resolves `#include` directives when a file path/app root is available, then
    /// builds one combined statement stream (see <see cref="VBScriptParser"/>) so that
    /// HTML written inside VBScript control-flow blocks lands in the right place in
    /// the resulting AST instead of being flattened after it.
    /// </summary>
    public class AspParser
    {
        private readonly AspLexer _lexer = new();

        public AspPageNode Parse(string content, string? filePath = null, string? applicationRoot = null)
        {
            var page = new AspPageNode { FilePath = filePath ?? string.Empty };
            var finalContent = content;

            if (filePath != null && applicationRoot != null)
            {
                var resolver = new IncludeFileResolver(applicationRoot);
                var resolved = resolver.ResolveIncludes(content, filePath);
                finalContent = resolved.Content;
                page.ResolvedIncludePaths.AddRange(resolved.ResolvedFiles);
                page.ParseWarnings.AddRange(resolved.Warnings);
            }

            var regions = _lexer.Tokenize(finalContent);
            var vbParser = new VBScriptParser();
            var stream = new List<AspStreamItem>();

            foreach (var region in regions)
            {
                switch (region.Type)
                {
                    case AspRegionType.Directive:
                        page.Directives.Add(ParseDirective(region.Text));
                        break;

                    case AspRegionType.ServerComment:
                        // Server-side comments never reach the output; nothing to add to the stream.
                        break;

                    case AspRegionType.Html:
                        if (region.Text.Length > 0)
                        {
                            stream.Add(AspStreamItem.ForLeaf(new HtmlOutputStatementNode { Html = region.Text }));
                        }
                        break;

                    case AspRegionType.OutputExpression:
                        var exprNode = vbParser.ParseExpressionText(region.Text);
                        stream.Add(AspStreamItem.ForLeaf(new AspOutputExpressionStatementNode
                        {
                            Expression = exprNode,
                            RawExpression = region.Text
                        }));
                        break;

                    case AspRegionType.CodeBlock:
                        AppendCodeTokens(stream, region.Text);
                        break;

                    case AspRegionType.Include:
                        // Only reachable when Parse() was called without filePath/applicationRoot
                        // (so includes couldn't be resolved) - fall back to treating it as inert text.
                        page.ParseWarnings.Add($"Include '{region.IncludePath}' left unresolved (no application root supplied).");
                        break;
                }
            }

            page.Statements.AddRange(vbParser.ParseProgram(stream));
            page.ParseWarnings.AddRange(vbParser.Warnings);
            return page;
        }

        private static void AppendCodeTokens(List<AspStreamItem> stream, string codeText)
        {
            var tokens = new VbScriptTokenizer().Tokenize(codeText);
            foreach (var t in tokens)
            {
                if (t.Kind == VbsTokenKind.EndOfFile) continue;
                stream.Add(AspStreamItem.ForToken(t));
            }
            // A `%>...<%` tag boundary is always at least as strong a statement break as a
            // physical newline (see class remarks) - without this, `<% rs.MoveNext %><% x = 1 %>`
            // would misparse as a parenless call to MoveNext with `x` as its argument.
            stream.Add(AspStreamItem.ForToken(new VbsToken { Kind = VbsTokenKind.NewLine, Value = "\n" }));
        }

        private static AspDirective ParseDirective(string text)
        {
            var directive = new AspDirective();
            foreach (System.Text.RegularExpressions.Match m in
                     System.Text.RegularExpressions.Regex.Matches(text, "(\\w+)\\s*=\\s*\"([^\"]*)\""))
            {
                directive.Attributes[m.Groups[1].Value] = m.Groups[2].Value;
            }
            return directive;
        }
    }
}

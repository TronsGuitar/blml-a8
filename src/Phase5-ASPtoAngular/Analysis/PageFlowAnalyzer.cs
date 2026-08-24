using System.Text.RegularExpressions;
using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;

namespace BLML.Phase5ASPtoAngular.Analysis
{
    public enum PageFlowTrigger { Redirect, ServerTransfer, ServerExecute, FormSubmit, Link }

    public class PageFlowEdge
    {
        public string FromPage { get; set; } = string.Empty;
        public string ToPage { get; set; } = string.Empty;
        public PageFlowTrigger Trigger { get; set; }
        /// <summary>GET for a link/GET form, POST for a POST form; null for server-side redirects.</summary>
        public string? HttpMethod { get; set; }
    }

    /// <summary>
    /// Maps how a page reaches other pages: server-side navigation
    /// (Response.Redirect/Server.Transfer/Server.Execute, found by walking the parsed
    /// statement tree) plus client-side navigation baked into the emitted HTML
    /// (`&lt;form action=...&gt;`, `&lt;a href=...&gt;`, matched with a regex over each
    /// HtmlOutputStatementNode's raw text since that markup was never itself parsed as
    /// VBScript). This becomes RoutingGenerator's page graph.
    /// </summary>
    public class PageFlowAnalyzer
    {
        private static readonly Regex FormRegex = new(
            "<form\\b[^>]*\\baction\\s*=\\s*[\"']([^\"']+)[\"'][^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FormMethodRegex = new(
            "\\bmethod\\s*=\\s*[\"']([^\"']+)[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex LinkRegex = new(
            "<a\\b[^>]*\\bhref\\s*=\\s*[\"']([^\"'#][^\"']*)[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public List<PageFlowEdge> Analyze(IEnumerable<StatementNode> statements, string currentPageName)
        {
            var edges = new List<PageFlowEdge>();
            WalkStatements(statements, currentPageName, edges);
            WalkHtml(statements, currentPageName, edges);
            return edges;
        }

        private void WalkStatements(IEnumerable<StatementNode> statements, string page, List<PageFlowEdge> edges)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case CallStatementNode call:
                        TryAddServerNavigation(call.Invocation, page, edges);
                        break;
                    case IfStatementNode ifStmt:
                        WalkStatements(ifStmt.TrueBlock.Statements, page, edges);
                        if (ifStmt.ElseBlock != null) WalkStatements(ifStmt.ElseBlock.Statements, page, edges);
                        break;
                    case SingleLineIfStatementNode single:
                        WalkStatements(new[] { single.ThenStatement }, page, edges);
                        if (single.ElseStatement != null) WalkStatements(new[] { single.ElseStatement }, page, edges);
                        break;
                    case WhileStatementNode whileStmt: WalkStatements(whileStmt.Body.Statements, page, edges); break;
                    case DoLoopStatementNode doLoop: WalkStatements(doLoop.Body.Statements, page, edges); break;
                    case ForStatementNode forStmt: WalkStatements(forStmt.Body.Statements, page, edges); break;
                    case ForEachStatementNode forEach: WalkStatements(forEach.Body.Statements, page, edges); break;
                    case SelectCaseStatementNode select:
                        foreach (var c in select.Cases) WalkStatements(c.Body.Statements, page, edges);
                        if (select.CaseElseBlock != null) WalkStatements(select.CaseElseBlock.Statements, page, edges);
                        break;
                }
            }
        }

        private void TryAddServerNavigation(ExpressionNode invocationExpr, string page, List<PageFlowEdge> edges)
        {
            if (invocationExpr is not InvocationExpressionNode inv) return;
            if (inv.Target is not BinaryExpressionNode { Operator: "." } member) return;
            if (member.Left is not IdentifierExpressionNode owner) return;
            if (member.Right is not IdentifierExpressionNode method) return;
            if (inv.Arguments.Count == 0 || inv.Arguments[0] is not LiteralExpressionNode { Value: string target }) return;

            PageFlowTrigger? trigger = (owner.Name.ToLowerInvariant(), method.Name.ToLowerInvariant()) switch
            {
                ("response", "redirect") => PageFlowTrigger.Redirect,
                ("server", "transfer") => PageFlowTrigger.ServerTransfer,
                ("server", "execute") => PageFlowTrigger.ServerExecute,
                _ => null
            };
            if (trigger is null) return;

            edges.Add(new PageFlowEdge { FromPage = page, ToPage = StripQueryString(target), Trigger = trigger.Value });
        }

        private void WalkHtml(IEnumerable<StatementNode> statements, string page, List<PageFlowEdge> edges)
        {
            foreach (var stmt in Flatten(statements))
            {
                if (stmt is not HtmlOutputStatementNode html) continue;

                foreach (Match m in FormRegex.Matches(html.Html))
                {
                    var methodMatch = FormMethodRegex.Match(m.Value);
                    var httpMethod = methodMatch.Success ? methodMatch.Groups[1].Value.ToUpperInvariant() : "GET";
                    edges.Add(new PageFlowEdge { FromPage = page, ToPage = StripQueryString(m.Groups[1].Value), Trigger = PageFlowTrigger.FormSubmit, HttpMethod = httpMethod });
                }

                foreach (Match m in LinkRegex.Matches(html.Html))
                {
                    var href = m.Groups[1].Value;
                    if (href.Contains("://") || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) continue;
                    edges.Add(new PageFlowEdge { FromPage = page, ToPage = StripQueryString(href), Trigger = PageFlowTrigger.Link, HttpMethod = "GET" });
                }
            }
        }

        /// <summary>Depth-first flatten so html buried in If/While/etc. bodies is still scanned.</summary>
        private static IEnumerable<StatementNode> Flatten(IEnumerable<StatementNode> statements)
        {
            foreach (var stmt in statements)
            {
                yield return stmt;
                IEnumerable<StatementNode>? nested = stmt switch
                {
                    IfStatementNode i => i.ElseBlock is null ? i.TrueBlock.Statements : i.TrueBlock.Statements.Concat(i.ElseBlock.Statements),
                    SingleLineIfStatementNode s => s.ElseStatement is null ? new[] { s.ThenStatement } : new[] { s.ThenStatement, s.ElseStatement },
                    WhileStatementNode w => w.Body.Statements,
                    DoLoopStatementNode d => d.Body.Statements,
                    ForStatementNode f => f.Body.Statements,
                    ForEachStatementNode fe => fe.Body.Statements,
                    SelectCaseStatementNode sc => sc.Cases.SelectMany(c => c.Body.Statements).Concat(sc.CaseElseBlock?.Statements ?? Enumerable.Empty<StatementNode>()),
                    _ => null
                };
                if (nested != null) foreach (var n in Flatten(nested)) yield return n;
            }
        }

        private static string StripQueryString(string url)
        {
            var idx = url.IndexOf('?');
            return idx >= 0 ? url[..idx] : url;
        }
    }
}

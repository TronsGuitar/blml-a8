using BLML.Phase1Foundation.AST;

namespace BLML.Phase5ASPtoAngular.AspParser
{
    /// <summary>
    /// A run of literal HTML/text output. In classic ASP, HTML sitting between
    /// `%&gt;` and the next `&lt;%` is implicitly a Response.Write call, including when it sits
    /// *inside* a VBScript block (e.g. `&lt;% If x Then %&gt;html&lt;% End If %&gt;`) - the html
    /// belongs inside the If's body, not after it. AspParser slots these nodes into
    /// whatever block body is currently open so that ambiguity resolves correctly.
    /// </summary>
    public class HtmlOutputStatementNode : StatementNode
    {
        public string Html { get; set; } = string.Empty;
    }

    /// <summary>An inline `&lt;%= expr %&gt;` output expression (shorthand for Response.Write(expr)).</summary>
    public class AspOutputExpressionStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; set; } = null!;
        public string RawExpression { get; set; } = string.Empty;
    }

    /// <summary>A `Call` statement or bare invocation used as a statement (e.g. `rs.MoveNext`, `Response.Write x`).</summary>
    public class CallStatementNode : StatementNode
    {
        public ExpressionNode Invocation { get; set; } = null!;
    }

    /// <summary>VBScript `Dim a, b(5), c` / `Const x = 1, y = 2` declares several names in one statement.</summary>
    public class VariableDeclarationGroupNode : StatementNode
    {
        public List<VariableDeclarationNode> Declarations { get; } = new();
    }

    public class ForEachStatementNode : StatementNode
    {
        public string LoopVariable { get; set; } = string.Empty;
        public ExpressionNode Collection { get; set; } = null!;
        public BlockNode Body { get; set; } = new BlockNode();
    }

    public class SingleLineIfStatementNode : StatementNode
    {
        public ExpressionNode Condition { get; set; } = null!;
        public StatementNode ThenStatement { get; set; } = null!;
        public StatementNode? ElseStatement { get; set; }
    }

    /// <summary>
    /// `&lt;%@ Language="VBScript" CodePage="65001" %&gt;` style page directive.
    /// Must appear before any other output to be valid ASP, but real-world pages
    /// are inconsistent about this - the parser records it wherever it's found.
    /// </summary>
    public class AspDirective
    {
        public Dictionary<string, string> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>One `Session("x") = ...` or `... = Session("x")` reference site, with the ambiguous-null-check pattern (if any) noted.</summary>
    public enum AspIntrinsicCollection
    {
        Session,
        Application,
        Request,
        RequestForm,
        RequestQueryString,
        RequestCookies,
        RequestServerVariables,
        Response
    }

    /// <summary>
    /// A parsed classic ASP page: directives + a flat statement stream where HTML,
    /// `&lt;%= %&gt;` output, and VBScript control flow are interleaved in source order and
    /// properly nested (If/For/etc. bodies contain the HTML that was physically
    /// written inside them in the .asp file).
    /// </summary>
    public class AspPageNode
    {
        public string FilePath { get; set; } = string.Empty;
        public List<AspDirective> Directives { get; } = new();
        public List<StatementNode> Statements { get; } = new();
        public List<string> ResolvedIncludePaths { get; } = new();
        public List<string> ParseWarnings { get; } = new();
    }
}

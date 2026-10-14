using System.Text.RegularExpressions;
using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;

namespace BLML.Phase5ASPtoAngular.Analysis
{
    public enum AdoObjectKind { Connection, Recordset, Command, Unknown }

    public class DatabaseCallSite
    {
        /// <summary>Reconstructed SQL text: literal fragments verbatim, non-literal operands rendered as `?` in order.</summary>
        public string SqlText { get; set; } = string.Empty;
        /// <summary>True when the SQL was assembled by string concatenation with a non-literal operand - a SQL-injection risk unless parameterized.</summary>
        public bool BuiltByUnsafeConcatenation { get; set; }
        /// <summary>Best-effort source text of each `?` placeholder in <see cref="SqlText"/>, in order - what ServiceGenerator binds as SqlParameters.</summary>
        public List<string> ConcatenatedParameterExpressions { get; } = new();
        public List<string> TablesReferenced { get; } = new();
        public StatementNode Statement { get; set; } = null!;
    }

    public class AdoObjectInfo
    {
        public string VariableName { get; set; } = string.Empty;
        public AdoObjectKind Kind { get; set; }
        public List<DatabaseCallSite> CallSites { get; } = new();
    }

    /// <summary>
    /// Finds ADO usage in a parsed page: `Server.CreateObject("ADODB.*")` object
    /// creation, `.Open`/`.Execute`/`.CommandText =` call sites, and reconstructs the
    /// SQL each site runs well enough to (a) name the tables involved and (b) flag the
    /// single most common classic-ASP data-access anti-pattern: building a SQL string
    /// by concatenating raw user input instead of using parameters. That flag is the
    /// signal ServiceGenerator uses to always emit parameterized commands regardless
    /// of what the original code did.
    /// </summary>
    public class DatabaseCallAnalyzer
    {
        private static readonly Regex TableRegex = new(
            @"\b(?:FROM|INTO|UPDATE|JOIN)\s+\[?([A-Za-z_][A-Za-z0-9_]*)\]?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public List<AdoObjectInfo> Analyze(IEnumerable<StatementNode> statements)
        {
            var objects = new Dictionary<string, AdoObjectInfo>(StringComparer.OrdinalIgnoreCase);
            Walk(statements, objects);
            return objects.Values.ToList();
        }

        /// <summary>
        /// Finds every `rsVar("FieldName")` read for the given recordset variable, in
        /// source order with duplicates removed - this is what drives DtoGenerator's
        /// best-effort field list, since classic ASP has no schema to read the shape
        /// from directly.
        /// </summary>
        public List<string> FindFieldReferences(IEnumerable<StatementNode> statements, string recordsetVariable)
        {
            var fields = new List<string>();
            foreach (var expr in EnumerateExpressions(statements))
            {
                if (expr is InvocationExpressionNode inv
                    && inv.Target is IdentifierExpressionNode id
                    && string.Equals(id.Name, recordsetVariable, StringComparison.OrdinalIgnoreCase)
                    && inv.Arguments.Count == 1
                    && inv.Arguments[0] is LiteralExpressionNode { Value: string field }
                    && !fields.Contains(field, StringComparer.OrdinalIgnoreCase))
                {
                    fields.Add(field);
                }
            }
            return fields;
        }

        private static IEnumerable<ExpressionNode> EnumerateExpressions(IEnumerable<StatementNode> statements)
        {
            foreach (var stmt in statements)
            {
                IEnumerable<ExpressionNode> here = stmt switch
                {
                    AssignmentNode a => new[] { a.Target, a.Value },
                    CallStatementNode c => new[] { c.Invocation },
                    AspOutputExpressionStatementNode o => new[] { o.Expression },
                    IfStatementNode i => new[] { i.Condition },
                    SingleLineIfStatementNode s => new[] { s.Condition },
                    WhileStatementNode w => new[] { w.Condition },
                    _ => Enumerable.Empty<ExpressionNode>()
                };
                foreach (var e in here)
                    foreach (var sub in EnumerateSubExpressions(e))
                        yield return sub;

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
                if (nested != null)
                    foreach (var e in EnumerateExpressions(nested))
                        yield return e;
            }
        }

        private static IEnumerable<ExpressionNode> EnumerateSubExpressions(ExpressionNode expr)
        {
            yield return expr;
            switch (expr)
            {
                case BinaryExpressionNode bin:
                    if (bin.Left != null) foreach (var e in EnumerateSubExpressions(bin.Left)) yield return e;
                    if (bin.Right != null) foreach (var e in EnumerateSubExpressions(bin.Right)) yield return e;
                    break;
                case InvocationExpressionNode inv:
                    foreach (var e in EnumerateSubExpressions(inv.Target)) yield return e;
                    foreach (var arg in inv.Arguments)
                        foreach (var e in EnumerateSubExpressions(arg)) yield return e;
                    break;
            }
        }

        private void Walk(IEnumerable<StatementNode> statements, Dictionary<string, AdoObjectInfo> objects)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case AssignmentNode assign:
                        HandleAssignment(assign, objects);
                        break;
                    case CallStatementNode call:
                        HandleCall(call.Invocation, call, objects);
                        break;
                    case IfStatementNode ifStmt:
                        Walk(ifStmt.TrueBlock.Statements, objects);
                        if (ifStmt.ElseBlock != null) Walk(ifStmt.ElseBlock.Statements, objects);
                        break;
                    case SingleLineIfStatementNode single:
                        Walk(new[] { single.ThenStatement }, objects);
                        if (single.ElseStatement != null) Walk(new[] { single.ElseStatement }, objects);
                        break;
                    case WhileStatementNode whileStmt: Walk(whileStmt.Body.Statements, objects); break;
                    case DoLoopStatementNode doLoop: Walk(doLoop.Body.Statements, objects); break;
                    case ForStatementNode forStmt: Walk(forStmt.Body.Statements, objects); break;
                    case ForEachStatementNode forEach: Walk(forEach.Body.Statements, objects); break;
                    case SelectCaseStatementNode select:
                        foreach (var c in select.Cases) Walk(c.Body.Statements, objects);
                        if (select.CaseElseBlock != null) Walk(select.CaseElseBlock.Statements, objects);
                        break;
                }
            }
        }

        private void HandleAssignment(AssignmentNode assign, Dictionary<string, AdoObjectInfo> objects)
        {
            var targetName = (assign.Target as IdentifierExpressionNode)?.Name;

            if (targetName != null && TryGetCreateObjectProgId(assign.Value, out var progId))
            {
                objects[targetName] = new AdoObjectInfo { VariableName = targetName, Kind = ClassifyProgId(progId!) };
                return;
            }

            // rs.CommandText = "SELECT ..." / cmd.CommandText = "..."
            if (assign.Target is BinaryExpressionNode { Operator: "." } member
                && member.Left is IdentifierExpressionNode ownerId
                && member.Right is IdentifierExpressionNode prop
                && string.Equals(prop.Name, "CommandText", StringComparison.OrdinalIgnoreCase)
                && objects.TryGetValue(ownerId.Name, out var owner))
            {
                owner.CallSites.Add(BuildCallSite(assign.Value, assign));
            }
        }

        private void HandleCall(ExpressionNode invocationExpr, StatementNode statement, Dictionary<string, AdoObjectInfo> objects)
        {
            if (invocationExpr is not InvocationExpressionNode inv) return;
            if (inv.Target is not BinaryExpressionNode { Operator: "." } member) return;
            if (member.Left is not IdentifierExpressionNode ownerId) return;
            if (member.Right is not IdentifierExpressionNode method) return;
            if (!objects.TryGetValue(ownerId.Name, out var owner)) return;

            if (string.Equals(method.Name, "Open", StringComparison.OrdinalIgnoreCase)
                || string.Equals(method.Name, "Execute", StringComparison.OrdinalIgnoreCase))
            {
                if (inv.Arguments.Count > 0)
                {
                    owner.CallSites.Add(BuildCallSite(inv.Arguments[0], statement));
                }
            }
        }

        private static DatabaseCallSite BuildCallSite(ExpressionNode sqlExpr, StatementNode statement)
        {
            var site = new DatabaseCallSite { Statement = statement };
            var (text, unsafeConcat) = ReconstructSql(sqlExpr, site.ConcatenatedParameterExpressions);
            site.SqlText = text;
            site.BuiltByUnsafeConcatenation = unsafeConcat;
            foreach (Match m in TableRegex.Matches(text))
            {
                var table = m.Groups[1].Value;
                if (!site.TablesReferenced.Contains(table, StringComparer.OrdinalIgnoreCase)) site.TablesReferenced.Add(table);
            }
            return site;
        }

        /// <summary>
        /// Rebuilds a best-effort SQL string from a `&amp;`-concatenation expression tree,
        /// substituting `?` for any non-literal operand (a variable, a function call,
        /// etc.) and reporting whether such a substitution was needed at all - that's
        /// the "was this built unsafely" signal.
        /// </summary>
        private static (string text, bool unsafeConcat) ReconstructSql(ExpressionNode expr, List<string> parameterExpressions)
        {
            switch (expr)
            {
                case LiteralExpressionNode { Value: string s }:
                    return (s, false);
                case BinaryExpressionNode { Operator: "&" } bin:
                    var (leftText, leftUnsafe) = ReconstructSql(bin.Left, parameterExpressions);
                    var (rightText, rightUnsafe) = ReconstructSql(bin.Right, parameterExpressions);
                    return (leftText + rightText, leftUnsafe || rightUnsafe);
                default:
                    parameterExpressions.Add(DescribeExpression(expr));
                    return ("?", true);
            }
        }

        private static string DescribeExpression(ExpressionNode expr) => expr switch
        {
            IdentifierExpressionNode id => id.Name,
            LiteralExpressionNode lit => lit.Value?.ToString() ?? "",
            BinaryExpressionNode { Operator: "." } member => $"{DescribeExpression(member.Left)}.{DescribeExpression(member.Right)}",
            InvocationExpressionNode inv => $"{DescribeExpression(inv.Target)}({string.Join(",", inv.Arguments.Select(DescribeExpression))})",
            _ => "expr"
        };

        private static bool TryGetCreateObjectProgId(ExpressionNode expr, out string? progId)
        {
            progId = null;
            if (expr is not InvocationExpressionNode inv) return false;
            if (inv.Target is not BinaryExpressionNode { Operator: "." } member) return false;
            if (member.Left is not IdentifierExpressionNode server || !string.Equals(server.Name, "Server", StringComparison.OrdinalIgnoreCase)) return false;
            if (member.Right is not IdentifierExpressionNode method || !string.Equals(method.Name, "CreateObject", StringComparison.OrdinalIgnoreCase)) return false;
            if (inv.Arguments.Count != 1 || inv.Arguments[0] is not LiteralExpressionNode { Value: string s }) return false;
            progId = s;
            return true;
        }

        private static AdoObjectKind ClassifyProgId(string progId) => progId.ToLowerInvariant() switch
        {
            "adodb.connection" => AdoObjectKind.Connection,
            "adodb.recordset" => AdoObjectKind.Recordset,
            "adodb.command" => AdoObjectKind.Command,
            _ => AdoObjectKind.Unknown
        };
    }
}

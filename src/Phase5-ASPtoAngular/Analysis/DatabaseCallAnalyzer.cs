using System.Text.RegularExpressions;
using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;

namespace BLML.Phase5ASPtoAngular.Analysis
{
    public enum AdoObjectKind { Connection, Recordset, Command, Unknown }

    public class DatabaseCallSite
    {
        /// <summary>Reconstructed SQL text: literal fragments verbatim, non-literal operands rendered as `?`.</summary>
        public string SqlText { get; set; } = string.Empty;
        /// <summary>True when the SQL was assembled by string concatenation with a non-literal operand - a SQL-injection risk unless parameterized.</summary>
        public bool BuiltByUnsafeConcatenation { get; set; }
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
            var (text, unsafeConcat) = ReconstructSql(sqlExpr);
            var site = new DatabaseCallSite { SqlText = text, BuiltByUnsafeConcatenation = unsafeConcat, Statement = statement };
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
        private static (string text, bool unsafeConcat) ReconstructSql(ExpressionNode expr)
        {
            switch (expr)
            {
                case LiteralExpressionNode { Value: string s }:
                    return (s, false);
                case BinaryExpressionNode { Operator: "&" } bin:
                    var (leftText, leftUnsafe) = ReconstructSql(bin.Left);
                    var (rightText, rightUnsafe) = ReconstructSql(bin.Right);
                    return (leftText + rightText, leftUnsafe || rightUnsafe);
                default:
                    return ("?", true);
            }
        }

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

using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;

namespace BLML.Phase5ASPtoAngular.Analysis
{
    public enum SessionAccessMode { Read, Write }

    /// <summary>
    /// The three idioms real ASP code uses interchangeably (and inconsistently within
    /// the same codebase) to ask "is this session variable unset" - genuinely
    /// ambiguous in classic ASP because Session("x") on an unset key returns Empty,
    /// which compares equal to both "" and vbNull-ish checks depending on how you ask.
    /// Recording which idiom a site used lets AuthConverter normalize them all to one
    /// C#/JWT-claim null check instead of guessing.
    /// </summary>
    public enum SessionNullCheckIdiom { EqualsEmptyString, IsEmptyCall, IsNothingComparison }

    public class SessionAccessSite
    {
        public SessionAccessMode Mode { get; set; }
        public StatementNode Statement { get; set; } = null!;
    }

    public class SessionVariableInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<SessionAccessSite> ReadSites { get; } = new();
        public List<SessionAccessSite> WriteSites { get; } = new();
        public List<SessionNullCheckIdiom> NullCheckIdiomsObserved { get; } = new();
    }

    /// <summary>
    /// Catalogs every `Session("key")` read/write in a parsed page. Session state has
    /// no direct equivalent in a stateless Web API, so this catalog is what
    /// AuthConverter/MiddlewareGenerator use to decide, per key, whether it should
    /// become a JWT claim (identity-ish keys like UserId/UserName/Role) or a
    /// short-lived server-side cache entry (shopping-cart-ish keys).
    /// </summary>
    public class SessionVariableTracker
    {
        public Dictionary<string, SessionVariableInfo> Catalog(IEnumerable<StatementNode> statements)
        {
            var result = new Dictionary<string, SessionVariableInfo>(StringComparer.OrdinalIgnoreCase);
            Walk(statements, result);
            return result;
        }

        private void Walk(IEnumerable<StatementNode> statements, Dictionary<string, SessionVariableInfo> catalog)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case AssignmentNode assign:
                        if (TryGetSessionKey(assign.Target, out var writeKey))
                        {
                            RecordAccess(catalog, writeKey!, SessionAccessMode.Write, assign);
                        }
                        ScanExpressionForReads(assign.Value, assign, catalog);
                        break;

                    case CallStatementNode call:
                        ScanExpressionForReads(call.Invocation, call, catalog);
                        break;

                    case AspOutputExpressionStatementNode output:
                        ScanExpressionForReads(output.Expression, output, catalog);
                        break;

                    case IfStatementNode ifStmt:
                        ScanExpressionForReads(ifStmt.Condition, ifStmt, catalog);
                        DetectNullCheckInCondition(ifStmt.Condition, catalog);
                        Walk(ifStmt.TrueBlock.Statements, catalog);
                        if (ifStmt.ElseBlock != null) Walk(ifStmt.ElseBlock.Statements, catalog);
                        break;

                    case SingleLineIfStatementNode single:
                        ScanExpressionForReads(single.Condition, single, catalog);
                        DetectNullCheckInCondition(single.Condition, catalog);
                        Walk(new[] { single.ThenStatement }, catalog);
                        if (single.ElseStatement != null) Walk(new[] { single.ElseStatement }, catalog);
                        break;

                    case WhileStatementNode whileStmt:
                        Walk(whileStmt.Body.Statements, catalog);
                        break;
                    case DoLoopStatementNode doLoop:
                        Walk(doLoop.Body.Statements, catalog);
                        break;
                    case ForStatementNode forStmt:
                        Walk(forStmt.Body.Statements, catalog);
                        break;
                    case ForEachStatementNode forEach:
                        Walk(forEach.Body.Statements, catalog);
                        break;
                    case SelectCaseStatementNode select:
                        foreach (var c in select.Cases) Walk(c.Body.Statements, catalog);
                        if (select.CaseElseBlock != null) Walk(select.CaseElseBlock.Statements, catalog);
                        break;
                }
            }
        }

        private void ScanExpressionForReads(ExpressionNode? expr, StatementNode owner, Dictionary<string, SessionVariableInfo> catalog)
        {
            if (expr is null) return;
            if (TryGetSessionKey(expr, out var key))
            {
                RecordAccess(catalog, key!, SessionAccessMode.Read, owner);
                return; // the Session(...) call itself is the leaf; don't also scan its literal argument
            }
            switch (expr)
            {
                case BinaryExpressionNode bin:
                    ScanExpressionForReads(bin.Left, owner, catalog);
                    ScanExpressionForReads(bin.Right, owner, catalog);
                    break;
                case InvocationExpressionNode inv:
                    ScanExpressionForReads(inv.Target, owner, catalog);
                    foreach (var arg in inv.Arguments) ScanExpressionForReads(arg, owner, catalog);
                    break;
            }
        }

        /// <summary>Matches `Session("key")` (an invocation of the identifier "Session" with one string-literal argument).</summary>
        private static bool TryGetSessionKey(ExpressionNode expr, out string? key)
        {
            key = null;
            if (expr is not InvocationExpressionNode inv) return false;
            if (inv.Target is not IdentifierExpressionNode id || !string.Equals(id.Name, "Session", StringComparison.OrdinalIgnoreCase)) return false;
            if (inv.Arguments.Count != 1 || inv.Arguments[0] is not LiteralExpressionNode lit) return false;
            key = lit.Value?.ToString();
            return key != null;
        }

        private static void RecordAccess(Dictionary<string, SessionVariableInfo> catalog, string name, SessionAccessMode mode, StatementNode statement)
        {
            if (!catalog.TryGetValue(name, out var info))
            {
                info = new SessionVariableInfo { Name = name };
                catalog[name] = info;
            }
            var list = mode == SessionAccessMode.Read ? info.ReadSites : info.WriteSites;
            list.Add(new SessionAccessSite { Mode = mode, Statement = statement });
        }

        private void DetectNullCheckInCondition(ExpressionNode condition, Dictionary<string, SessionVariableInfo> catalog)
        {
            // `If IsEmpty(Session("x")) Then` - IsEmpty(...) can be the *entire* condition,
            // not just one side of a comparison, so this check has to run before (and
            // independently of) the BinaryExpressionNode-only checks below.
            if (condition is InvocationExpressionNode { Arguments.Count: 1 } isEmptyCall
                && isEmptyCall.Target is IdentifierExpressionNode fn
                && string.Equals(fn.Name, "IsEmpty", StringComparison.OrdinalIgnoreCase)
                && TryGetSessionKey(isEmptyCall.Arguments[0], out var isEmptyKey))
            {
                NoteIdiom(catalog, isEmptyKey!, SessionNullCheckIdiom.IsEmptyCall);
                return;
            }

            if (condition is not BinaryExpressionNode bin) return;

            if (TryGetSessionKey(bin.Left, out var key1) || TryGetSessionKey(bin.Right, out key1))
            {
                var key = key1!;
                bool comparesToEmptyString = string.Equals(bin.Operator, "=", StringComparison.Ordinal)
                    && (IsEmptyStringLiteral(bin.Left) || IsEmptyStringLiteral(bin.Right));
                bool comparesToNothing = string.Equals(bin.Operator, "Is", StringComparison.OrdinalIgnoreCase)
                    && (IsNothingLiteral(bin.Left) || IsNothingLiteral(bin.Right));

                if (comparesToEmptyString) NoteIdiom(catalog, key, SessionNullCheckIdiom.EqualsEmptyString);
                else if (comparesToNothing) NoteIdiom(catalog, key, SessionNullCheckIdiom.IsNothingComparison);
            }
        }

        private static bool IsEmptyStringLiteral(ExpressionNode expr) => expr is LiteralExpressionNode { Value: string s } && s.Length == 0;
        private static bool IsNothingLiteral(ExpressionNode expr) => expr is LiteralExpressionNode { Value: null };

        private static void NoteIdiom(Dictionary<string, SessionVariableInfo> catalog, string key, SessionNullCheckIdiom idiom)
        {
            if (!catalog.TryGetValue(key, out var info))
            {
                info = new SessionVariableInfo { Name = key };
                catalog[key] = info;
            }
            if (!info.NullCheckIdiomsObserved.Contains(idiom)) info.NullCheckIdiomsObserved.Add(idiom);
        }
    }
}

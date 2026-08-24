using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;

namespace BLML.Phase5ASPtoAngular.Analysis
{
    public enum StatementKind
    {
        /// <summary>Literal HTML or an output expression - belongs in the Angular template.</summary>
        Presentation,
        /// <summary>Database/session/control-flow work - belongs in the generated API service.</summary>
        BusinessLogic,
        /// <summary>Neither clearly - left in place for a human to sort out.</summary>
        Ambiguous
    }

    public class ClassifiedStatement
    {
        public StatementNode Statement { get; set; } = null!;
        public StatementKind Kind { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Classic ASP pages routinely interleave data access, session/auth checks, and
    /// HTML rendering in one file with no separation of concerns. This walks a parsed
    /// page's statement tree and tags each statement as presentation (HTML/output -
    /// becomes the Angular template) or business logic (ADO/session/auth work -
    /// becomes API service code), so the backend/frontend generators know which is
    /// which without re-deriving it themselves.
    ///
    /// This is a heuristic, not a proof: an assignment is judged business logic only
    /// when its right-hand side mentions a known data-access/session identifier
    /// (Server.CreateObject, an ADODB.* ProgID, Session/Application/Request access,
    /// or a variable already tagged business logic earlier in the same page/sub -
    /// i.e. classification propagates along simple def-use chains). Anything it can't
    /// confidently place either way is marked Ambiguous rather than guessed at.
    /// </summary>
    public class BusinessLogicExtractor
    {
        private static readonly string[] DataAccessMarkers =
        {
            "adodb", "createobject", "connection", "recordset", "command",
            "execute", "moveNext".ToLowerInvariant(), "fields", "commandtext"
        };

        private static readonly string[] SessionAuthMarkers =
        {
            "session", "application", "request", "response.redirect", "server.mappath"
        };

        public List<ClassifiedStatement> Classify(IEnumerable<StatementNode> statements)
        {
            var results = new List<ClassifiedStatement>();
            var businessLogicVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Walk(statements, results, businessLogicVariables);
            return results;
        }

        private void Walk(IEnumerable<StatementNode> statements, List<ClassifiedStatement> results, HashSet<string> businessLogicVariables)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case HtmlOutputStatementNode:
                    case AspOutputExpressionStatementNode:
                        results.Add(new ClassifiedStatement { Statement = stmt, Kind = StatementKind.Presentation, Reason = "Literal HTML or inline output expression." });
                        break;

                    case AssignmentNode assign:
                        results.Add(ClassifyAssignment(assign, businessLogicVariables));
                        break;

                    case CallStatementNode call:
                        results.Add(ClassifyExpression(call, call.Invocation, "Statement-level call"));
                        break;

                    case VariableDeclarationGroupNode:
                        results.Add(new ClassifiedStatement { Statement = stmt, Kind = StatementKind.Ambiguous, Reason = "Declaration only; classification follows from later assignment." });
                        break;

                    case IfStatementNode ifStmt:
                        results.Add(new ClassifiedStatement { Statement = ifStmt, Kind = ContainsAnyMarker(ExpressionToSearchText(ifStmt.Condition), SessionAuthMarkers) ? StatementKind.BusinessLogic : StatementKind.Ambiguous, Reason = "If condition; see nested statements for the branches." });
                        Walk(ifStmt.TrueBlock.Statements, results, businessLogicVariables);
                        if (ifStmt.ElseBlock != null) Walk(ifStmt.ElseBlock.Statements, results, businessLogicVariables);
                        break;

                    case SingleLineIfStatementNode single:
                        Walk(new[] { single.ThenStatement }, results, businessLogicVariables);
                        if (single.ElseStatement != null) Walk(new[] { single.ElseStatement }, results, businessLogicVariables);
                        break;

                    case WhileStatementNode whileStmt:
                        results.Add(new ClassifiedStatement { Statement = whileStmt, Kind = ContainsAnyMarker(ExpressionToSearchText(whileStmt.Condition), DataAccessMarkers) ? StatementKind.BusinessLogic : StatementKind.Ambiguous, Reason = "Loop condition; see nested statements for the body." });
                        Walk(whileStmt.Body.Statements, results, businessLogicVariables);
                        break;

                    case DoLoopStatementNode doLoop:
                        Walk(doLoop.Body.Statements, results, businessLogicVariables);
                        break;

                    case ForStatementNode forStmt:
                        Walk(forStmt.Body.Statements, results, businessLogicVariables);
                        break;

                    case ForEachStatementNode forEach:
                        Walk(forEach.Body.Statements, results, businessLogicVariables);
                        break;

                    case SelectCaseStatementNode select:
                        foreach (var c in select.Cases) Walk(c.Body.Statements, results, businessLogicVariables);
                        if (select.CaseElseBlock != null) Walk(select.CaseElseBlock.Statements, results, businessLogicVariables);
                        break;

                    default:
                        results.Add(new ClassifiedStatement { Statement = stmt, Kind = StatementKind.Ambiguous, Reason = "No classification rule for this statement type." });
                        break;
                }
            }
        }

        private ClassifiedStatement ClassifyAssignment(AssignmentNode assign, HashSet<string> businessLogicVariables)
        {
            var rhsText = ExpressionToSearchText(assign.Value);
            var targetName = (assign.Target as IdentifierExpressionNode)?.Name;

            bool isBusinessLogic = ContainsAnyMarker(rhsText, DataAccessMarkers)
                || ContainsAnyMarker(rhsText, SessionAuthMarkers)
                || ReferencesAny(assign.Value, businessLogicVariables);

            if (isBusinessLogic && targetName != null) businessLogicVariables.Add(targetName);

            return new ClassifiedStatement
            {
                Statement = assign,
                Kind = isBusinessLogic ? StatementKind.BusinessLogic : StatementKind.Ambiguous,
                Reason = isBusinessLogic
                    ? "Right-hand side references data access/session state."
                    : "No known data-access/session marker found; defaulting to ambiguous rather than presentation."
            };
        }

        private ClassifiedStatement ClassifyExpression(StatementNode stmt, ExpressionNode expr, string label)
        {
            var text = ExpressionToSearchText(expr);
            bool isDataAccess = ContainsAnyMarker(text, DataAccessMarkers);
            bool isSessionAuth = ContainsAnyMarker(text, SessionAuthMarkers);
            bool isPresentation = text.Contains("response.write", StringComparison.OrdinalIgnoreCase);

            if (isDataAccess || isSessionAuth)
            {
                return new ClassifiedStatement { Statement = stmt, Kind = StatementKind.BusinessLogic, Reason = $"{label} touches data access or session/auth state." };
            }
            if (isPresentation)
            {
                return new ClassifiedStatement { Statement = stmt, Kind = StatementKind.Presentation, Reason = $"{label} writes directly to the response." };
            }
            return new ClassifiedStatement { Statement = stmt, Kind = StatementKind.Ambiguous, Reason = $"{label} did not match a known classification rule." };
        }

        private static bool ReferencesAny(ExpressionNode expr, HashSet<string> names)
        {
            if (names.Count == 0) return false;
            return CollectIdentifiers(expr).Any(names.Contains);
        }

        private static IEnumerable<string> CollectIdentifiers(ExpressionNode expr)
        {
            switch (expr)
            {
                case IdentifierExpressionNode id:
                    yield return id.Name;
                    break;
                case BinaryExpressionNode bin:
                    if (bin.Left != null) foreach (var n in CollectIdentifiers(bin.Left)) yield return n;
                    if (bin.Right != null) foreach (var n in CollectIdentifiers(bin.Right)) yield return n;
                    break;
                case InvocationExpressionNode inv:
                    foreach (var n in CollectIdentifiers(inv.Target)) yield return n;
                    foreach (var arg in inv.Arguments)
                        foreach (var n in CollectIdentifiers(arg)) yield return n;
                    break;
            }
        }

        /// <summary>Flattens an expression tree to lowercase text for simple substring marker matching.</summary>
        private static string ExpressionToSearchText(ExpressionNode? expr)
        {
            if (expr is null) return string.Empty;
            return expr switch
            {
                IdentifierExpressionNode id => id.Name,
                LiteralExpressionNode lit => lit.Value?.ToString() ?? string.Empty,
                BinaryExpressionNode bin => $"{ExpressionToSearchText(bin.Left)} {bin.Operator} {ExpressionToSearchText(bin.Right)}",
                InvocationExpressionNode inv => $"{ExpressionToSearchText(inv.Target)}({string.Join(",", inv.Arguments.Select(ExpressionToSearchText))})",
                _ => string.Empty
            };
        }

        private static bool ContainsAnyMarker(string text, string[] markers) =>
            markers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase));
    }
}

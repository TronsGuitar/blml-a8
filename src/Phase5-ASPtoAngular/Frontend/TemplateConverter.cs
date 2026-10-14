using System.Text;
using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;

namespace BLML.Phase5ASPtoAngular.Frontend
{
    /// <summary>
    /// Converts a parsed ASP page's statement tree into an Angular template, using the
    /// modern (Angular 17+) built-in control-flow syntax - `@if`/`@else if`/`@else` and
    /// `@for ( ; track ...)` - rather than the older `*ngIf`/`*ngFor` structural
    /// directives, since that's both the current Angular default and what
    /// AngularAntiPatternChecker enforces on every generator's own output.
    ///
    /// The classic ASP recordset loop (`While Not rs.EOF ... rs.MoveNext ... Wend`,
    /// the exact pattern documented in ProjectPlan.md's "Data Binding Patterns"
    /// section) is recognized specifically and rendered as `@for (item of
    /// xItems(); track item.id)`; `rs.MoveNext` is loop mechanics with no template
    /// equivalent and is dropped rather than mistranslated.
    /// </summary>
    public class TemplateConverter
    {
        public List<string> Warnings { get; } = new();

        public string Convert(IEnumerable<StatementNode> statements)
        {
            var exprConverter = new AspExpressionToTypeScript();
            var sb = new StringBuilder();
            ConvertStatements(statements, sb, exprConverter);
            return sb.ToString();
        }

        private void ConvertStatements(IEnumerable<StatementNode> statements, StringBuilder sb, AspExpressionToTypeScript exprConverter)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case HtmlOutputStatementNode html:
                        sb.Append(html.Html);
                        break;

                    case AspOutputExpressionStatementNode output:
                        sb.Append("{{ ").Append(exprConverter.Convert(output.Expression)).Append(" }}");
                        break;

                    case CallStatementNode call when IsRecordsetMoveNext(call, exprConverter):
                        // loop mechanics - no template equivalent
                        break;

                    case CallStatementNode call when TryGetResponseWriteArgument(call, out var writeExpr):
                        sb.Append("{{ ").Append(exprConverter.Convert(writeExpr!)).Append(" }}");
                        break;

                    case CallStatementNode:
                        // business logic, not presentation - nothing to render
                        break;

                    case AssignmentNode:
                        // business logic, not presentation - nothing to render
                        break;

                    case IfStatementNode ifStmt:
                        ConvertIf(ifStmt, sb, exprConverter, isFirst: true);
                        break;

                    case SingleLineIfStatementNode single:
                        sb.Append("@if (").Append(exprConverter.Convert(single.Condition)).Append(") {\n");
                        ConvertStatements(new[] { single.ThenStatement }, sb, exprConverter);
                        sb.Append("\n}");
                        if (single.ElseStatement != null)
                        {
                            sb.Append(" @else {\n");
                            ConvertStatements(new[] { single.ElseStatement }, sb, exprConverter);
                            sb.Append("\n}");
                        }
                        break;

                    case WhileStatementNode whileStmt when TryGetRecordsetLoopVariable(whileStmt.Condition, out var rsVar):
                        ConvertRecordsetLoop(whileStmt, rsVar!, sb, exprConverter);
                        break;

                    case WhileStatementNode whileStmt:
                        Warnings.Add("A While loop that isn't a recognized `While Not rs.EOF` recordset pattern was found in presentation code; rendering as @for over its condition is not supported, statement skipped.");
                        break;

                    case ForStatementNode forStmt:
                        sb.Append("@for (").Append(AspExpressionToTypeScript.ToCamelCase(forStmt.LoopVariable))
                          .Append(" of range(").Append(exprConverter.Convert(forStmt.StartValue)).Append(", ")
                          .Append(exprConverter.Convert(forStmt.EndValue)).Append("); track $index) {\n");
                        ConvertStatements(forStmt.Body.Statements, sb, exprConverter);
                        sb.Append("\n}");
                        break;

                    case ForEachStatementNode forEach:
                        var loopVar = AspExpressionToTypeScript.ToCamelCase(forEach.LoopVariable);
                        sb.Append("@for (").Append(loopVar).Append(" of ").Append(exprConverter.Convert(forEach.Collection))
                          .Append("; track $index) {\n");
                        ConvertStatements(forEach.Body.Statements, sb, exprConverter);
                        sb.Append("\n}");
                        break;

                    case SelectCaseStatementNode select:
                        ConvertSelectCase(select, sb, exprConverter);
                        break;

                    case DoLoopStatementNode doLoop:
                        Warnings.Add("A Do/Loop was found in presentation code; classic ASP rarely uses Do/Loop for row iteration and no template mapping is defined, statement skipped.");
                        break;
                }
            }
        }

        private void ConvertIf(IfStatementNode ifStmt, StringBuilder sb, AspExpressionToTypeScript exprConverter, bool isFirst)
        {
            sb.Append(isFirst ? "@if (" : "@else if (").Append(exprConverter.Convert(ifStmt.Condition)).Append(") {\n");
            ConvertStatements(ifStmt.TrueBlock.Statements, sb, exprConverter);
            sb.Append("\n}");

            if (ifStmt.ElseBlock is { Statements.Count: 1 } && ifStmt.ElseBlock.Statements[0] is IfStatementNode nestedElseIf)
            {
                sb.Append(' ');
                ConvertIf(nestedElseIf, sb, exprConverter, isFirst: false);
            }
            else if (ifStmt.ElseBlock != null)
            {
                sb.Append(" @else {\n");
                ConvertStatements(ifStmt.ElseBlock.Statements, sb, exprConverter);
                sb.Append("\n}");
            }
        }

        private void ConvertSelectCase(SelectCaseStatementNode select, StringBuilder sb, AspExpressionToTypeScript exprConverter)
        {
            var testExprText = exprConverter.Convert(select.TestExpression);
            bool first = true;
            foreach (var clause in select.Cases)
            {
                var conditionText = string.Join(" || ", clause.Values.Select(v => $"{testExprText} === {exprConverter.Convert(v)}"));
                sb.Append(first ? "@if (" : "@else if (").Append(conditionText).Append(") {\n");
                ConvertStatements(clause.Body.Statements, sb, exprConverter);
                sb.Append("\n}");
                first = false;
            }
            if (select.CaseElseBlock != null)
            {
                sb.Append(" @else {\n");
                ConvertStatements(select.CaseElseBlock.Statements, sb, exprConverter);
                sb.Append("\n}");
            }
        }

        private void ConvertRecordsetLoop(WhileStatementNode whileStmt, string rsVar, StringBuilder sb, AspExpressionToTypeScript exprConverter)
        {
            var itemsSignal = AspExpressionToTypeScript.ToCamelCase(rsVar) + "Items";
            exprConverter.RecordsetLoopVariables[rsVar] = "item";

            sb.Append("@for (item of ").Append(itemsSignal).Append("(); track item.id) {\n");
            ConvertStatements(whileStmt.Body.Statements, sb, exprConverter);
            sb.Append("\n}");

            exprConverter.RecordsetLoopVariables.Remove(rsVar);
        }

        /// <summary>Matches the `While Not rs.EOF` loop guard and returns the recordset variable name.</summary>
        private static bool TryGetRecordsetLoopVariable(ExpressionNode condition, out string? rsVar)
        {
            rsVar = null;
            if (condition is not BinaryExpressionNode { Operator: "Not" } notExpr) return false;
            if (notExpr.Right is not BinaryExpressionNode { Operator: "." } member) return false;
            if (member.Left is not IdentifierExpressionNode id) return false;
            if (member.Right is not IdentifierExpressionNode prop || !string.Equals(prop.Name, "EOF", StringComparison.OrdinalIgnoreCase)) return false;
            rsVar = id.Name;
            return true;
        }

        private static bool IsRecordsetMoveNext(CallStatementNode call, AspExpressionToTypeScript exprConverter)
        {
            if (call.Invocation is not InvocationExpressionNode inv) return false;
            if (inv.Target is not BinaryExpressionNode { Operator: "." } member) return false;
            if (member.Left is not IdentifierExpressionNode id) return false;
            if (member.Right is not IdentifierExpressionNode method || !string.Equals(method.Name, "MoveNext", StringComparison.OrdinalIgnoreCase)) return false;
            return exprConverter.RecordsetLoopVariables.ContainsKey(id.Name);
        }

        private static bool TryGetResponseWriteArgument(CallStatementNode call, out ExpressionNode? argument)
        {
            argument = null;
            if (call.Invocation is not InvocationExpressionNode inv) return false;
            if (inv.Target is not BinaryExpressionNode { Operator: "." } member) return false;
            if (member.Left is not IdentifierExpressionNode owner || !string.Equals(owner.Name, "Response", StringComparison.OrdinalIgnoreCase)) return false;
            if (member.Right is not IdentifierExpressionNode method || !string.Equals(method.Name, "Write", StringComparison.OrdinalIgnoreCase)) return false;
            if (inv.Arguments.Count == 0) return false;
            argument = inv.Arguments[0];
            return true;
        }
    }
}

using BLML.Phase1Foundation.AST;

namespace BLML.Phase5ASPtoAngular.Frontend
{
    /// <summary>
    /// Renders a VBScript expression (as parsed by VBScriptParser) as a TypeScript
    /// expression for use inside an Angular template or component. Scoped to the
    /// constructs that actually appear in presentation-classified expressions
    /// (BusinessLogicExtractor has already routed data-access/session work
    /// elsewhere) - anything it doesn't recognize renders as an inline TODO comment
    /// rather than silently emitting something plausible-but-wrong.
    ///
    /// The one recordset-specific rule: inside a converted recordset loop, `rsVar("Field")`
    /// becomes `loopItemVar.field` - the direct translation of ASP's classic
    /// `rs("Field")` pattern into the `@for (item of items(); track item.id)` shape
    /// TemplateConverter generates for `While Not rs.EOF ... Wend`.
    /// </summary>
    public class AspExpressionToTypeScript
    {
        /// <summary>Maps a recordset variable name (e.g. "rs") to the loop item variable currently in scope (e.g. "item"), if any.</summary>
        public Dictionary<string, string> RecordsetLoopVariables { get; } = new(StringComparer.OrdinalIgnoreCase);

        public string Convert(ExpressionNode expr)
        {
            switch (expr)
            {
                case LiteralExpressionNode lit:
                    return ConvertLiteral(lit);

                case IdentifierExpressionNode id:
                    return ToCamelCase(id.Name);

                case InvocationExpressionNode inv when TryConvertRecordsetFieldAccess(inv, out var fieldAccess):
                    return fieldAccess!;

                case InvocationExpressionNode inv:
                    return $"{Convert(inv.Target)}({string.Join(", ", inv.Arguments.Select(Convert))})";

                case BinaryExpressionNode { Operator: "Not" } notExpr:
                    return $"!({Convert(notExpr.Right)})";

                case BinaryExpressionNode { Operator: "." } member:
                    return $"{Convert(member.Left)}.{ConvertMemberName(member.Right)}";

                case BinaryExpressionNode bin:
                    return $"({Convert(bin.Left)} {MapOperator(bin.Operator)} {Convert(bin.Right)})";

                default:
                    return $"/* TODO: manual conversion needed */ null";
            }
        }

        private bool TryConvertRecordsetFieldAccess(InvocationExpressionNode inv, out string? result)
        {
            result = null;
            if (inv.Target is not IdentifierExpressionNode id) return false;
            if (!RecordsetLoopVariables.TryGetValue(id.Name, out var loopVar)) return false;
            if (inv.Arguments.Count != 1 || inv.Arguments[0] is not LiteralExpressionNode { Value: string field }) return false;
            result = $"{loopVar}.{ToCamelCase(field)}";
            return true;
        }

        private static string ConvertMemberName(ExpressionNode member) =>
            member is IdentifierExpressionNode id ? ToCamelCase(id.Name) : "unknown";

        private static string ConvertLiteral(LiteralExpressionNode lit) => lit.Value switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            string s => $"'{s.Replace("\\", "\\\\").Replace("'", "\\'")}'",
            _ => lit.Value.ToString() ?? "null"
        };

        private static string MapOperator(string op) => op switch
        {
            "&" => "+",
            "=" => "===",
            "<>" => "!==",
            "And" => "&&",
            "Or" => "||",
            "Mod" => "%",
            "\\" => "/", // integer division has no direct TS operator; nearest equivalent, flagged separately by the anti-pattern checker is out of scope here
            _ => op
        };

        public static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLowerInvariant(name[0]) + name[1..];
        }
    }
}

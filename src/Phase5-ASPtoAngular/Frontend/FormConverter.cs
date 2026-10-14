using System.Text;
using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;

namespace BLML.Phase5ASPtoAngular.Frontend
{
    public class FormFieldSpec
    {
        public string Name { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool LooksLikeEmail { get; set; }
    }

    public class FormConversionResult
    {
        public List<FormFieldSpec> Fields { get; } = new();
        public List<string> Warnings { get; } = new();
        public string TypeScript { get; set; } = string.Empty;
    }

    /// <summary>
    /// Converts an ASP form-handling page (fields read via `Request.Form("x")`, with
    /// any inline emptiness checks treated as the ASP equivalent of `required`) into a
    /// typed Angular Reactive Form - never `[(ngModel)]`, per this generator's house
    /// style (see AngularAntiPatternChecker).
    ///
    /// Classic ASP ambiguity handled here: bare `Request("x")` (no `.Form`/
    /// `.QueryString`/`.Cookies` qualifier) actually searches QueryString, Form,
    /// Cookies, ServerVariables, and ClientCertificate, in that order - there's no way
    /// to know which collection a given call actually meant without knowing which
    /// collections were populated at runtime. This treats an unqualified read as a
    /// form field (FormConverter's whole job), but records a warning per site instead
    /// of silently guessing.
    /// </summary>
    public class FormConverter
    {
        public FormConversionResult Convert(IEnumerable<StatementNode> statements, string formTypeName)
        {
            var result = new FormConversionResult();
            var fieldNames = new List<string>();
            CollectRequestFields(statements, fieldNames, result.Warnings);

            var fields = fieldNames.Select(name => new FormFieldSpec
            {
                Name = name,
                Required = FieldIsCheckedForEmptiness(statements, name),
                LooksLikeEmail = name.Contains("email", StringComparison.OrdinalIgnoreCase)
            }).ToList();
            result.Fields.AddRange(fields);
            result.TypeScript = GenerateFormTypeScript(formTypeName, fields);
            return result;
        }

        private static void CollectRequestFields(IEnumerable<StatementNode> statements, List<string> fields, List<string> warnings)
        {
            foreach (var expr in EnumerateExpressions(statements))
            {
                if (expr is not InvocationExpressionNode inv) continue;
                if (inv.Arguments.Count != 1 || inv.Arguments[0] is not LiteralExpressionNode { Value: string fieldName }) continue;

                bool isBareRequest = inv.Target is IdentifierExpressionNode bareId && string.Equals(bareId.Name, "Request", StringComparison.OrdinalIgnoreCase);
                bool isRequestForm = inv.Target is BinaryExpressionNode { Operator: "." } member
                    && member.Left is IdentifierExpressionNode owner && string.Equals(owner.Name, "Request", StringComparison.OrdinalIgnoreCase)
                    && member.Right is IdentifierExpressionNode prop && string.Equals(prop.Name, "Form", StringComparison.OrdinalIgnoreCase);

                if (isBareRequest)
                {
                    warnings.Add($"`Request(\"{fieldName}\")` is ambiguous in classic ASP (checks QueryString, Form, Cookies, ServerVariables, ClientCertificate in that order) - treated as a form field; verify this wasn't actually reading the query string.");
                }

                if ((isBareRequest || isRequestForm) && !fields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
                {
                    fields.Add(fieldName);
                }
            }
        }

        /// <summary>Heuristic: an `If Request.Form("x") = "" Then ...error...` (or `<>`) guard near the field read is treated as that field being required.</summary>
        private static bool FieldIsCheckedForEmptiness(IEnumerable<StatementNode> statements, string fieldName)
        {
            foreach (var condition in EnumerateConditions(statements))
            {
                if (condition is not BinaryExpressionNode { Operator: "=" or "<>" } bin) continue;
                if (IsRequestFieldRead(bin.Left, fieldName) && IsEmptyStringLiteral(bin.Right)) return true;
                if (IsRequestFieldRead(bin.Right, fieldName) && IsEmptyStringLiteral(bin.Left)) return true;
            }
            return false;
        }

        private static bool IsRequestFieldRead(ExpressionNode expr, string fieldName)
        {
            if (expr is not InvocationExpressionNode inv) return false;
            if (inv.Arguments.Count != 1 || inv.Arguments[0] is not LiteralExpressionNode { Value: string f }) return false;
            if (!string.Equals(f, fieldName, StringComparison.OrdinalIgnoreCase)) return false;
            return inv.Target is IdentifierExpressionNode { Name: "Request" }
                || (inv.Target is BinaryExpressionNode { Operator: "." } m && m.Left is IdentifierExpressionNode { Name: "Request" });
        }

        private static bool IsEmptyStringLiteral(ExpressionNode expr) => expr is LiteralExpressionNode { Value: string s } && s.Length == 0;

        private static IEnumerable<ExpressionNode> EnumerateConditions(IEnumerable<StatementNode> statements)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case IfStatementNode i:
                        yield return i.Condition;
                        foreach (var c in EnumerateConditions(i.TrueBlock.Statements)) yield return c;
                        if (i.ElseBlock != null) foreach (var c in EnumerateConditions(i.ElseBlock.Statements)) yield return c;
                        break;
                    case SingleLineIfStatementNode s:
                        yield return s.Condition;
                        break;
                }
            }
        }

        private static IEnumerable<ExpressionNode> EnumerateExpressions(IEnumerable<StatementNode> statements)
        {
            foreach (var stmt in statements)
            {
                IEnumerable<ExpressionNode> here = stmt switch
                {
                    AssignmentNode a => new[] { a.Value },
                    CallStatementNode c => new[] { c.Invocation },
                    AspOutputExpressionStatementNode o => new[] { o.Expression },
                    IfStatementNode i => new[] { i.Condition },
                    SingleLineIfStatementNode s => new[] { s.Condition },
                    _ => Enumerable.Empty<ExpressionNode>()
                };
                foreach (var e in here) foreach (var sub in Flatten(e)) yield return sub;

                IEnumerable<StatementNode>? nested = stmt switch
                {
                    IfStatementNode i => i.ElseBlock is null ? i.TrueBlock.Statements : i.TrueBlock.Statements.Concat(i.ElseBlock.Statements),
                    SingleLineIfStatementNode s => s.ElseStatement is null ? new[] { s.ThenStatement } : new[] { s.ThenStatement, s.ElseStatement },
                    _ => null
                };
                if (nested != null) foreach (var e in EnumerateExpressions(nested)) yield return e;
            }
        }

        private static IEnumerable<ExpressionNode> Flatten(ExpressionNode expr)
        {
            yield return expr;
            if (expr is BinaryExpressionNode bin)
            {
                if (bin.Left != null) foreach (var e in Flatten(bin.Left)) yield return e;
                if (bin.Right != null) foreach (var e in Flatten(bin.Right)) yield return e;
            }
            else if (expr is InvocationExpressionNode inv)
            {
                foreach (var e in Flatten(inv.Target)) yield return e;
                foreach (var arg in inv.Arguments) foreach (var e in Flatten(arg)) yield return e;
            }
        }

        private static string GenerateFormTypeScript(string formTypeName, List<FormFieldSpec> fields)
        {
            var sb = new StringBuilder();
            sb.AppendLine("import { inject } from '@angular/core';");
            sb.AppendLine("import { FormBuilder, Validators } from '@angular/forms';");
            sb.AppendLine();
            sb.AppendLine($"// Typed Reactive Form - no [(ngModel)], per this generator's house style.");
            sb.AppendLine($"const fb = inject(FormBuilder);");
            sb.AppendLine($"export const {formTypeName} = fb.group({{");
            foreach (var field in fields)
            {
                var validators = new List<string>();
                if (field.Required) validators.Add("Validators.required");
                if (field.LooksLikeEmail) validators.Add("Validators.email");
                var validatorsText = validators.Count > 0 ? $", validators: [{string.Join(", ", validators)}]" : "";
                sb.AppendLine($"  {AspExpressionToTypeScript.ToCamelCase(field.Name)}: fb.control('', {{ nonNullable: true{validatorsText} }}),");
            }
            sb.AppendLine("});");
            return sb.ToString();
        }
    }
}

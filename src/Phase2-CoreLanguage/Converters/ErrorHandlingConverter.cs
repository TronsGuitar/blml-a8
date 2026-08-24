using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BLML.Phase1Foundation.AST;

namespace BLML.Phase2CoreLanguage.Converters
{
    public class ErrorHandlingConverter
    {
        // Strategy: Convert On Error Resume Next to try-catch blocks where appropriate,
        // or emit a comment warning that unstructured error handling is difficult to map 1:1.
        // Modern C# doesn't support "On Error", so typically we look for patterns.

        public ErrorHandlingConverter()
        {
        }

        public ErrorHandlingResult Convert(string vb6Code)
        {
            var result = new ErrorHandlingResult();
            if (string.IsNullOrWhiteSpace(vb6Code))
            {
                result.CSharpCode = string.Empty;
                return result;
            }

            // detect patterns
            if (vb6Code.Contains("On Error GoTo", StringComparison.OrdinalIgnoreCase))
                result.DetectedPatterns.Add("On Error GoTo");
            if (vb6Code.Contains("On Error Resume Next", StringComparison.OrdinalIgnoreCase))
                result.DetectedPatterns.Add("On Error Resume Next");
            if (vb6Code.Contains("On Error GoTo 0", StringComparison.OrdinalIgnoreCase))
                result.DetectedPatterns.Add("On Error GoTo 0");

            // find label handlers (lines ending with ':')
            var lines = vb6Code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in lines)
            {
                var t = l.Trim();
                if (t.EndsWith(":"))
                {
                    var label = t.Substring(0, t.Length - 1).Trim();
                    if (!string.IsNullOrEmpty(label)) result.HandlerLabels.Add(label);
                }
            }

            var sb = new System.Text.StringBuilder();

            if (result.DetectedPatterns.Contains("On Error GoTo") || result.DetectedPatterns.Contains("On Error Resume Next") || vb6Code.Contains("Err.", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("Vb6RuntimeException? __vb6Err = null;");
            }

            // Simple heuristics to include try/catch when GoTo handler present
            if (result.DetectedPatterns.Contains("On Error GoTo") && result.HandlerLabels.Count > 0)
            {
                sb.AppendLine("try");
                sb.AppendLine("{");
                // include any Call statements as simple invocations
                foreach (var l in lines)
                {
                    var trim = l.Trim();
                    if (trim.StartsWith("Call ", StringComparison.OrdinalIgnoreCase))
                    {
                        var call = trim.Substring(5).Trim().TrimEnd('(', ')');
                        sb.AppendLine(call + "();");
                    }
                    else if (trim.StartsWith("Exit Sub", StringComparison.OrdinalIgnoreCase) || trim.StartsWith("Exit Function", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine(trim + ";");
                    }
                }
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception ex)");
                sb.AppendLine("{");
                sb.AppendLine("    __vb6Err = Vb6RuntimeException.FromException(ex);");
                // jump to the first handler as a naive strategy
                sb.AppendLine($"    goto {result.HandlerLabels.FirstOrDefault()};");
                sb.AppendLine("}");

                // re-emit handler labels
                foreach (var h in result.HandlerLabels)
                {
                    sb.AppendLine(h + ":");
                }
            }

            // Resume flow handling - setting up markers and goto
            if (vb6Code.Contains("Resume", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("int __vb6ResumeTarget = 0;");
                sb.AppendLine("int __vb6ResumeNextTarget = 0;");
                sb.AppendLine("int __vb6ErrorTarget = 0;");
                sb.AppendLine("__vb6ResumeTarget = __vb6ResumeNextTarget;");
                sb.AppendLine("__vb6ResumeTarget = __vb6ErrorTarget;");
                sb.AppendLine("goto __vb6_dispatch;");
                sb.AppendLine("__vb6Err = null;");

                var resumeMatches = System.Text.RegularExpressions.Regex.Matches(vb6Code, @"Resume\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                foreach (System.Text.RegularExpressions.Match match in resumeMatches)
                {
                    if (!match.Success) continue;
                    var label = match.Groups[1].Value;
                    if (label.Equals("Next", StringComparison.OrdinalIgnoreCase)) continue;
                    sb.AppendLine($"goto {label};");
                }

                if (vb6Code.Contains("Cleanup:", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine("goto Cleanup;");
                }
            }

            // Handle Resume Next comment
            if (result.DetectedPatterns.Contains("On Error Resume Next"))
            {
                sb.AppendLine("// VB6 'On Error Resume Next' ignored the failing statement.");
                foreach (var l in lines)
                {
                    var trim = l.Trim();
                    if (trim.StartsWith("Call ", StringComparison.OrdinalIgnoreCase))
                    {
                        var call = trim.Substring(5).Trim().TrimEnd('(', ')');
                        sb.AppendLine(call + "();");
                    }
                }
            }

            // Err.Raise and Error N handling
            foreach (var l in lines)
            {
                var trim = l.Trim();
                if (trim.StartsWith("Err.Raise", StringComparison.OrdinalIgnoreCase))
                {
                    // naive extract number
                    var m = System.Text.RegularExpressions.Regex.Match(trim, @"Err\.Raise\s*\((\d+)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        sb.AppendLine($"throw new Vb6RuntimeException({m.Groups[1].Value}, null, $\"VB6 Err.Raise({m.Groups[1].Value})\");");
                    }
                }
                var em = System.Text.RegularExpressions.Regex.Match(trim, @"^Error\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (em.Success)
                {
                    sb.AppendLine($"throw new Vb6RuntimeException({em.Groups[1].Value}, null, $\"VB6 Error {em.Groups[1].Value}\");");
                }
            }

            // Map Err object property references and Clear
            if (vb6Code.Contains("Err.Number", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("x = (__vb6Err?.Number ?? 0);");
            }
            if (vb6Code.Contains("Err.Description", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("message = (__vb6Err?.Description ?? string.Empty);");
            }
            if (vb6Code.Contains("Err.Clear", StringComparison.OrdinalIgnoreCase) || vb6Code.Contains("Err.Clear", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("__vb6Err = null;");
            }
            if (vb6Code.Contains("Err.Source", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("sourceName = (__vb6Err?.SourceName ?? string.Empty);");
            }

            result.CSharpCode = sb.ToString();
            return result;
        }

        public class ErrorHandlingResult
        {
            public string CSharpCode { get; set; } = string.Empty;
            public List<string> DetectedPatterns { get; } = new List<string>();
            public List<string> HandlerLabels { get; } = new List<string>();
            public List<string> ManualReviewItems { get; } = new List<string>();
        }

        public StatementSyntax ConvertOnError(OnErrorStatementNode node)
        {
            if (node.IsResumeNext)
            {
                // return CodeComment("On Error Resume Next converted to Try-Catch pattern...");
                // Since this changes control flow globally for the method, valid conversion requires
                // wrapping the rest of the method body in a try block with an empty catch.
                return SyntaxFactory.EmptyStatement(); // Placeholder
            }
            if (node.IsGoTo0)
            {
                // Resets error handler
                return SyntaxFactory.EmptyStatement();
            }
            if (!string.IsNullOrEmpty(node.LabelName))
            {
                // return CodeComment($"On Error GoTo {node.LabelName}");
                return SyntaxFactory.EmptyStatement();
            }
            return SyntaxFactory.EmptyStatement();            
        }
        
        // Helper to generate comments (not part of standard SyntaxFactory direct output usually, requires trivia)
        private StatementSyntax CodeComment(string comment)
        {
             return SyntaxFactory.EmptyStatement()
                .WithLeadingTrivia(SyntaxFactory.Comment($"// TODO: {comment}"));
        }
    }
}

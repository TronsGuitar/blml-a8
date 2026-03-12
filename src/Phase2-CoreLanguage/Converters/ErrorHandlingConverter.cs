using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BLML.Phase1Foundation.AST;

namespace BLML.Phase2CoreLanguage.Converters
{
    public class ErrorHandlingConverter
    {
        private static readonly Regex LabelRegex = new(@"^(?<label>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*$", RegexOptions.Compiled);
        private static readonly Regex OnErrorGoToRegex = new(@"^On\s+Error\s+GoTo\s+(?<label>[A-Za-z_][A-Za-z0-9_]*|0)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex OnErrorResumeNextRegex = new(@"^On\s+Error\s+Resume\s+Next\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ErrRaiseRegex = new(@"^Err\.Raise\s*\((?<args>.*)\)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ErrorStatementRegex = new(@"^Error\s+(?<number>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ResumeNextRegex = new(@"^Resume\s+Next\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ResumeRegex = new(@"^Resume(?:\s+(?<label>[A-Za-z_][A-Za-z0-9_]*))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ErrorHandlingConversionResult Convert(string vb6Code)
        {
            ArgumentNullException.ThrowIfNull(vb6Code);

            var procedure = ParseProcedure(vb6Code);
            var manualReviewItems = new List<string>();
            var detectedPatterns = procedure.DetectedPatterns.ToList();
            var builder = new StringBuilder();

            if (procedure.RequiresErrObject)
            {
                builder.AppendLine("Vb6RuntimeException? __vb6Err = null;");
            }

            if (!string.IsNullOrWhiteSpace(procedure.FirstGoToLabel))
            {
                AppendGoToHandlerProcedure(builder, procedure, manualReviewItems);
            }
            else
            {
                AppendLinearProcedure(builder, procedure, manualReviewItems);
            }

            return new ErrorHandlingConversionResult
            {
                CSharpCode = builder.ToString().TrimEnd(),
                HandlerLabels = procedure.Labels,
                DetectedPatterns = detectedPatterns.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                ManualReviewItems = manualReviewItems.Distinct(StringComparer.Ordinal).ToArray()
            };
        }

        private static void AppendLinearProcedure(StringBuilder builder, ErrorHandlingProcedureNode procedure, List<string> manualReviewItems)
        {
            var inResumeNextScope = false;

            foreach (var statement in procedure.Statements)
            {
                switch (statement)
                {
                    case OnErrorResumeNextStatementNode:
                        inResumeNextScope = true;
                        break;
                    case OnErrorGoToStatementNode goToStatement when string.Equals(goToStatement.Label, "0", StringComparison.Ordinal):
                        inResumeNextScope = false;
                        break;
                    case LabelStatementNode labelStatement:
                        builder.AppendLine($"{labelStatement.Label}:");
                        break;
                    case ResumeNextStatementNode:
                        manualReviewItems.Add("Resume Next requires manual review when no active 'On Error GoTo' handler is available.");
                        builder.AppendLine("// TODO: Resume Next requires manual review.");
                        break;
                    case ResumeStatementNode resumeStatement when !string.IsNullOrWhiteSpace(resumeStatement.TargetLabel):
                        if (procedure.RequiresErrObject)
                        {
                            builder.AppendLine("__vb6Err = null;");
                        }
                        builder.AppendLine($"goto {resumeStatement.TargetLabel};");
                        break;
                    case ResumeStatementNode:
                        manualReviewItems.Add("Resume requires manual review when no active 'On Error GoTo' handler is available.");
                        builder.AppendLine("// TODO: Resume requires manual review.");
                        break;
                    case ExecutableStatementNode executableStatement:
                        AppendExecutableStatement(builder, executableStatement.Text, inResumeNextScope, procedure.RequiresErrObject, indent: string.Empty, catchAction: null);
                        break;
                }
            }
        }

        private static void AppendGoToHandlerProcedure(StringBuilder builder, ErrorHandlingProcedureNode procedure, List<string> manualReviewItems)
        {
            var handlerLabel = procedure.FirstGoToLabel!;
            var handlerIndex = -1;
            for (var i = 0; i < procedure.Statements.Count; i++)
            {
                if (procedure.Statements[i] is LabelStatementNode labelStatement && string.Equals(labelStatement.Label, handlerLabel, StringComparison.OrdinalIgnoreCase))
                {
                    handlerIndex = i;
                    break;
                }
            }

            if (handlerIndex < 0)
            {
                manualReviewItems.Add($"Handler label '{handlerLabel}' could not be located for exact Resume reconstruction.");
                AppendLinearProcedure(builder, procedure, manualReviewItems);
                return;
            }

            var protectedStatements = procedure.Statements.Take(handlerIndex).ToList();
            var labelStateMap = BuildLabelStateMap(protectedStatements);
            var afterProtectedState = protectedStatements.Count;

            builder.AppendLine("int __vb6ResumeTarget = 0;");
            builder.AppendLine("int __vb6ResumeNextTarget = 0;");
            builder.AppendLine("int __vb6ErrorTarget = 0;");
            builder.AppendLine("__vb6_dispatch:");
            builder.AppendLine("switch (__vb6ResumeTarget)");
            builder.AppendLine("{");

            for (var i = 0; i < protectedStatements.Count; i++)
            {
                builder.AppendLine($"    case {i}:");
                builder.AppendLine($"        goto __vb6_state_{i};");
            }

            builder.AppendLine($"    case {afterProtectedState}:");
            builder.AppendLine("    default:");
            builder.AppendLine("        goto __vb6_after_protected;");
            builder.AppendLine("}");

            var errorMode = ErrorHandlingMode.None;
            for (var i = 0; i < protectedStatements.Count; i++)
            {
                var statement = protectedStatements[i];
                var nextState = i + 1;

                builder.AppendLine($"__vb6_state_{i}:");

                switch (statement)
                {
                    case OnErrorResumeNextStatementNode:
                        errorMode = ErrorHandlingMode.ResumeNext;
                        break;
                    case OnErrorGoToStatementNode goToStatement when string.Equals(goToStatement.Label, "0", StringComparison.OrdinalIgnoreCase):
                        errorMode = ErrorHandlingMode.None;
                        break;
                    case OnErrorGoToStatementNode goToStatement:
                        if (string.Equals(goToStatement.Label, handlerLabel, StringComparison.OrdinalIgnoreCase))
                        {
                            errorMode = ErrorHandlingMode.GoToHandler;
                        }
                        else
                        {
                            manualReviewItems.Add($"On Error GoTo {goToStatement.Label} is not the primary handler label and may require manual review.");
                        }
                        break;
                    case LabelStatementNode labelStatement:
                        builder.AppendLine($"{labelStatement.Label}:");
                        break;
                    case ExecutableStatementNode executableStatement:
                        var catchAction = errorMode == ErrorHandlingMode.GoToHandler ? $"goto {handlerLabel};" : null;
                        AppendExecutableStatement(builder, executableStatement.Text, errorMode == ErrorHandlingMode.ResumeNext || errorMode == ErrorHandlingMode.GoToHandler, procedure.RequiresErrObject, indent: string.Empty, catchAction, i, nextState);
                        break;
                    case ResumeNextStatementNode:
                    case ResumeStatementNode:
                        manualReviewItems.Add("Resume statements are expected inside the handler region; placement before the handler label requires manual review.");
                        break;
                }
            }

            builder.AppendLine("__vb6_after_protected:");

            for (var i = handlerIndex; i < procedure.Statements.Count; i++)
            {
                switch (procedure.Statements[i])
                {
                    case LabelStatementNode labelStatement:
                        builder.AppendLine($"{labelStatement.Label}:");
                        break;
                    case ResumeNextStatementNode:
                        if (procedure.RequiresErrObject)
                        {
                            builder.AppendLine("__vb6Err = null;");
                        }
                        builder.AppendLine("__vb6ResumeTarget = __vb6ResumeNextTarget;");
                        builder.AppendLine("goto __vb6_dispatch;");
                        break;
                    case ResumeStatementNode resumeStatement when string.IsNullOrWhiteSpace(resumeStatement.TargetLabel):
                        if (procedure.RequiresErrObject)
                        {
                            builder.AppendLine("__vb6Err = null;");
                        }
                        builder.AppendLine("__vb6ResumeTarget = __vb6ErrorTarget;");
                        builder.AppendLine("goto __vb6_dispatch;");
                        break;
                    case ResumeStatementNode resumeStatement:
                        if (procedure.RequiresErrObject)
                        {
                            builder.AppendLine("__vb6Err = null;");
                        }

                        if (labelStateMap.TryGetValue(resumeStatement.TargetLabel!, out var state))
                        {
                            builder.AppendLine($"__vb6ResumeTarget = {state};");
                            builder.AppendLine("goto __vb6_dispatch;");
                        }
                        else
                        {
                            builder.AppendLine($"goto {resumeStatement.TargetLabel};");
                        }
                        break;
                    case ExecutableStatementNode executableStatement:
                        builder.AppendLine(ConvertExecutableLine(executableStatement.Text));
                        break;
                    case OnErrorResumeNextStatementNode:
                    case OnErrorGoToStatementNode:
                        break;
                }
            }
        }

        private static Dictionary<string, int> BuildLabelStateMap(IReadOnlyList<ErrorHandlingStatementNode> statements)
        {
            var labelStateMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < statements.Count; i++)
            {
                if (statements[i] is LabelStatementNode labelStatement)
                {
                    labelStateMap[labelStatement.Label] = i;
                }
            }

            return labelStateMap;
        }

        private static void AppendExecutableStatement(
            StringBuilder builder,
            string line,
            bool wrapInTryCatch,
            bool requiresErrObject,
            string indent,
            string? catchAction,
            int? errorTarget = null,
            int? resumeNextTarget = null)
        {
            var convertedLine = ConvertExecutableLine(line);

            if (errorTarget.HasValue)
            {
                builder.AppendLine($"{indent}__vb6ErrorTarget = {errorTarget.Value};");
            }

            if (resumeNextTarget.HasValue)
            {
                builder.AppendLine($"{indent}__vb6ResumeNextTarget = {resumeNextTarget.Value};");
            }

            if (!wrapInTryCatch)
            {
                builder.AppendLine($"{indent}{convertedLine}");
                return;
            }

            builder.AppendLine($"{indent}try");
            builder.AppendLine($"{indent}{{");
            builder.AppendLine($"{indent}    {convertedLine}");
            builder.AppendLine($"{indent}}}");
            builder.AppendLine($"{indent}catch (Exception ex)");
            builder.AppendLine($"{indent}{{");
            if (requiresErrObject)
            {
                builder.AppendLine($"{indent}    __vb6Err = Vb6RuntimeException.FromException(ex);");
            }

            if (!string.IsNullOrWhiteSpace(catchAction))
            {
                builder.AppendLine($"{indent}    {catchAction}");
            }
            else
            {
                builder.AppendLine($"{indent}    // VB6 'On Error Resume Next' ignored the failing statement.");
            }
            builder.AppendLine($"{indent}}}");
        }

        private static ErrorHandlingProcedureNode ParseProcedure(string vb6Code)
        {
            var procedure = new ErrorHandlingProcedureNode();
            var detectedPatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var requiresErrObject = false;
            string? firstGoToLabel = null;

            foreach (var rawLine in SplitLines(vb6Code))
            {
                var trimmed = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                var labelMatch = LabelRegex.Match(trimmed);
                if (labelMatch.Success)
                {
                    var label = labelMatch.Groups["label"].Value;
                    procedure.Labels.Add(label);
                    procedure.Statements.Add(new LabelStatementNode { Label = label });
                    continue;
                }

                var goToMatch = OnErrorGoToRegex.Match(trimmed);
                if (goToMatch.Success)
                {
                    var label = goToMatch.Groups["label"].Value;
                    detectedPatterns.Add(label == "0" ? "On Error GoTo 0" : "On Error GoTo");
                    requiresErrObject = true;
                    if (label != "0" && firstGoToLabel is null)
                    {
                        firstGoToLabel = label;
                    }
                    procedure.Statements.Add(new OnErrorGoToStatementNode { Label = label });
                    continue;
                }

                if (OnErrorResumeNextRegex.IsMatch(trimmed))
                {
                    detectedPatterns.Add("On Error Resume Next");
                    requiresErrObject = true;
                    procedure.Statements.Add(new OnErrorResumeNextStatementNode());
                    continue;
                }

                if (ResumeNextRegex.IsMatch(trimmed))
                {
                    detectedPatterns.Add("Resume Next");
                    procedure.Statements.Add(new ResumeNextStatementNode());
                    continue;
                }

                var resumeMatch = ResumeRegex.Match(trimmed);
                if (resumeMatch.Success)
                {
                    detectedPatterns.Add("Resume");
                    procedure.Statements.Add(new ResumeStatementNode
                    {
                        TargetLabel = resumeMatch.Groups["label"].Success ? resumeMatch.Groups["label"].Value : null
                    });
                    continue;
                }

                if (trimmed.Contains("Err.", StringComparison.OrdinalIgnoreCase) || ErrorStatementRegex.IsMatch(trimmed))
                {
                    requiresErrObject = true;
                }

                procedure.Statements.Add(new ExecutableStatementNode { Text = trimmed });
            }

            procedure.RequiresErrObject = requiresErrObject;
            procedure.FirstGoToLabel = firstGoToLabel;
            procedure.DetectedPatterns.AddRange(detectedPatterns);

            var distinctLabels = procedure.Labels.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            procedure.Labels.Clear();
            procedure.Labels.AddRange(distinctLabels);

            return procedure;
        }

        private static string ConvertExecutableLine(string line)
        {
            var errRaiseMatch = ErrRaiseRegex.Match(line);
            if (errRaiseMatch.Success)
            {
                var arguments = SplitArguments(errRaiseMatch.Groups["args"].Value);
                var number = arguments.ElementAtOrDefault(0) ?? "0";
                var source = arguments.ElementAtOrDefault(1) ?? "null";
                var description = arguments.ElementAtOrDefault(2) ?? $"$\"VB6 Err.Raise({number})\"";
                return $"throw new Vb6RuntimeException({number}, {source}, {description});";
            }

            var errorMatch = ErrorStatementRegex.Match(line);
            if (errorMatch.Success)
            {
                var errorNumber = errorMatch.Groups["number"].Value.Trim();
                return $"throw new Vb6RuntimeException({errorNumber}, null, $\"VB6 Error {errorNumber}\");";
            }

            if (string.Equals(line, "Err.Clear", StringComparison.OrdinalIgnoreCase))
            {
                return "__vb6Err = null;";
            }

            line = Regex.Replace(line, @"\bCall\s+", string.Empty, RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bExit\s+(Sub|Function|Property)\b", "return", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bExit\s+(Do|For)\b", "break", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bGoTo\s+(?<label>[A-Za-z_][A-Za-z0-9_]*)\b", "goto ${label}", RegexOptions.IgnoreCase);
            line = ReplaceErrObjectReferences(line);

            return line.EndsWith(";", StringComparison.Ordinal) ? line : $"{line};";
        }

        private static string ReplaceErrObjectReferences(string line)
        {
            line = Regex.Replace(line, @"\bErr\.Number\b", "(__vb6Err?.Number ?? 0)", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bErr\.Description\b", "(__vb6Err?.Description ?? string.Empty)", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bErr\.Source\b", "(__vb6Err?.SourceName ?? string.Empty)", RegexOptions.IgnoreCase);
            return line;
        }

        private static IReadOnlyList<string> SplitArguments(string argumentText)
        {
            return argumentText
                .Split(',')
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToArray();
        }

        private static string[] SplitLines(string text)
        {
            return text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        }

        private enum ErrorHandlingMode
        {
            None,
            GoToHandler,
            ResumeNext
        }
    }

    public sealed class ErrorHandlingConversionResult
    {
        public string CSharpCode { get; init; } = string.Empty;

        public IReadOnlyList<string> HandlerLabels { get; init; } = Array.Empty<string>();

        public IReadOnlyList<string> DetectedPatterns { get; init; } = Array.Empty<string>();

        public IReadOnlyList<string> ManualReviewItems { get; init; } = Array.Empty<string>();
    }

    public sealed class Vb6RuntimeException : Exception
    {
        public Vb6RuntimeException(int? number, string? sourceName, string? description)
            : this(number, sourceName, description, null)
        {
        }

        private Vb6RuntimeException(int? number, string? sourceName, string? description, Exception? innerException)
            : base(description ?? (number.HasValue ? $"VB6 runtime error {number.Value}" : "VB6 runtime error"), innerException)
        {
            Number = number;
            SourceName = sourceName;
        }

        public int? Number { get; }

        public string? SourceName { get; }

        public string Description => Message;

        public static Vb6RuntimeException FromException(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception as Vb6RuntimeException
                ?? new Vb6RuntimeException(null, exception.Source, exception.Message, exception);
        }
    }
}

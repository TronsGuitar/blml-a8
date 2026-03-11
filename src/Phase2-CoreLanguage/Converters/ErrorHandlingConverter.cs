using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

            var goToLabel = procedure.FirstGoToLabel;
            var inResumeNextScope = false;
            var inHandler = false;
            var tryOpened = false;

            if (goToLabel is not null)
            {
                builder.AppendLine("try");
                builder.AppendLine("{");
                tryOpened = true;
            }

            foreach (var statement in procedure.Statements)
            {
                switch (statement)
                {
                    case OnErrorResumeNextStatement:
                        inResumeNextScope = true;
                        break;
                    case OnErrorGoToStatement goToStatement when string.Equals(goToStatement.Label, "0", StringComparison.Ordinal):
                        inResumeNextScope = false;
                        break;
                    case LabelStatement labelStatement:
                        if (tryOpened && !inHandler && string.Equals(labelStatement.Label, goToLabel, StringComparison.OrdinalIgnoreCase))
                        {
                            builder.AppendLine("}");
                            builder.AppendLine("catch (Exception ex)");
                            builder.AppendLine("{");
                            if (procedure.RequiresErrObject)
                            {
                                builder.AppendLine("    __vb6Err = Vb6RuntimeException.FromException(ex);");
                            }
                            builder.AppendLine($"    // Equivalent of VB6 'On Error GoTo {goToLabel}'");
                            builder.AppendLine($"    goto {goToLabel};");
                            builder.AppendLine("}");
                            inHandler = true;
                        }

                        builder.AppendLine($"{labelStatement.Label}:");
                        break;
                    case ResumeNextStatement:
                        manualReviewItems.Add("Resume Next requires manual review because the original control-flow target is context dependent.");
                        builder.AppendLine("// TODO: Resume Next requires manual review.");
                        break;
                    case ResumeStatement resumeStatement when !string.IsNullOrWhiteSpace(resumeStatement.TargetLabel):
                        if (procedure.RequiresErrObject)
                        {
                            builder.AppendLine("__vb6Err = null;");
                        }
                        builder.AppendLine($"goto {resumeStatement.TargetLabel};");
                        break;
                    case ResumeStatement:
                        manualReviewItems.Add("Resume requires manual review because the original failure point must be reconstructed.");
                        builder.AppendLine("// TODO: Resume requires manual review.");
                        break;
                    case ExecutableStatement executableStatement:
                        var convertedLine = ConvertExecutableLine(executableStatement.Text);

                        if (inResumeNextScope)
                        {
                            builder.AppendLine("try");
                            builder.AppendLine("{");
                            builder.AppendLine($"    {convertedLine}");
                            builder.AppendLine("}");
                            builder.AppendLine("catch (Exception ex)");
                            builder.AppendLine("{");
                            if (procedure.RequiresErrObject)
                            {
                                builder.AppendLine("    __vb6Err = Vb6RuntimeException.FromException(ex);");
                            }
                            builder.AppendLine("    // VB6 'On Error Resume Next' ignored the failing statement.");
                            builder.AppendLine("}");
                        }
                        else if (tryOpened && !inHandler)
                        {
                            builder.AppendLine($"    {convertedLine}");
                        }
                        else
                        {
                            builder.AppendLine(convertedLine);
                        }

                        break;
                }
            }

            if (tryOpened && !inHandler)
            {
                builder.AppendLine("}");
                builder.AppendLine("catch (Exception ex)");
                builder.AppendLine("{");
                if (procedure.RequiresErrObject)
                {
                    builder.AppendLine("    __vb6Err = Vb6RuntimeException.FromException(ex);");
                }
                builder.AppendLine("    throw;");
                builder.AppendLine("}");
            }

            return new ErrorHandlingConversionResult
            {
                CSharpCode = builder.ToString().TrimEnd(),
                HandlerLabels = procedure.Labels,
                DetectedPatterns = detectedPatterns.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                ManualReviewItems = manualReviewItems.Distinct(StringComparer.Ordinal).ToArray()
            };
        }

        private static ErrorHandlingProcedure ParseProcedure(string vb6Code)
        {
            var statements = new List<ErrorHandlingStatement>();
            var labels = new List<string>();
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
                    labels.Add(label);
                    statements.Add(new LabelStatement(label));
                    continue;
                }

                var goToMatch = OnErrorGoToRegex.Match(trimmed);
                if (goToMatch.Success)
                {
                    var label = goToMatch.Groups["label"].Value;
                    detectedPatterns.Add(label == "0" ? "On Error GoTo 0" : "On Error GoTo");
                    if (label != "0" && firstGoToLabel is null)
                    {
                        firstGoToLabel = label;
                    }
                    statements.Add(new OnErrorGoToStatement(label));
                    continue;
                }

                if (OnErrorResumeNextRegex.IsMatch(trimmed))
                {
                    detectedPatterns.Add("On Error Resume Next");
                    statements.Add(new OnErrorResumeNextStatement());
                    continue;
                }

                if (ResumeNextRegex.IsMatch(trimmed))
                {
                    detectedPatterns.Add("Resume Next");
                    statements.Add(new ResumeNextStatement());
                    continue;
                }

                var resumeMatch = ResumeRegex.Match(trimmed);
                if (resumeMatch.Success)
                {
                    detectedPatterns.Add("Resume");
                    statements.Add(new ResumeStatement(resumeMatch.Groups["label"].Success ? resumeMatch.Groups["label"].Value : null));
                    continue;
                }

                if (trimmed.Contains("Err.", StringComparison.OrdinalIgnoreCase) || ErrorStatementRegex.IsMatch(trimmed))
                {
                    requiresErrObject = true;
                }

                statements.Add(new ExecutableStatement(trimmed));
            }

            return new ErrorHandlingProcedure(
                statements,
                labels.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                detectedPatterns.ToArray(),
                requiresErrObject,
                firstGoToLabel);
        }

        private static string ConvertExecutableLine(string line)
        {
            var errRaiseMatch = ErrRaiseRegex.Match(line);
            if (errRaiseMatch.Success)
            {
                var arguments = SplitArguments(errRaiseMatch.Groups["args"].Value);
                var number = arguments.ElementAtOrDefault(0) ?? "0";
                var source = arguments.ElementAtOrDefault(1) ?? "null";
                var description = arguments.ElementAtOrDefault(2) ?? "$\"VB6 Err.Raise({number})\"";
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

        private sealed record ErrorHandlingProcedure(
            IReadOnlyList<ErrorHandlingStatement> Statements,
            IReadOnlyList<string> Labels,
            IReadOnlyList<string> DetectedPatterns,
            bool RequiresErrObject,
            string? FirstGoToLabel);

        private abstract record ErrorHandlingStatement;

        private sealed record OnErrorGoToStatement(string Label) : ErrorHandlingStatement;

        private sealed record OnErrorResumeNextStatement : ErrorHandlingStatement;

        private sealed record LabelStatement(string Label) : ErrorHandlingStatement;

        private sealed record ResumeStatement(string? TargetLabel) : ErrorHandlingStatement;

        private sealed record ResumeNextStatement : ErrorHandlingStatement;

        private sealed record ExecutableStatement(string Text) : ErrorHandlingStatement;
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

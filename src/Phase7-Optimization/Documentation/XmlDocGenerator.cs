using System.Text;
using System.Text.RegularExpressions;

namespace BLML.Phase7Optimization.Documentation
{
    public class XmlDocGenerator
    {
        private static readonly Regex SignatureRegex = new(
            @"^\s*(?:(?<access>Public|Private|Friend|Static)\s+)?(?<kind>Function|Sub|Property\s+(?:Get|Let|Set))\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\((?<params>[^)]*)\)\s*(?:As\s+(?<returnType>[A-Za-z_][A-Za-z0-9_\.]*)\s*)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ParameterRegex = new(
            @"^\s*(?:(?<optional>Optional)\s+)?(?:(?<modifier>ByVal|ByRef|ParamArray)\s+)?(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?:\(\))?\s*(?:As\s+(?<type>[A-Za-z_][A-Za-z0-9_\.]*)\s*)?(?:=\s*(?<defaultValue>.+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex TaskCommentRegex = new(
            @"^(?<marker>TODO|FIXME|HACK|UNDONE)\s*:?
\s*(?<body>.*)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public GeneratedXmlDocumentation GenerateForProcedure(ProcedureDocumentationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var signature = ParseSignature(request.Signature);
            var templates = request.Templates ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var summaryLines = new List<string>();
            var taskComments = new List<string>();

            foreach (var commentLine in request.LeadingComments ?? Array.Empty<string>())
            {
                var normalizedComment = NormalizeCommentText(commentLine);
                if (string.IsNullOrWhiteSpace(normalizedComment))
                {
                    continue;
                }

                if (IsTaskComment(normalizedComment))
                {
                    taskComments.Add(NormalizeTaskComment(normalizedComment));
                }
                else
                {
                    summaryLines.Add(normalizedComment);
                }
            }

            if (summaryLines.Count == 0)
            {
                summaryLines.Add(GetTemplateOrDefault(templates, "summary", InferSummary(signature)));
            }

            var xmlBuilder = new StringBuilder();
            AppendSummary(xmlBuilder, summaryLines);

            foreach (var parameter in signature.Parameters)
            {
                var parameterText = GetTemplateOrDefault(
                    templates,
                    $"param:{parameter.Name}",
                    InferParameterDocumentation(parameter));
                xmlBuilder.AppendLine($"/// <param name=\"{EscapeXml(parameter.Name)}\">{EscapeXml(parameterText)}</param>");
            }

            if (signature.RequiresReturnDocumentation)
            {
                var returnsText = GetTemplateOrDefault(templates, "returns", InferReturnsDocumentation(signature));
                xmlBuilder.AppendLine($"/// <returns>{EscapeXml(returnsText)}</returns>");
            }

            return new GeneratedXmlDocumentation
            {
                XmlDocumentation = xmlBuilder.ToString().TrimEnd(),
                TaskComments = taskComments,
                Signature = signature
            };
        }

        public string NormalizeTaskComment(string comment)
        {
            var normalizedComment = NormalizeCommentText(comment);
            var match = TaskCommentRegex.Match(normalizedComment);
            if (!match.Success)
            {
                return $"// TODO: {normalizedComment}";
            }

            var marker = match.Groups["marker"].Value.ToUpperInvariant();
            if (marker is "FIXME" or "UNDONE")
            {
                marker = "TODO";
            }

            var body = match.Groups["body"].Value.Trim();
            return string.IsNullOrWhiteSpace(body)
                ? $"// {marker}"
                : $"// {marker}: {body}";
        }

        public Vb6ProcedureSignature ParseSignature(string signature)
        {
            if (string.IsNullOrWhiteSpace(signature))
            {
                throw new ArgumentException("A VB6 signature is required.", nameof(signature));
            }

            var match = SignatureRegex.Match(signature.Trim());
            if (!match.Success)
            {
                throw new ArgumentException($"Unsupported VB6 procedure signature: '{signature}'.", nameof(signature));
            }

            var kind = match.Groups["kind"].Value;
            var name = match.Groups["name"].Value;
            var parameters = ParseParameters(match.Groups["params"].Value);
            var returnType = match.Groups["returnType"].Value;

            return new Vb6ProcedureSignature
            {
                MemberName = name,
                MemberKind = kind,
                ReturnType = string.IsNullOrWhiteSpace(returnType) ? null : returnType,
                Parameters = parameters
            };
        }

        private static IReadOnlyList<Vb6ProcedureParameter> ParseParameters(string parameterList)
        {
            if (string.IsNullOrWhiteSpace(parameterList))
            {
                return Array.Empty<Vb6ProcedureParameter>();
            }

            return parameterList
                .Split(',')
                .Select(segment => segment.Trim())
                .Where(segment => !string.IsNullOrWhiteSpace(segment))
                .Select(ParseParameter)
                .ToArray();
        }

        private static Vb6ProcedureParameter ParseParameter(string parameter)
        {
            var match = ParameterRegex.Match(parameter);
            if (!match.Success)
            {
                throw new ArgumentException($"Unsupported VB6 parameter definition: '{parameter}'.", nameof(parameter));
            }

            return new Vb6ProcedureParameter
            {
                Name = match.Groups["name"].Value,
                Type = match.Groups["type"].Success ? match.Groups["type"].Value : "Variant",
                IsOptional = match.Groups["optional"].Success,
                Modifier = match.Groups["modifier"].Success ? match.Groups["modifier"].Value : "ByRef",
                DefaultValue = match.Groups["defaultValue"].Success ? match.Groups["defaultValue"].Value.Trim() : null
            };
        }

        private static void AppendSummary(StringBuilder builder, IReadOnlyList<string> summaryLines)
        {
            builder.AppendLine("/// <summary>");
            foreach (var line in summaryLines)
            {
                builder.AppendLine($"/// {EscapeXml(line)}");
            }
            builder.AppendLine("/// </summary>");
        }

        private static bool IsTaskComment(string comment)
        {
            return TaskCommentRegex.IsMatch(comment);
        }

        private static string NormalizeCommentText(string comment)
        {
            var trimmed = comment.Trim();
            if (trimmed.StartsWith("'", StringComparison.Ordinal))
            {
                trimmed = trimmed[1..].Trim();
            }

            return trimmed;
        }

        private static string InferSummary(Vb6ProcedureSignature signature)
        {
            var memberName = SplitWords(signature.MemberName);

            if (signature.MemberKind.StartsWith("Property", StringComparison.OrdinalIgnoreCase))
            {
                return $"Gets or sets {ToSentenceFragment(signature.MemberName)}.";
            }

            foreach (var (prefix, verb) in new[]
                     {
                         ("Get", "Gets"),
                         ("Set", "Sets"),
                         ("Create", "Creates"),
                         ("Build", "Builds"),
                         ("Generate", "Generates"),
                         ("Load", "Loads"),
                         ("Save", "Saves"),
                         ("Parse", "Parses"),
                         ("Convert", "Converts"),
                         ("Calculate", "Calculates"),
                         ("Find", "Finds")
                     })
            {
                if (signature.MemberName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && signature.MemberName.Length > prefix.Length)
                {
                    return $"{verb} {ToSentenceFragment(signature.MemberName[prefix.Length..])}.";
                }
            }

            return signature.MemberKind.Equals("Sub", StringComparison.OrdinalIgnoreCase)
                ? $"Executes {memberName.ToLowerInvariant()}."
                : $"Returns {memberName.ToLowerInvariant()}.";
        }

        private static string InferParameterDocumentation(Vb6ProcedureParameter parameter)
        {
            var description = $"The {ToSentenceFragment(parameter.Name)}.";
            return parameter.IsOptional ? $"Optional. {description}" : description;
        }

        private static string InferReturnsDocumentation(Vb6ProcedureSignature signature)
        {
            if (string.IsNullOrWhiteSpace(signature.ReturnType))
            {
                return "The computed result.";
            }

            return $"The {SplitWords(signature.ReturnType).ToLowerInvariant()} result.";
        }

        private static string GetTemplateOrDefault(IReadOnlyDictionary<string, string> templates, string key, string fallback)
        {
            return templates.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;
        }

        private static string SplitWords(string value)
        {
            return Regex.Replace(value, "([a-z0-9])([A-Z])", "$1 $2").Trim();
        }

        private static string ToSentenceFragment(string value)
        {
            return SplitWords(value).ToLowerInvariant();
        }

        private static string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("'", "&apos;", StringComparison.Ordinal);
        }
    }

    public sealed class ProcedureDocumentationRequest
    {
        public string Signature { get; init; } = string.Empty;

        public IReadOnlyList<string> LeadingComments { get; init; } = Array.Empty<string>();

        public IReadOnlyDictionary<string, string> Templates { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class GeneratedXmlDocumentation
    {
        public string XmlDocumentation { get; init; } = string.Empty;

        public IReadOnlyList<string> TaskComments { get; init; } = Array.Empty<string>();

        public Vb6ProcedureSignature? Signature { get; init; }
    }

    public sealed class Vb6ProcedureSignature
    {
        public string MemberName { get; init; } = string.Empty;

        public string MemberKind { get; init; } = string.Empty;

        public string? ReturnType { get; init; }

        public IReadOnlyList<Vb6ProcedureParameter> Parameters { get; init; } = Array.Empty<Vb6ProcedureParameter>();

        public bool RequiresReturnDocumentation =>
            MemberKind.StartsWith("Function", StringComparison.OrdinalIgnoreCase) ||
            MemberKind.Equals("Property Get", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class Vb6ProcedureParameter
    {
        public string Name { get; init; } = string.Empty;

        public string Type { get; init; } = "Variant";

        public string Modifier { get; init; } = "ByRef";

        public bool IsOptional { get; init; }

        public string? DefaultValue { get; init; }
    }
}

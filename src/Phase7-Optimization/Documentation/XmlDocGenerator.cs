using System;

namespace BLML.Phase7Optimization.Documentation
{
    public class XmlDocGenerator
    {
        public void GenerateDocs()
        {
            // Convert ' comments to /// XML docs
        }

        public DocumentationResult GenerateForProcedure(ProcedureDocumentationRequest req)
        {
            var result = new DocumentationResult();
            if (req == null)
            {
                result.XmlDocumentation = string.Empty;
                return result;
            }

            var sb = new System.Text.StringBuilder();

            // Summary
            sb.AppendLine("/// <summary>");
            string summary = null;
            if (req.Templates != null && req.Templates.TryGetValue("summary", out var t)) summary = t;
            if (summary == null && req.LeadingComments != null && req.LeadingComments.Count > 0)
            {
                // use first leading comment line(s) as summary
                foreach (var c in req.LeadingComments)
                {
                    var clean = c.Trim();
                    if (clean.StartsWith("'")) clean = clean.Substring(1).Trim();
                    sb.AppendLine("/// " + System.Security.SecurityElement.Escape(clean));
                }
            }
            else if (summary != null)
            {
                sb.AppendLine("/// " + System.Security.SecurityElement.Escape(summary));
            }
            sb.AppendLine("/// </summary>");

            // parameters
            var paramList = new List<(string Name, bool Optional)>();
            if (!string.IsNullOrEmpty(req.Signature))
            {
                var m = System.Text.RegularExpressions.Regex.Match(req.Signature, @"\((.*)\)");
                if (m.Success)
                {
                    var inside = m.Groups[1].Value;
                    var parts = inside.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts)
                    {
                        var pp = p.Trim();
                        var nameMatch = System.Text.RegularExpressions.Regex.Match(pp, @"ByVal\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (nameMatch.Success)
                        {
                            var name = nameMatch.Groups[1].Value;
                            var optional = pp.IndexOf("Optional", StringComparison.OrdinalIgnoreCase) >= 0;
                            paramList.Add((name, optional));
                        }
                    }
                }
            }

            foreach (var (Name, Optional) in paramList)
            {
                var key = $"param:{Name}";
                string text = null;
                if (req.Templates != null && req.Templates.TryGetValue(key, out var t2)) text = t2;
                if (text == null)
                {
                    text = (Optional ? "Optional. " : string.Empty) + $"The {Name}.";
                }
                sb.AppendLine($"/// <param name=\"{Name}\">{System.Security.SecurityElement.Escape(text)}</param>");
            }

            // returns
            string returnsText = null;
            if (req.Templates != null && req.Templates.TryGetValue("returns", out var rt)) returnsText = rt;
            if (returnsText == null)
            {
                if (!string.IsNullOrEmpty(req.Signature) && req.Signature.IndexOf("As Double", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    returnsText = "The double result.";
                }
                else if (!string.IsNullOrEmpty(req.Signature) && req.Signature.IndexOf("As String", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    returnsText = "The string result.";
                }
                else
                {
                    returnsText = "The result.";
                }
            }

            sb.AppendLine($"/// <returns>{System.Security.SecurityElement.Escape(returnsText)}</returns>");

            result.XmlDocumentation = sb.ToString();
            return result;
        }

        public string NormalizeTaskComment(string comment)
        {
            if (string.IsNullOrEmpty(comment)) return comment ?? string.Empty;
            var trimmed = comment.Trim();
            if (trimmed.StartsWith("'FIXME:", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("'TODO:", StringComparison.OrdinalIgnoreCase))
            {
                var payload = trimmed.Substring(1).Trim(); // remove leading '
                // Replace FIXME: with TODO:
                payload = System.Text.RegularExpressions.Regex.Replace(payload, "^FIXME:\\s*", "TODO: ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                payload = System.Text.RegularExpressions.Regex.Replace(payload, "^TODO:\\s*", "TODO: ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return "// " + payload.Substring(0).Trim();
            }
            return comment;
        }

    }

    public class ProcedureDocumentationRequest
    {
        public string Signature { get; set; } = string.Empty;
        public List<string>? LeadingComments { get; set; }
        public Dictionary<string, string>? Templates { get; set; }
    }

    public class DocumentationResult
    {
        public string XmlDocumentation { get; set; } = string.Empty;
    }
}

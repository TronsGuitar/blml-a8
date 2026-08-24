using BLML.Phase1Foundation.AST;

namespace BLML.Phase5ASPtoAngular.AspParser
{
    /// <summary>One `&lt;OBJECT RUNAT=Server SCOPE=... ID=... PROGID/CLASSID=...&gt;` declaration.</summary>
    public class GlobalAsaObjectDeclaration
    {
        public string Id { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty; // Session or Application
        public string? ProgId { get; set; }
        public string? ClassId { get; set; }
    }

    public class GlobalAsaPageNode
    {
        public MethodDeclarationNode? ApplicationOnStart { get; set; }
        public MethodDeclarationNode? ApplicationOnEnd { get; set; }
        public MethodDeclarationNode? SessionOnStart { get; set; }
        public MethodDeclarationNode? SessionOnEnd { get; set; }
        public List<MethodDeclarationNode> OtherSubs { get; } = new();
        public List<GlobalAsaObjectDeclaration> Objects { get; } = new();
        public List<string> Warnings { get; } = new();
    }

    /// <summary>
    /// Parses Global.asa: the four well-known application/session lifecycle events
    /// (each a plain `Sub ... End Sub` using the same VBScript grammar as page code,
    /// so it's parsed with the same <see cref="VBScriptParser"/>), plus `&lt;OBJECT
    /// RUNAT=Server&gt;` declarations that make a COM component available for the whole
    /// application/session scope. `&lt;SCRIPT LANGUAGE="VBScript" RUNAT="Server"&gt;` wrapper
    /// tags are stripped before parsing since they're markup, not VBScript.
    /// </summary>
    public class GlobalAsaParser
    {
        private static readonly System.Text.RegularExpressions.Regex ScriptBlockRegex = new(
            "<script[^>]*runat\\s*=\\s*[\"']?server[\"']?[^>]*>(.*?)</script>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

        private static readonly System.Text.RegularExpressions.Regex ObjectTagRegex = new(
            "<object\\b([^>]*)>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static readonly System.Text.RegularExpressions.Regex AttrRegex = new(
            "(\\w+)\\s*=\\s*(?:\"([^\"]*)\"|'([^']*)'|([^\\s>]+))",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public GlobalAsaPageNode Parse(string content)
        {
            var result = new GlobalAsaPageNode();

            foreach (System.Text.RegularExpressions.Match m in ObjectTagRegex.Matches(content))
            {
                result.Objects.Add(ParseObjectTag(m.Groups[1].Value));
            }

            var vbParser = new VBScriptParser();
            var scriptMatches = ScriptBlockRegex.Matches(content);
            var codeToParse = scriptMatches.Count > 0
                ? string.Join("\n", scriptMatches.Select(m => m.Groups[1].Value))
                : content; // tolerate a Global.asa with bare Subs and no <script> wrapper

            var statements = vbParser.ParseCodeText(codeToParse);
            result.Warnings.AddRange(vbParser.Warnings);

            foreach (var stmt in statements)
            {
                if (stmt is not MethodDeclarationNode method) continue;
                switch (method.Name.ToLowerInvariant())
                {
                    case "application_onstart": result.ApplicationOnStart = method; break;
                    case "application_onend": result.ApplicationOnEnd = method; break;
                    case "session_onstart": result.SessionOnStart = method; break;
                    case "session_onend": result.SessionOnEnd = method; break;
                    default: result.OtherSubs.Add(method); break;
                }
            }

            return result;
        }

        private static GlobalAsaObjectDeclaration ParseObjectTag(string attrText)
        {
            var decl = new GlobalAsaObjectDeclaration();
            foreach (System.Text.RegularExpressions.Match m in AttrRegex.Matches(attrText))
            {
                var name = m.Groups[1].Value.ToLowerInvariant();
                var value = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Success ? m.Groups[3].Value : m.Groups[4].Value;
                switch (name)
                {
                    case "id": decl.Id = value; break;
                    case "scope": decl.Scope = value; break;
                    case "progid": decl.ProgId = value; break;
                    case "classid": decl.ClassId = value; break;
                }
            }
            return decl;
        }
    }
}

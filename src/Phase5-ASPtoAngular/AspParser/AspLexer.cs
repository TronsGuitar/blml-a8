using System.Text;
using System.Text.RegularExpressions;

namespace BLML.Phase5ASPtoAngular.AspParser
{
    public enum AspRegionType
    {
        Html,
        CodeBlock,
        OutputExpression,
        Directive,
        ServerComment,
        Include
    }

    public class AspRegion
    {
        public AspRegionType Type { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Line { get; set; }

        /// <summary>Populated only for Include regions: the raw file/virtual path attribute value.</summary>
        public string? IncludePath { get; set; }
        public bool IncludeIsVirtual { get; set; }
    }

    /// <summary>
    /// Splits a raw classic ASP file into an ordered list of regions: literal HTML,
    /// `&lt;% code %&gt;` blocks, `&lt;%= expr %&gt;` output expressions, `&lt;%@ ... %&gt;` directives,
    /// `&lt;%-- ... --%&gt;` server comments, and `&lt;!--#include ... --&gt;` directives.
    ///
    /// Classic ASP ambiguities handled here specifically:
    ///  - a `%&gt;` that appears *inside* a VBScript string literal inside a code block
    ///    (e.g. `&lt;% s = "50%&gt;" %&gt;`) must not be treated as the block terminator, so the
    ///    scanner tracks string-literal state (VBScript escapes quotes by doubling them)
    ///    while looking for the real close tag.
    ///  - `&lt;!--#include file="x.asp"--&gt;` / `virtual="/x.asp"` looks like an ordinary HTML
    ///    comment and must be distinguished from one before being emitted as literal Html.
    ///  - matching is case-insensitive and tolerant of extra whitespace around `=`,
    ///    since real-world ASP markup is inconsistent about both.
    /// </summary>
    public class AspLexer
    {
        private static readonly Regex IncludeRegex = new(
            @"<!--\s*#include\s+(file|virtual)\s*=\s*[""']([^""']+)[""']\s*-->",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public List<AspRegion> Tokenize(string content)
        {
            var regions = new List<AspRegion>();
            if (string.IsNullOrEmpty(content)) return regions;

            int index = 0;
            int line = 1;
            var htmlBuffer = new StringBuilder();

            void FlushHtml()
            {
                if (htmlBuffer.Length == 0) return;
                var text = htmlBuffer.ToString();
                var split = ExtractIncludes(text, line - CountNewlines(text));
                if (split is null)
                {
                    regions.Add(new AspRegion { Type = AspRegionType.Html, Text = text, Line = line });
                }
                else
                {
                    regions.AddRange(split);
                }
                htmlBuffer.Clear();
            }

            while (index < content.Length)
            {
                if (content[index] == '<' && index + 1 < content.Length && content[index + 1] == '%')
                {
                    FlushHtml();

                    int tagStart = index;
                    int cursor = index + 2;
                    AspRegionType type = AspRegionType.CodeBlock;

                    if (cursor + 1 < content.Length && content[cursor] == '-' && content[cursor + 1] == '-')
                    {
                        type = AspRegionType.ServerComment;
                        cursor += 2;
                    }
                    else if (cursor < content.Length && content[cursor] == '=')
                    {
                        type = AspRegionType.OutputExpression;
                        cursor += 1;
                    }
                    else if (cursor < content.Length && content[cursor] == '@')
                    {
                        type = AspRegionType.Directive;
                        cursor += 1;
                    }

                    string closer = type == AspRegionType.ServerComment ? "--%>" : "%>";
                    int end = FindBlockEnd(content, cursor, closer);
                    string inner = end >= cursor ? content.Substring(cursor, end - cursor) : content.Substring(cursor);

                    regions.Add(new AspRegion { Type = type, Text = inner.Trim(), Line = line });

                    line += CountNewlines(content.Substring(tagStart, (end < 0 ? content.Length : end + closer.Length) - tagStart));
                    index = end < 0 ? content.Length : end + closer.Length;
                    continue;
                }

                if (content[index] == '\n') line++;
                htmlBuffer.Append(content[index]);
                index++;
            }

            FlushHtml();
            return regions;
        }

        /// <summary>
        /// Finds the index of `closer` in `content` starting at `from`, treating text
        /// inside VBScript double-quoted string literals (with `""` escaping) as opaque
        /// so a stray `%&gt;` embedded in a string doesn't prematurely end the code block.
        /// </summary>
        private static int FindBlockEnd(string content, int from, string closer)
        {
            bool inString = false;
            int i = from;
            while (i < content.Length)
            {
                char c = content[i];
                if (c == '"')
                {
                    if (inString && i + 1 < content.Length && content[i + 1] == '"')
                    {
                        i += 2;
                        continue;
                    }
                    inString = !inString;
                    i++;
                    continue;
                }

                if (!inString && c == closer[0] && i + closer.Length <= content.Length &&
                    content.Substring(i, closer.Length) == closer)
                {
                    return i;
                }

                i++;
            }
            return -1;
        }

        /// <summary>
        /// A run of literal HTML can itself contain `&lt;!--#include--&gt;` directives, which are
        /// not real HTML comments even though they're spelled like one. Splits such a run
        /// into Html/Include regions in order; returns null when there are none, in which
        /// case the caller keeps the original single Html region.
        /// </summary>
        private static List<AspRegion>? ExtractIncludes(string html, int startLine)
        {
            var matches = IncludeRegex.Matches(html);
            if (matches.Count == 0) return null;

            var result = new List<AspRegion>();
            int cursor = 0;
            int line = startLine;
            foreach (Match m in matches)
            {
                if (m.Index > cursor)
                {
                    var before = html.Substring(cursor, m.Index - cursor);
                    result.Add(new AspRegion { Type = AspRegionType.Html, Text = before, Line = line });
                    line += CountNewlines(before);
                }

                result.Add(new AspRegion
                {
                    Type = AspRegionType.Include,
                    Text = m.Value,
                    Line = line,
                    IncludePath = m.Groups[2].Value,
                    IncludeIsVirtual = string.Equals(m.Groups[1].Value, "virtual", StringComparison.OrdinalIgnoreCase)
                });
                line += CountNewlines(m.Value);
                cursor = m.Index + m.Length;
            }

            if (cursor < html.Length)
            {
                result.Add(new AspRegion { Type = AspRegionType.Html, Text = html.Substring(cursor), Line = line });
            }

            return result;
        }

        private static int CountNewlines(string text)
        {
            int count = 0;
            foreach (var c in text) if (c == '\n') count++;
            return count;
        }
    }
}

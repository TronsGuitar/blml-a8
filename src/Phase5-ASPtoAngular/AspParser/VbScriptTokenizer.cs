using System.Text;

namespace BLML.Phase5ASPtoAngular.AspParser
{
    public enum VbsTokenKind
    {
        Keyword,
        Identifier,
        String,
        Number,
        Operator,
        NewLine,
        EndOfFile
    }

    public class VbsToken
    {
        public VbsTokenKind Kind { get; set; }
        public string Value { get; set; } = string.Empty;
        public int Line { get; set; }

        public bool IsKeyword(string word) => Kind == VbsTokenKind.Keyword && string.Equals(Value, word, StringComparison.OrdinalIgnoreCase);
        public bool IsOperator(string op) => Kind == VbsTokenKind.Operator && Value == op;
    }

    /// <summary>
    /// Tokenizes classic-ASP-flavored VBScript code (the text found inside `&lt;% %&gt;`
    /// blocks). Unlike <see cref="BLML.Phase1Foundation.Lexer.VB6Lexer"/>, this keeps
    /// explicit NewLine tokens (collapsing `_` line-continuations) because VBScript's
    /// single-line `If x Then y = 1` vs. block `If x Then` / ... / `End If` forms are
    /// only distinguishable by whether a newline follows `Then` - a genuine classic-ASP
    /// ambiguity that a newline-blind token stream can't resolve.
    /// </summary>
    public class VbScriptTokenizer
    {
        private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "And", "As", "Byref", "Byval", "Call", "Case", "Class", "Const", "Dim", "Do",
            "Each", "Else", "ElseIf", "Empty", "End", "Eqv", "Erase", "Error", "Exit",
            "False", "For", "Function", "Get", "GoTo", "If", "Imp", "In", "Is", "Let",
            "Like", "Loop", "Me", "Mod", "New", "Next", "Not", "Nothing", "Null", "On",
            "Option", "Or", "Preserve", "Private", "Property", "Public", "ReDim",
            "Rem", "Resume", "Select", "Set", "Step", "Sub", "Then", "To", "True",
            "Until", "Wend", "While", "With", "Xor"
        };

        public static bool IsKeyword(string identifier) => Keywords.Contains(identifier);

        public List<VbsToken> Tokenize(string code)
        {
            var tokens = new List<VbsToken>();
            int i = 0;
            int line = 1;
            bool lineHasContent = false;

            void EmitNewLineIfNeeded()
            {
                if (lineHasContent)
                {
                    tokens.Add(new VbsToken { Kind = VbsTokenKind.NewLine, Value = "\n", Line = line });
                }
            }

            while (i < code.Length)
            {
                char c = code[i];

                if (c == '\r') { i++; continue; }

                if (c == '\n')
                {
                    EmitNewLineIfNeeded();
                    lineHasContent = false;
                    line++;
                    i++;
                    continue;
                }

                if (c == ' ' || c == '\t') { i++; continue; }

                // Line continuation: a lone `_` immediately before end-of-line suppresses the newline.
                if (c == '_' && LooksLikeLineContinuation(code, i))
                {
                    i++;
                    while (i < code.Length && (code[i] == ' ' || code[i] == '\t')) i++;
                    if (i < code.Length && code[i] == '\r') i++;
                    if (i < code.Length && code[i] == '\n') { i++; line++; }
                    continue;
                }

                if (c == '\'' || StartsWithWord(code, i, "Rem"))
                {
                    // Comment: skip to end of line (do not emit a token; the comment carries no semantics we act on).
                    while (i < code.Length && code[i] != '\n') i++;
                    continue;
                }

                if (c == '"')
                {
                    tokens.Add(new VbsToken { Kind = VbsTokenKind.String, Value = ReadString(code, ref i), Line = line });
                    lineHasContent = true;
                    continue;
                }

                if (char.IsDigit(c) || (c == '.' && i + 1 < code.Length && char.IsDigit(code[i + 1])))
                {
                    tokens.Add(new VbsToken { Kind = VbsTokenKind.Number, Value = ReadNumber(code, ref i), Line = line });
                    lineHasContent = true;
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    var word = ReadIdentifier(code, ref i);
                    tokens.Add(new VbsToken
                    {
                        Kind = IsKeyword(word) ? VbsTokenKind.Keyword : VbsTokenKind.Identifier,
                        Value = word,
                        Line = line
                    });
                    lineHasContent = true;
                    continue;
                }

                var op = ReadOperator(code, ref i);
                if (op != null)
                {
                    tokens.Add(new VbsToken { Kind = VbsTokenKind.Operator, Value = op, Line = line });
                    lineHasContent = true;
                    // A colon is an explicit same-line statement separator, semantically like a newline.
                    if (op == ":")
                    {
                        tokens.Add(new VbsToken { Kind = VbsTokenKind.NewLine, Value = ":", Line = line });
                        lineHasContent = false;
                    }
                    continue;
                }

                // Unrecognized character: skip it rather than fail the whole page's parse.
                i++;
            }

            EmitNewLineIfNeeded();
            tokens.Add(new VbsToken { Kind = VbsTokenKind.EndOfFile, Value = string.Empty, Line = line });
            return tokens;
        }

        private static bool LooksLikeLineContinuation(string code, int i)
        {
            int j = i + 1;
            while (j < code.Length && (code[j] == ' ' || code[j] == '\t')) j++;
            return j < code.Length && (code[j] == '\r' || code[j] == '\n');
        }

        private static bool StartsWithWord(string code, int i, string word)
        {
            if (i + word.Length > code.Length) return false;
            if (!string.Equals(code.Substring(i, word.Length), word, StringComparison.OrdinalIgnoreCase)) return false;
            int after = i + word.Length;
            return after >= code.Length || !char.IsLetterOrDigit(code[after]);
        }

        private static string ReadString(string code, ref int i)
        {
            var sb = new StringBuilder();
            i++; // opening quote
            while (i < code.Length)
            {
                if (code[i] == '"')
                {
                    if (i + 1 < code.Length && code[i + 1] == '"') { sb.Append('"'); i += 2; continue; }
                    i++;
                    break;
                }
                sb.Append(code[i]);
                i++;
            }
            return sb.ToString();
        }

        private static string ReadNumber(string code, ref int i)
        {
            int start = i;
            bool hasDecimal = false;
            while (i < code.Length && (char.IsDigit(code[i]) || (code[i] == '.' && !hasDecimal)))
            {
                if (code[i] == '.') hasDecimal = true;
                i++;
            }
            return code.Substring(start, i - start);
        }

        private static string ReadIdentifier(string code, ref int i)
        {
            int start = i;
            while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_')) i++;
            return code.Substring(start, i - start);
        }

        private static readonly string[] MultiCharOperators = { "<=", ">=", "<>" };

        private static string? ReadOperator(string code, ref int i)
        {
            foreach (var op in MultiCharOperators)
            {
                if (i + op.Length <= code.Length && code.Substring(i, op.Length) == op)
                {
                    i += op.Length;
                    return op;
                }
            }

            const string singleChar = "+-*/\\^&=<>(),.:";
            if (singleChar.IndexOf(code[i]) >= 0)
            {
                var s = code[i].ToString();
                i++;
                return s;
            }

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLML.Phase1Foundation.SymbolTable;

namespace BLML.Phase1Foundation.Lexer
{
    public enum TokenType
    {
        Keyword,
        Identifier,
        StringLiteral,
        NumberLiteral,
        Operator,
        Delimiter,
        Comment,
        LineTerminator,
        Whitespace,
        Unknown
    }

    public class VB6Token
    {
        public string Value { get; set; }
        public TokenType Type { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public class VB6Lexer
    {
<<<<<<< HEAD
        // Note: VB6 built-in functions (Len, Mid, Left, Right, Trim, etc.) are NOT included here
        // because they are functions, not keywords. They should be treated as identifiers.
        private readonly HashSet<string> reservedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "And", "As", "ByRef", "ByVal", "Call", "Case", "Close",
            "Const", "Declare", "Dim", "Do", "Each", "Else", "ElseIf", "End",
            "Enum", "Erase", "Error", "Event", "Exit", "False", "For", "Friend",
            "Function", "Get", "GoSub", "GoTo", "If", "Implements", "In",
            "Is", "Let", "Like", "Lock", "Loop", "Me", "Mod", "New",
            "Next", "Not", "Nothing", "Null", "On", "Option", "Optional", "Or",
            "ParamArray", "Private", "Property", "Public", "RaiseEvent",
            "ReDim", "REM", "Resume", "Return", "Seek", "Select", "Set", "Static",
            "Step", "Stop", "Sub", "Then", "To", "True", "Type",
            "Unload", "Until", "Variant", "Wend", "While", "With", "WithEvents",
            "Xor", "Eqv", "Imp"
=======
        private readonly HashSet<string> reservedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "And", "As", "Beep", "Binary", "ByRef", "ByVal", "Call", "Case", "Close",
            "Const", "Date", "Declare", "Dim", "Do", "Each", "Else", "ElseIf", "End",
            "Enum", "Erase", "Error", "Event", "Exit", "False", "For", "Friend",
            "Function", "Get", "GoSub", "GoTo", "If", "Implements", "In", "Input",
            "Is", "Kill", "Len", "Let", "Like", "Lock", "Loop", "Me", "Mod", "New",
            "Next", "Not", "Nothing", "Null", "On", "Option", "Optional", "Or",
            "ParamArray", "Print", "Private", "Property", "Public", "RaiseEvent",
            "ReDim", "REM", "Resume", "Return", "Seek", "Select", "Set", "Static",
            "Step", "Stop", "String", "Sub", "Then", "Time", "To", "True", "Type",
            "Unload", "Until", "Variant", "Wend", "While", "With", "WithEvents",
            "Write", "Xor", "Eqv", "Imp"
>>>>>>> 2e0740d (The prototype files)
        };

        private readonly HashSet<string> predefinedConstants = new HashSet<string>(SymbolTableBuilder.PredefinedConstants.Keys, StringComparer.OrdinalIgnoreCase);

        public List<VB6Token> Tokenize(string code)
        {
            var tokens = new List<VB6Token>();
            int line = 1;
            int column = 1;
            int index = 0;

            while (index < code.Length)
            {
                char current = code[index];

                // Skip whitespace but keep track of position
                if (char.IsWhiteSpace(current))
                {
                    if (current == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }
                    index++;
                    continue;
                }

                // Handle comments
                if (current == '\'' || (current == 'R' && index + 2 < code.Length && 
                    code.Substring(index, 3).Equals("REM", StringComparison.OrdinalIgnoreCase)))
                {
                    var comment = ParseComment(code, ref index, ref line, ref column);
                    tokens.Add(new VB6Token
                    {
                        Type = TokenType.Comment,
                        Value = comment,
                        Line = line,
                        Column = column
                    });
                    continue;
                }

                // Handle string literals
                if (current == '"')
                {
                    var str = ParseStringLiteral(code, ref index, ref line, ref column);
                    tokens.Add(new VB6Token
                    {
                        Type = TokenType.StringLiteral,
                        Value = str,
                        Line = line,
                        Column = column
                    });
                    continue;
                }

                // Handle numbers
                if (char.IsDigit(current) || (current == '.' && index + 1 < code.Length && 
                    char.IsDigit(code[index + 1])))
                {
                    var number = ParseNumber(code, ref index, ref column);
                    tokens.Add(new VB6Token
                    {
                        Type = TokenType.NumberLiteral,
                        Value = number,
                        Line = line,
                        Column = column
                    });
                    continue;
                }

                // Handle identifiers and keywords
                if (char.IsLetter(current) || current == '_')
                {
                    var identifier = ParseIdentifier(code, ref index, ref column);
                    var tokenType = reservedKeywords.Contains(identifier) ? TokenType.Keyword :
                                    predefinedConstants.Contains(identifier) ? TokenType.Identifier : // Treat as identifier for now, or add PredefinedConstant to TokenType
                                    TokenType.Identifier;
                    
                    tokens.Add(new VB6Token
                    {
                        Type = tokenType,
                        Value = identifier,
                        Line = line,
                        Column = column
                    });
                    continue;
                }

                // Handle operators and delimiters
                var op = ParseOperator(code, ref index, ref column);
                if (!string.IsNullOrEmpty(op))
                {
                    tokens.Add(new VB6Token
                    {
                        Type = TokenType.Operator,
                        Value = op,
                        Line = line,
                        Column = column
                    });
                    continue;
                }

                // Unknown character
                tokens.Add(new VB6Token
                {
                    Type = TokenType.Unknown,
                    Value = current.ToString(),
                    Line = line,
                    Column = column
                });
                index++;
                column++;
            }

            return tokens;
        }

        private string ParseComment(string code, ref int index, ref int line, ref int column)
        {
            var comment = new StringBuilder();
            var isREM = code.Substring(index).StartsWith("REM", StringComparison.OrdinalIgnoreCase);
            
            // Skip the comment marker (' or REM)
            index += isREM ? 3 : 1;
            column += isREM ? 3 : 1;

            // Read until end of line
            while (index < code.Length && code[index] != '\n')
            {
                comment.Append(code[index]);
                index++;
                column++;
            }

            // Handle line ending
            if (index < code.Length && code[index] == '\n')
            {
                index++;
                line++;
                column = 1;
            }

            return comment.ToString();
        }

        private string ParseStringLiteral(string code, ref int index, ref int line, ref int column)
        {
            var str = new StringBuilder();
            index++; // Skip opening quote
            column++;

            while (index < code.Length)
            {
                if (code[index] == '"')
                {
                    if (index + 1 < code.Length && code[index + 1] == '"')
                    {
                        // Double quotes escape sequence
                        str.Append('"');
                        index += 2;
                        column += 2;
                    }
                    else
                    {
                        // End of string
                        index++;
                        column++;
                        break;
                    }
                }
                else if (code[index] == '\n')
                {
                    line++;
                    column = 1;
                    index++;
                }
                else
                {
                    str.Append(code[index]);
                    index++;
                    column++;
                }
            }

            return str.ToString();
        }

        private string ParseNumber(string code, ref int index, ref int column)
        {
            var number = new StringBuilder();
            bool hasDecimal = false;
            bool hasExponent = false;

            // Handle leading signs
            if (index < code.Length && (code[index] == '+' || code[index] == '-'))
            {
                number.Append(code[index]);
                index++;
                column++;
            }

            while (index < code.Length)
            {
                char current = code[index];

                if (char.IsDigit(current))
                {
                    number.Append(current);
                }
                else if (current == '.' && !hasDecimal && !hasExponent)
                {
                    hasDecimal = true;
                    number.Append(current);
                }
                else if ((current == 'e' || current == 'E') && !hasExponent)
                {
                    hasExponent = true;
                    number.Append(current);

                    // Handle exponent sign
                    if (index + 1 < code.Length && (code[index + 1] == '+' || code[index + 1] == '-'))
                    {
                        index++;
                        column++;
                        number.Append(code[index]);
                    }
                }
                else if (char.IsLetter(current))
                {
                    // Handle type suffixes
                    HandleTypeSuffix(current, number);
                    index++;
                    column++;
                    break;
                }
                else
                {
                    break;
                }

                index++;
                column++;
            }

            return number.ToString();
        }

        private void HandleTypeSuffix(char suffix, StringBuilder number)
        {
            switch (char.ToUpper(suffix))
            {
                case 'D': // Double
                case 'R': // Double in some contexts
                    number.Append('D');
                    break;
                case 'F': // Single
                    number.Append('F');
                    break;
                case 'L': // Long
                    number.Append('L');
                    break;
                case 'S': // Short
                    // Will be handled during type conversion
                    break;
                case 'I': // Integer
                    // Will be handled during type conversion
                    break;
                case '@': // Decimal
                    number.Append('M');
                    break;
            }
        }

        private string ParseIdentifier(string code, ref int index, ref int column)
        {
            var identifier = new StringBuilder();

            while (index < code.Length)
            {
                char current = code[index];

                if (char.IsLetterOrDigit(current) || current == '_')
                {
                    identifier.Append(current);
                    index++;
                    column++;
                }
                else
                {
                    break;
                }
            }

            return identifier.ToString();
        }

        private string ParseOperator(string code, ref int index, ref int column)
        {
            // List of possible multi-character operators
            string[] multiCharOps = new[]
            {
                "<=", ">=", "<>", "+=", "-=", "*=", "/=", "\\=", "&=", "^=",
                "==", "=>", "->", "<<", ">>", "||", "&&"
            };

            foreach (var op in multiCharOps)
            {
                if (code.Length >= index + op.Length &&
                    code.Substring(index, op.Length) == op)
                {
                    index += op.Length;
                    column += op.Length;
                    return op;
                }
            }

            // Single character operators
            if ("+-*/<>=&|^!~(){}[],.;:\\".Contains(code[index]))
            {
                char op = code[index];
                index++;
                column++;
                return op.ToString();
            }

            return null;
        }
    }
}

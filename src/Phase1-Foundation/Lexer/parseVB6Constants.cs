using System;
using System.Collections.Generic;

namespace BLML.Phase1Foundation.Lexer.ConstantsParser
{
    public enum TokenType
    {
        Identifier,
        PredefinedConstant,
        StringLiteral,
        Operator,
        EndOfFile,
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public override string ToString()
        {
            return $"{Type}: {Value} (Pos: {Position})";
        }
    }

    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private readonly HashSet<string> _predefinedConstants;

        public Lexer(string input)
        {
            _input = input;
            _position = 0;
            _predefinedConstants = new HashSet<string>
            {
                "vbCrLf", "vbTab", "vbNewLine", "vbNullChar", "vbCr", "vbLf", "vbFormFeed", "vbVerticalTab", "vbNullString",
                "vbBlack", "vbRed", "vbGreen", "vbYellow", "vbBlue", "vbMagenta", "vbCyan", "vbWhite",
                "vbBinaryCompare", "vbTextCompare", "vbDatabaseCompare",
                "vbGeneralDate", "vbLongDate", "vbShortDate", "vbLongTime", "vbShortTime",
                "vbObjectError"
            };
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char current = _input[_position];

                if (char.IsWhiteSpace(current))
                {
                    _position++;
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    string identifier = ReadIdentifier();
                    if (_predefinedConstants.Contains(identifier))
                    {
                        tokens.Add(new Token(TokenType.PredefinedConstant, identifier, _position));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Identifier, identifier, _position));
                    }
                    continue;
                }

                if (current == '"' || current == '\'')
                {
                    string str = ReadStringLiteral();
                    tokens.Add(new Token(TokenType.StringLiteral, str, _position));
                    continue;
                }

                switch (current)
                {
                    case '&':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '=':
                    case '<':
                    case '>':
                    case '(': 
                    case ')':
                        tokens.Add(new Token(TokenType.Operator, current.ToString(), _position));
                        _position++;
                        break;
                    default:
                        // Be more permissive or skip unknown chars? For now, throw as in original
                        throw new Exception($"Unrecognized character '{current}' at position {_position}");
                }
            }

            tokens.Add(new Token(TokenType.EndOfFile, string.Empty, _position));
            return tokens;
        }

        private string ReadIdentifier()
        {
            int start = _position;
            while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
            {
                _position++;
            }
            return _input.Substring(start, _position - start);
        }

        private string ReadStringLiteral()
        {
            char quote = _input[_position];
            _position++;
            int start = _position;
            while (_position < _input.Length && _input[_position] != quote)
            {
                _position++;
            }
            if (_position >= _input.Length)
                throw new Exception("Unterminated string literal");

            string str = _input.Substring(start, _position - start);
            _position++;
            return str;
        }
    }

    public static class SymbolTable
    {
        public static readonly Dictionary<string, string> PredefinedConstants = new Dictionary<string, string>
        {
            { "vbCrLf", "\r\n" },
            { "vbTab", "\t" },
            { "vbNewLine", "\r\n" },
            { "vbNullChar", "\0" },
            { "vbCr", "\r" },
            { "vbLf", "\n" },
            { "vbFormFeed", "\f" },
            { "vbVerticalTab", "\v" },
            { "vbNullString", null }
        };
    }

    public abstract class Expression
    {
    }

    public class StringLiteralExpression : Expression
    {
        public string Value { get; }
        public StringLiteralExpression(string value) { Value = value; }
    }

    public class PredefinedConstantExpression : Expression
    {
        public string ConstantName { get; }
        public string Value { get; }
        public PredefinedConstantExpression(string name, string value) { ConstantName = name; Value = value; }
    }
    
    public class IdentifierExpression : Expression
    {
        public string Name { get; }
        public IdentifierExpression(string name) { Name = name; }
    }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public string Operator { get; }
        public Expression Right { get; }
        public BinaryExpression(Expression left, string op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _current = 0;
        }

        public Expression Parse()
        {
            return ParseExpression();
        }

        private Expression ParseExpression()
        {
            Expression left = ParsePrimary();

            while (Match(TokenType.Operator) && CurrentToken().Value == "&")
            {
                string op = Consume(TokenType.Operator).Value;
                Expression right = ParsePrimary();
                left = new BinaryExpression(left, op, right);
            }

            return left;
        }

        private Expression ParsePrimary()
        {
            Token token = CurrentToken();

            if (token.Type == TokenType.StringLiteral)
            {
                Consume(TokenType.StringLiteral);
                return new StringLiteralExpression(token.Value);
            }
            else if (token.Type == TokenType.PredefinedConstant)
            {
                return ParsePredefinedConstant();
            }
            else if (token.Type == TokenType.Identifier)
            {
                Consume(TokenType.Identifier);
                return new IdentifierExpression(token.Value);
            }

            throw new Exception($"Unexpected token {token.Type} at position {token.Position}");
        }

        private PredefinedConstantExpression ParsePredefinedConstant()
        {
            Token token = Consume(TokenType.PredefinedConstant);
            if (SymbolTable.PredefinedConstants.TryGetValue(token.Value, out string value))
            {
                return new PredefinedConstantExpression(token.Value, value);
            }
            // Allow treating it as an identifier if not in map but tokenized as predefined (edge case)
            // or just return empty/default
             return new PredefinedConstantExpression(token.Value, ""); 
        }
        
        // This method was unused in the provided snippet but referenced in logic
        private Expression ParseConstantExpression()
        {
            Token token = CurrentToken();
            if (token.Type == TokenType.PredefinedConstant)
            {
                return ParsePredefinedConstant();
            }
            else if (token.Type == TokenType.Identifier)
            {
                Consume(TokenType.Identifier);
                return new IdentifierExpression(token.Value);
            }
            throw new Exception($"Expected a constant at position {token.Position}");
        }

        private bool Match(TokenType type)
        {
            if (IsAtEnd()) return false;
            return CurrentToken().Type == type;
        }

        private Token Consume(TokenType type)
        {
            if (Match(type))
                return _tokens[_current++];
            throw new Exception($"Expected token {type} at position {_current}");
        }

        private Token CurrentToken()
        {
            return _tokens[_current];
        }

        private bool IsAtEnd()
        {
            return _current >= _tokens.Count || _tokens[_current].Type == TokenType.EndOfFile;
        }
    }
}

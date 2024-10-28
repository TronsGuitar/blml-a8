
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace VB6ToCSharpTranspiler
{
    public class VB6Parser
    {
        private readonly HashSet<string> reservedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AddHandler", "AddressOf", "Alias", "And", "AndAlso", "As", "Boolean",
            "ByRef", "Byte", "ByVal", "Call", "Case", "Catch", "CBool", "CByte",
            "CChar", "CDate", "CDbl", "CDec", "Char", "CInt", "Class", "CLng",
            "CObj", "Const", "Continue", "CSByte", "CShort", "CSng", "CStr",
            "CType", "CUInt", "CULng", "CUShort", "Date", "Decimal", "Declare",
            "Default", "Delegate", "Dim", "DirectCast", "Do", "Double", "Each",
            "Else", "ElseIf", "End", "EndIf", "Enum", "Erase", "Error", "Event",
            "Exit", "False", "Finally", "For", "Friend", "Function", "Get",
            "GetType", "GetXMLNamespace", "Global", "GoSub", "GoTo", "Handles",
            "If", "Implements", "Imports", "In", "Inherits", "Integer", "Interface",
            "Is", "IsNot", "Let", "Lib", "Like", "Long", "Loop", "Me", "Mod",
            "Module", "MustInherit", "MustOverride", "MyBase", "MyClass", "Namespace",
            "Narrowing", "New", "Next", "Not", "Nothing", "NotInheritable",
            "NotOverridable", "Object", "Of", "On", "Operator", "Option", "Optional",
            "Or", "OrElse", "Out", "Overloads", "Overridable", "Overrides",
            "ParamArray", "Partial", "Private", "Property", "Protected", "Public",
            "RaiseEvent", "ReadOnly", "ReDim", "REM", "RemoveHandler", "Resume",
            "Return", "SByte", "Select", "Set", "Shadows", "Shared", "Short",
            "Single", "Static", "Step", "Stop", "String", "Structure", "Sub",
            "SyncLock", "Then", "Throw", "To", "True", "Try", "TryCast", "TypeOf",
            "UInteger", "ULong", "UShort", "Using", "Variant", "Wend", "When",
            "While", "Widening", "With", "WithEvents", "WriteOnly", "Xor"
        };

        private class VB6Token
        {
            public string Value { get; set; }
            public TokenType Type { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }
        }

        private enum TokenType
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

        public class VB6SyntaxNode
        {
            public NodeType Type { get; set; }
            public string Value { get; set; }
            public List<VB6SyntaxNode> Children { get; set; } = new List<VB6SyntaxNode>();
            public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        }

        public enum NodeType
        {
            Module,
            Class,
            Function,
            Sub,
            Property,
            Declaration,
            Statement,
            Expression,
            Type,
            Variable
        }

        private class Context
        {
            public Stack<VB6SyntaxNode> Scope { get; } = new Stack<VB6SyntaxNode>();
            public Dictionary<string, VB6SyntaxNode> SymbolTable { get; } = new Dictionary<string, VB6SyntaxNode>();
            public List<string> Errors { get; } = new List<string>();
        }

        public class TranspilerResult
        {
            public string CSharpCode { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
        }

        private readonly Context context = new Context();
        private List<VB6Token> tokens = new List<VB6Token>();
        private int currentTokenIndex = 0;

        public TranspilerResult TranspileFile(string vb6Code)
        {
            var result = new TranspilerResult();
            
            try
            {
                // Lexical analysis
                Tokenize(vb6Code);

                // Syntax analysis and AST construction
                var ast = ParseModule();

                // Symbol table construction
                BuildSymbolTable(ast);

                // Type checking and semantic analysis
                PerformSemanticAnalysis(ast);

                // Code generation
                result.CSharpCode = GenerateCSharpCode(ast);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Transpilation failed: {ex.Message}");
            }

            return result;
        }

        private void Tokenize(string code)
        {
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
                    var tokenType = reservedKeywords.Contains(identifier) ? 
                        TokenType.Keyword : TokenType.Identifier;
                    
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
        }

        private VB6SyntaxNode ParseModule()
        {
            var moduleNode = new VB6SyntaxNode
            {
                Type = NodeType.Module,
                Value = "Module"
            };

            while (currentTokenIndex < tokens.Count)
            {
                var declaration = ParseDeclaration();
                if (declaration != null)
                {
                    moduleNode.Children.Add(declaration);
                }
            }

            return moduleNode;
        }

        private VB6SyntaxNode ParseDeclaration()
        {
            var token = PeekToken();
            if (token == null) return null;

            switch (token.Value.ToLowerInvariant())
            {
                case "class":
                    return ParseClass();
                case "function":
                    return ParseFunction();
                case "sub":
                    return ParseSub();
                case "property":
                    return ParseProperty();
                case "dim":
                case "private":
                case "public":
                case "friend":
                    return ParseVariableDeclaration();
                default:
                    SkipToken();
                    return null;
            }
        }

        private void BuildSymbolTable(VB6SyntaxNode node)
        {
            switch (node.Type)
            {
                case NodeType.Variable:
                case NodeType.Function:
                case NodeType.Sub:
                case NodeType.Property:
                    context.SymbolTable[node.Value] = node;
                    break;
            }

            foreach (var child in node.Children)
            {
                BuildSymbolTable(child);
            }
        }

        private void PerformSemanticAnalysis(VB6SyntaxNode node)
        {
            // Check for undefined variables
            if (node.Type == NodeType.Variable && !context.SymbolTable.ContainsKey(node.Value))
            {
                context.Errors.Add($"Undefined variable: {node.Value}");
            }

            // Check for type compatibility
            if (node.Type == NodeType.Expression)
            {
                // Implement type checking logic here
            }

            foreach (var child in node.Children)
            {
                PerformSemanticAnalysis(child);
            }
        }

        private string GenerateCSharpCode(VB6SyntaxNode node)
        {
            var compilation = CSharpCompilation.Create("VB6Converted")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var syntaxTree = CSharpSyntaxTree.Create(
                GenerateCompilationUnit(node)
            );

            return syntaxTree.ToString();
        }

        private CompilationUnitSyntax GenerateCompilationUnit(VB6SyntaxNode node)
        {
            var usings = new List<UsingDirectiveSyntax>
            {
                SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName("System")
                )
            };

            var members = new List<MemberDeclarationSyntax>();
            foreach (var child in node.Children)
            {
                var member = GenerateMember(child);
                if (member != null)
                {
                    members.Add(member);
                }
            }

            return SyntaxFactory.CompilationUnit()
                .AddUsings(usings.ToArray())
                .AddMembers(members.ToArray());
        }

        private MemberDeclarationSyntax GenerateMember(VB6SyntaxNode node)
        {
            switch (node.Type)
            {
                case NodeType.Class:
                    return GenerateClass(node);
                case NodeType.Function:
                    return GenerateMethod(node, true);
                case NodeType.Sub:
                    return GenerateMethod(node, false);
                case NodeType.Property:
                    return GenerateProperty(node);
                default:
                    return null;
            }
        }

        private ClassDeclarationSyntax GenerateClass(VB6SyntaxNode node)
        {
            var members = new List<MemberDeclarationSyntax>();
            foreach (var child in node.Children)
            {
                var member = GenerateMember(child);
                if (member != null)
                {
                    members.Add(member);
                }
            }

            return SyntaxFactory.ClassDeclaration(node.Value)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(members.ToArray());
        }

        private MethodDeclarationSyntax GenerateMethod(VB6SyntaxNode node, bool isFunction)
        {
            var returnType = isFunction ? 
                ParseVB6Type(node.Attributes["ReturnType"]) : 
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

            var parameters = new List<ParameterSyntax>();
            foreach (var param in node.Children.Where(c => c.Type == NodeType.Variable))
            {
                parameters.Add(SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier(param.Value))
                    .WithType(ParseVB6Type(param.Attributes["Type"]))
                );
            }

            var body = SyntaxFactory.Block();
            // TODO: Generate method body statements

            return SyntaxFactory.MethodDeclaration(returnType, node.Value)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(parameters.ToArray())
                .WithBody(body);
        }

        private PropertyDeclarationSyntax GenerateProperty(VB6SyntaxNode node)
        {
            var propertyType = ParseVB6Type(node.Attributes["Type"]);
            
            var accessors = new List<AccessorDeclarationSyntax>
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(SyntaxFactory.Block()),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(SyntaxFactory.Block())
            };

            return SyntaxFactory.PropertyDeclaration(propertyType, node.Value)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(accessors.ToArray());
        }
   
private TypeSyntax ParseVB6Type(string vb6Type)
        {
            switch (vb6Type?.ToLowerInvariant())
            {
                case "string":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.StringKeyword));
                case "integer":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.IntKeyword));
                case "long":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.LongKeyword));
                case "single":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.FloatKeyword));
                case "double":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
                case "boolean":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword));
                case "date":
                    return SyntaxFactory.IdentifierName("DateTime");
                case "variant":
                    return SyntaxFactory.IdentifierName("object");
                case "object":
                    return SyntaxFactory.IdentifierName("object");
                default:
                    // Handle custom types
                    return SyntaxFactory.IdentifierName(vb6Type ?? "object");
            }
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

        private VB6Token PeekToken()
        {
            return currentTokenIndex < tokens.Count ? tokens[currentTokenIndex] : null;
        }

        private void SkipToken()
        {
            currentTokenIndex++;
        }

        private VB6Token GetToken()
        {
            return currentTokenIndex < tokens.Count ? tokens[currentTokenIndex++] : null;
        }

        private bool Match(string value)
        {
            var token = PeekToken();
            if (token != null && token.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                currentTokenIndex++;
                return true;
            }
            return false;
        }

        private void HandleWithEvents()
        {
            // Transform VB6 WithEvents to C# event handling
            var token = PeekToken();
            if (token != null && token.Value.Equals("WithEvents", StringComparison.OrdinalIgnoreCase))
            {
                // Generate event handler infrastructure
                // This will be expanded in the next continuation
            }
        }

        private void HandleDefaultProperties()
        {
            // Handle VB6 default properties
            // This will be expanded in the next continuation
        }
    }
}
//Made with Santa Claude


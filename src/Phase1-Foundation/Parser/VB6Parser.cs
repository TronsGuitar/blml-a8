using System;
using System.Collections.Generic;
using System.Linq;
using BLML.Phase1Foundation.AST;
using BLML.Phase1Foundation.Lexer;
using BLML.Phase1Foundation.SymbolTable;
using BLML.Phase1Foundation.TypeInference;

namespace BLML.Phase1Foundation.Parser
{
    public class VB6Parser
    {
        private List<VB6Token> tokens = new List<VB6Token>();
        private int currentTokenIndex = 0;

        public class TranspilerResult
        {
            public string CSharpCode { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
        }

        public TranspilerResult TranspileFile(string vb6Code)
        {
            var result = new TranspilerResult();
            
            try
            {
                // Lexical analysis
                var lexer = new VB6Lexer();
                tokens = lexer.Tokenize(vb6Code);

                // Syntax analysis and AST construction
                var ast = ParseModule();

                // Symbol table construction
                var symbolTableBuilder = new SymbolTableBuilder();
                var symbolTable = symbolTableBuilder.BuildSymbolTable(ast);

                // Type checking and semantic analysis
                var typeEngine = new TypeInferenceEngine(symbolTable);
                result.Errors.AddRange(typeEngine.PerformSemanticAnalysis(ast));

                // Code generation
                if (result.Errors.Count == 0)
                {
                    var codeGen = new VB6CodeGenerator();
                    result.CSharpCode = codeGen.GenerateCSharpCode(ast);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Transpilation failed: {ex.Message}");
            }

            return result;
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

        // Note: The following methods are placeholders as they were missing from the original file 
        // but referenced in ParseDeclaration.
        
        private VB6SyntaxNode ParseClass()
        {
            SkipToken(); // Skip 'Class'
            var name = GetToken()?.Value ?? "UnknownClass";
            return new VB6SyntaxNode { Type = NodeType.Class, Value = name };
        }

        private VB6SyntaxNode ParseFunction()
        {
            SkipToken(); // Skip 'Function'
            var name = GetToken()?.Value ?? "UnknownFunction";
            return new VB6SyntaxNode { Type = NodeType.Function, Value = name };
        }

        private VB6SyntaxNode ParseSub()
        {
            SkipToken(); // Skip 'Sub'
            var name = GetToken()?.Value ?? "UnknownSub";
            return new VB6SyntaxNode { Type = NodeType.Sub, Value = name };
        }

        private VB6SyntaxNode ParseProperty()
        {
            SkipToken(); // Skip 'Property'
            var name = GetToken()?.Value ?? "UnknownProperty";
            return new VB6SyntaxNode { Type = NodeType.Property, Value = name };
        }

        private VB6SyntaxNode ParseVariableDeclaration()
        {
            SkipToken(); // Skip 'Dim', 'Public', etc.
            var name = GetToken()?.Value ?? "UnknownVariable";
            return new VB6SyntaxNode { Type = NodeType.Variable, Value = name };
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
    }
}

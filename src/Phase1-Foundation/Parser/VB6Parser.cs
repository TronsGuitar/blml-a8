<<<<<<< HEAD
<<<<<<<
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
                var rawAst = ParseModule();

                // Build Higher-level Semantic AST
                var astBuilder = new AstBuilder();
                var semanticAst = astBuilder.BuildModule(rawAst);

                // Symbol table construction
                var symbolTableBuilder = new SymbolTableBuilder();
                var symbolTable = symbolTableBuilder.BuildSymbolTable(rawAst);

                // Type checking and semantic analysis
                var typeEngine = new TypeInferenceEngine(symbolTable);
                result.Errors.AddRange(typeEngine.PerformSemanticAnalysis(rawAst));

                // Code generation
                if (result.Errors.Count == 0)
                {
                    var codeGen = new VB6CodeGenerator();
                    result.CSharpCode = codeGen.GenerateCSharpCode(semanticAst);
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
            var funcNode = new VB6SyntaxNode { Type = NodeType.Function, Value = name };
            
            ParseParameters(funcNode);
            if (Match("As"))
            {
                funcNode.Attributes["ReturnType"] = GetToken()?.Value ?? "Variant";
            }

            ParseMethodBody(funcNode, "Function");
            return funcNode;
        }

        private VB6SyntaxNode ParseSub()
        {
            SkipToken(); // Skip 'Sub'
            var name = GetToken()?.Value ?? "UnknownSub";
            var subNode = new VB6SyntaxNode { Type = NodeType.Sub, Value = name };

            ParseParameters(subNode);
            ParseMethodBody(subNode, "Sub");
            return subNode;
        }

        private void ParseParameters(VB6SyntaxNode methodNode)
        {
            if (Match("("))
            {
                while (PeekToken() != null && PeekToken().Value != ")")
                {
                    var param = ParseVariableDeclaration(true);
                    if (param != null) methodNode.Children.Add(param);
                    if (!Match(",")) break;
                }
                Match(")");
            }
        }

        private void ParseMethodBody(VB6SyntaxNode methodNode, string endKeyword)
        {
            while (PeekToken() != null)
            {
                if (PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    var next = tokens.ElementAtOrDefault(currentTokenIndex + 1);
                    if (next != null && next.Value.Equals(endKeyword, StringComparison.OrdinalIgnoreCase))
                    {
                        SkipToken(); // Skip 'End'
                        SkipToken(); // Skip keyword
                        break;
                    }
                }

                var statement = ParseStatement();
                if (statement != null) methodNode.Children.Add(statement);
            }
        }

        private VB6SyntaxNode ParseStatement()
        {
            var token = PeekToken();
            if (token == null) return null;

            switch (token.Value.ToLowerInvariant())
            {
                case "if":
                    return ParseIfStatement();
                case "for":
                    return ParseForStatement();
                case "while":
                    return ParseWhileStatement();
                case "do":
                    return ParseDoLoopStatement();
                case "select":
                    return ParseSelectCaseStatement();
                case "set":
                case "let":
                    SkipToken(); // Skip Set/Let
                    return ParseAssignment();
                case "dim":
                case "static":
                    return ParseVariableDeclaration();
                case "redim":
                    return ParseReDimStatement();
                default:
                    // If it's an identifier followed by an equal sign, it's an assignment
                    if (token.Type == TokenType.Identifier)
                    {
                        var next = tokens.ElementAtOrDefault(currentTokenIndex + 1);
                        if (next != null && next.Value == "=")
                        {
                            return ParseAssignment();
                        }
                    }
                    // Otherwise, try to parse as a standalone expression (method call)
                    var expr = ParseExpression();
                    if (expr != null)
                    {
                        return new VB6SyntaxNode { Type = NodeType.Statement, Value = "Expression", Children = { expr } };
                    }
                    SkipToken();
                    return null;
            }
        }

        private VB6SyntaxNode ParseAssignment()
        {
            var target = GetToken()?.Value;
            if (Match("="))
            {
                var expr = ParseExpression();
                var assignNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "=" };
                assignNode.Children.Add(new VB6SyntaxNode { Type = NodeType.Expression, Value = target });
                if (expr != null) assignNode.Children.Add(expr);
                return assignNode;
            }
            return null;
        }

        private VB6SyntaxNode ParseIfStatement()
        {
            SkipToken(); // Skip 'If'
            var condition = ParseExpression();
            Match("Then");

            var ifNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "If" };
            if (condition != null) ifNode.Children.Add(condition);

            // True block
            var trueBlock = new VB6SyntaxNode { Type = NodeType.Statement, Value = "Then" };
            while (PeekToken() != null && !PeekToken().Value.Equals("Else", StringComparison.OrdinalIgnoreCase) && 
                   !PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                var stmt = ParseStatement();
                if (stmt != null) trueBlock.Children.Add(stmt);
            }
            ifNode.Children.Add(trueBlock);

            if (Match("Else"))
            {
                var elseBlock = new VB6SyntaxNode { Type = NodeType.Statement, Value = "Else" };
                while (PeekToken() != null && !PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    var stmt = ParseStatement();
                    if (stmt != null) elseBlock.Children.Add(stmt);
                }
                ifNode.Children.Add(elseBlock);
            }

            if (Match("End")) Match("If");
            return ifNode;
        }

        private VB6SyntaxNode ParseForStatement()
        {
            SkipToken(); // Skip 'For'
            var loopVar = GetToken()?.Value ?? "i";
            Match("=");
            var startExpr = ParseExpression();
            Match("To");
            var endExpr = ParseExpression();

            VB6SyntaxNode? stepExpr = null;
            if (Match("Step"))
            {
                stepExpr = ParseExpression();
            }

            var forNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "For" };
            forNode.Attributes["LoopVariable"] = loopVar;
            if (startExpr != null) forNode.Children.Add(startExpr);
            if (endExpr != null) forNode.Children.Add(endExpr);
            if (stepExpr != null) forNode.Children.Add(stepExpr);

            // Parse body
            var bodyNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "ForBody" };
            while (PeekToken() != null && !PeekToken().Value.Equals("Next", StringComparison.OrdinalIgnoreCase))
            {
                var stmt = ParseStatement();
                if (stmt != null) bodyNode.Children.Add(stmt);
            }
            forNode.Children.Add(bodyNode);

            Match("Next");
            // Optionally consume the loop variable after Next
            var nextToken = PeekToken();
            if (nextToken != null && nextToken.Type == TokenType.Identifier &&
                nextToken.Value.Equals(loopVar, StringComparison.OrdinalIgnoreCase))
            {
                SkipToken();
            }

            return forNode;
        }

        private VB6SyntaxNode ParseWhileStatement()
        {
            SkipToken(); // Skip 'While'
            var condition = ParseExpression();

            var whileNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "While" };
            if (condition != null) whileNode.Children.Add(condition);

            // Parse body
            var bodyNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "WhileBody" };
            while (PeekToken() != null && !PeekToken().Value.Equals("Wend", StringComparison.OrdinalIgnoreCase))
            {
                var stmt = ParseStatement();
                if (stmt != null) bodyNode.Children.Add(stmt);
            }
            whileNode.Children.Add(bodyNode);

            Match("Wend");
            return whileNode;
        }

        private VB6SyntaxNode ParseDoLoopStatement()
        {
            SkipToken(); // Skip 'Do'
            var doNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "Do" };

            // Check for Do While/Until at start
            bool isDoWhile = false;
            bool isUntil = false;
            if (Match("While"))
            {
                isDoWhile = true;
                var condition = ParseExpression();
                if (condition != null) doNode.Children.Add(condition);
            }
            else if (Match("Until"))
            {
                isDoWhile = true;
                isUntil = true;
                var condition = ParseExpression();
                if (condition != null) doNode.Children.Add(condition);
            }

            doNode.Attributes["IsDoWhile"] = isDoWhile.ToString();
            doNode.Attributes["IsUntil"] = isUntil.ToString();

            // Parse body
            var bodyNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "DoBody" };
            while (PeekToken() != null && !PeekToken().Value.Equals("Loop", StringComparison.OrdinalIgnoreCase))
            {
                var stmt = ParseStatement();
                if (stmt != null) bodyNode.Children.Add(stmt);
            }
            doNode.Children.Add(bodyNode);

            Match("Loop");

            // Check for Loop While/Until at end
            if (!isDoWhile)
            {
                if (Match("While"))
                {
                    var condition = ParseExpression();
                    if (condition != null) doNode.Children.Insert(0, condition);
                }
                else if (Match("Until"))
                {
                    doNode.Attributes["IsUntil"] = "True";
                    var condition = ParseExpression();
                    if (condition != null) doNode.Children.Insert(0, condition);
                }
            }

            return doNode;
        }

        private VB6SyntaxNode ParseSelectCaseStatement()
        {
            SkipToken(); // Skip 'Select'
            Match("Case"); // Skip 'Case'
            var testExpr = ParseExpression();

            var selectNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "Select" };
            if (testExpr != null) selectNode.Children.Add(testExpr);

            // Parse Case clauses
            while (PeekToken() != null)
            {
                var token = PeekToken();
                if (token.Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    var next = tokens.ElementAtOrDefault(currentTokenIndex + 1);
                    if (next != null && next.Value.Equals("Select", StringComparison.OrdinalIgnoreCase))
                    {
                        SkipToken(); // Skip 'End'
                        SkipToken(); // Skip 'Select'
                        break;
                    }
                }

                if (Match("Case"))
                {
                    // Check for Case Else
                    if (Match("Else"))
                    {
                        var caseElseNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "CaseElse" };
                        // Parse statements until End Select
                        while (PeekToken() != null &&
                               !PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                        {
                            var stmt = ParseStatement();
                            if (stmt != null) caseElseNode.Children.Add(stmt);
                        }
                        selectNode.Children.Add(caseElseNode);
                    }
                    else
                    {
                        var caseNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "Case" };

                        // Parse case values (can be multiple separated by commas)
                        // Also handles: Case 1 To 10, Case Is > 5
                        do
                        {
                            // Check for "Is" comparison
                            if (Match("Is"))
                            {
                                caseNode.Attributes["IsComparison"] = "True";
                                var opToken = GetToken();
                                if (opToken != null)
                                {
                                    caseNode.Attributes["ComparisonOperator"] = opToken.Value;
                                }
                                var valueExpr = ParseExpression();
                                if (valueExpr != null) caseNode.Children.Add(valueExpr);
                            }
                            else
                            {
                                var valueExpr = ParseExpression();
                                if (valueExpr != null)
                                {
                                    // Check for "To" range
                                    if (Match("To"))
                                    {
                                        caseNode.Attributes["IsRange"] = "True";
                                        var rangeEndExpr = ParseExpression();
                                        caseNode.Children.Add(valueExpr);
                                        if (rangeEndExpr != null) caseNode.Children.Add(rangeEndExpr);
                                    }
                                    else
                                    {
                                        caseNode.Children.Add(valueExpr);
                                    }
                                }
                            }
                        } while (Match(","));

                        // Parse body node
                        var bodyNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "CaseBody" };
                        while (PeekToken() != null &&
                               !PeekToken().Value.Equals("Case", StringComparison.OrdinalIgnoreCase) &&
                               !PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                        {
                            var stmt = ParseStatement();
                            if (stmt != null) bodyNode.Children.Add(stmt);
                        }
                        caseNode.Children.Add(bodyNode);
                        selectNode.Children.Add(caseNode);
                    }
                }
                else
                {
                    break;
                }
            }

            return selectNode;
        }

        private VB6SyntaxNode ParseExpression()
        {
            return ParseBinaryExpression(0);
        }

        private VB6SyntaxNode ParseBinaryExpression(int precedence)
        {
            var left = ParsePrimaryExpression();
            while (true)
            {
                var token = PeekToken();
                if (token == null || token.Type != TokenType.Operator) break;

                // Don't treat delimiters like ) , as binary operators - they terminate expressions
                if (token.Value == ")" || token.Value == ",") break;

                int opPrecedence = GetOperatorPrecedence(token.Value);
                if (opPrecedence < precedence) break;

                SkipToken();
                var right = ParseBinaryExpression(opPrecedence + 1);
                var binaryNode = new VB6SyntaxNode { Type = NodeType.Expression, Value = token.Value };
                if (left != null) binaryNode.Children.Add(left);
                if (right != null) binaryNode.Children.Add(right);
                left = binaryNode;
            }
            return left;
        }

        private VB6SyntaxNode ParsePrimaryExpression()
        {
            var token = GetToken();
            if (token == null) return null;

            if (token.Type == TokenType.NumberLiteral || token.Type == TokenType.StringLiteral)
            {
                return new VB6SyntaxNode { Type = NodeType.Expression, Value = token.Value };
            }

            if (token.Type == TokenType.Identifier)
            {
                var node = new VB6SyntaxNode { Type = NodeType.Expression, Value = token.Value };
                if (Match("("))
                {
                    while (PeekToken() != null && PeekToken().Value != ")")
                    {
                        var arg = ParseExpression();
                        if (arg != null) node.Children.Add(arg);
                        if (!Match(",")) break;
                    }
                    Match(")");
                }
                return node;
            }

            if (token.Value == "(")
            {
                var expr = ParseExpression();
                Match(")");
                return expr;
            }

            return null;
        }

        private int GetOperatorPrecedence(string op)
        {
            return op switch
            {
                "*" or "/" => 3,
                "+" or "-" or "&" => 2,
                "=" or "<" or ">" or "<=" or ">=" or "<>" => 1,
                _ => 0
            };
        }

        private VB6SyntaxNode ParseProperty()
        {
            SkipToken(); // Skip 'Property'
            var kind = GetToken()?.Value; // Get/Let/Set
            var name = GetToken()?.Value ?? "UnknownProperty";
            var propNode = new VB6SyntaxNode { Type = NodeType.Property, Value = name };
            propNode.Attributes["PropertyKind"] = kind ?? "Get";

            ParseParameters(propNode);
            if (Match("As"))
            {
                propNode.Attributes["Type"] = GetToken()?.Value ?? "Variant";
            }

            ParseMethodBody(propNode, "Property");
            return propNode;
        }

        private VB6SyntaxNode ParseReDimStatement()
        {
            SkipToken(); // Skip 'ReDim'

            var reDimNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "ReDim" };

            // Check for Preserve keyword
            if (Match("Preserve"))
            {
                reDimNode.Attributes["Preserve"] = "true";
            }

            // Get variable name
            var varName = GetToken()?.Value ?? "UnknownVariable";
            reDimNode.Attributes["VariableName"] = varName;

            // Parse new dimensions
            if (Match("("))
            {
                while (PeekToken() != null && PeekToken().Value != ")")
                {
                    var dimExpr = ParseExpression();
                    if (dimExpr != null) reDimNode.Children.Add(dimExpr);
                    if (!Match(",")) break;
                }
                Match(")");
            }

            return reDimNode;
        }

        private VB6SyntaxNode ParseVariableDeclaration(bool isParameter = false)
        {
            var startToken = PeekToken();
            if (!isParameter) SkipToken(); // Skip 'Dim', 'Public', etc.

            var name = GetToken()?.Value ?? "UnknownVariable";
            var varNode = new VB6SyntaxNode { Type = NodeType.Variable, Value = name };

            if (isParameter) varNode.Attributes["IsParameter"] = "true";
            if (startToken != null) varNode.Attributes["Accessibility"] = startToken.Value;

            // Check for array declaration: Dim arr(10) or Dim arr()
            if (Match("("))
            {
                varNode.Attributes["IsArray"] = "true";
                // Parse array dimensions
                while (PeekToken() != null && PeekToken().Value != ")")
                {
                    var dimExpr = ParseExpression();
                    if (dimExpr != null) varNode.Children.Add(dimExpr);
                    if (!Match(",")) break;
                }
                Match(")");
            }

            if (Match("As"))
            {
                varNode.Attributes["Type"] = GetToken()?.Value ?? "Variant";
            }
            else
            {
                varNode.Attributes["Type"] = "Variant";
            }

            return varNode;
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

=======

=======
>>>>>>> 2e0740d (The prototype files)
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
                var rawAst = ParseModule();

                // Build Higher-level Semantic AST
                var astBuilder = new AstBuilder();
                var semanticAst = astBuilder.BuildModule(rawAst);

                // Symbol table construction
                var symbolTableBuilder = new SymbolTableBuilder();
                var symbolTable = symbolTableBuilder.BuildSymbolTable(rawAst);

                // Type checking and semantic analysis
                var typeEngine = new TypeInferenceEngine(symbolTable);
                result.Errors.AddRange(typeEngine.PerformSemanticAnalysis(rawAst));

                // Code generation
                if (result.Errors.Count == 0)
                {
                    var codeGen = new VB6CodeGenerator();
                    result.CSharpCode = codeGen.GenerateCSharpCode(semanticAst);
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
            var funcNode = new VB6SyntaxNode { Type = NodeType.Function, Value = name };
            
            ParseParameters(funcNode);
            if (Match("As"))
            {
                funcNode.Attributes["ReturnType"] = GetToken()?.Value ?? "Variant";
            }

            ParseMethodBody(funcNode, "Function");
            return funcNode;
        }

        private VB6SyntaxNode ParseSub()
        {
            SkipToken(); // Skip 'Sub'
            var name = GetToken()?.Value ?? "UnknownSub";
            var subNode = new VB6SyntaxNode { Type = NodeType.Sub, Value = name };

            ParseParameters(subNode);
            ParseMethodBody(subNode, "Sub");
            return subNode;
        }

        private void ParseParameters(VB6SyntaxNode methodNode)
        {
            if (Match("("))
            {
                while (PeekToken() != null && PeekToken().Value != ")")
                {
                    var param = ParseVariableDeclaration(true);
                    if (param != null) methodNode.Children.Add(param);
                    if (!Match(",")) break;
                }
                Match(")");
            }
        }

        private void ParseMethodBody(VB6SyntaxNode methodNode, string endKeyword)
        {
            while (PeekToken() != null)
            {
                if (PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    var next = tokens.ElementAtOrDefault(currentTokenIndex + 1);
                    if (next != null && next.Value.Equals(endKeyword, StringComparison.OrdinalIgnoreCase))
                    {
                        SkipToken(); // Skip 'End'
                        SkipToken(); // Skip keyword
                        break;
                    }
                }

                var statement = ParseStatement();
                if (statement != null) methodNode.Children.Add(statement);
            }
        }

        private VB6SyntaxNode ParseStatement()
        {
            var token = PeekToken();
            if (token == null) return null;

            switch (token.Value.ToLowerInvariant())
            {
                case "if":
                    return ParseIfStatement();
                case "set":
                case "let":
                    SkipToken(); // Skip Set/Let
                    return ParseAssignment();
                case "dim":
                case "static":
                    return ParseVariableDeclaration();
                default:
                    // If it's an identifier followed by an equal sign, it's an assignment
                    if (token.Type == TokenType.Identifier)
                    {
                        var next = tokens.ElementAtOrDefault(currentTokenIndex + 1);
                        if (next != null && next.Value == "=")
                        {
                            return ParseAssignment();
                        }
                    }
                    // Otherwise, try to parse as a standalone expression (method call)
                    var expr = ParseExpression();
                    if (expr != null)
                    {
                        return new VB6SyntaxNode { Type = NodeType.Statement, Value = "Expression", Children = { expr } };
                    }
                    SkipToken();
                    return null;
            }
        }

        private VB6SyntaxNode ParseAssignment()
        {
            var target = GetToken()?.Value;
            if (Match("="))
            {
                var expr = ParseExpression();
                var assignNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "=" };
                assignNode.Children.Add(new VB6SyntaxNode { Type = NodeType.Expression, Value = target });
                if (expr != null) assignNode.Children.Add(expr);
                return assignNode;
            }
            return null;
        }

        private VB6SyntaxNode ParseIfStatement()
        {
            SkipToken(); // Skip 'If'
            var condition = ParseExpression();
            Match("Then");

            var ifNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "If" };
            if (condition != null) ifNode.Children.Add(condition);

            // True block
            var trueBlock = new VB6SyntaxNode { Type = NodeType.Statement, Value = "Then" };
            while (PeekToken() != null && !PeekToken().Value.Equals("Else", StringComparison.OrdinalIgnoreCase) && 
                   !PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                var stmt = ParseStatement();
                if (stmt != null) trueBlock.Children.Add(stmt);
            }
            ifNode.Children.Add(trueBlock);

            if (Match("Else"))
            {
                var elseBlock = new VB6SyntaxNode { Type = NodeType.Statement, Value = "Else" };
                while (PeekToken() != null && !PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    var stmt = ParseStatement();
                    if (stmt != null) elseBlock.Children.Add(stmt);
                }
                ifNode.Children.Add(elseBlock);
            }

            if (Match("End")) Match("If");
            return ifNode;
        }

        private VB6SyntaxNode ParseExpression()
        {
            return ParseBinaryExpression(0);
        }

        private VB6SyntaxNode ParseBinaryExpression(int precedence)
        {
            var left = ParsePrimaryExpression();
            while (true)
            {
                var token = PeekToken();
                if (token == null || token.Type != TokenType.Operator) break;

                int opPrecedence = GetOperatorPrecedence(token.Value);
                if (opPrecedence < precedence) break;

                SkipToken();
                var right = ParseBinaryExpression(opPrecedence + 1);
                var binaryNode = new VB6SyntaxNode { Type = NodeType.Expression, Value = token.Value };
                if (left != null) binaryNode.Children.Add(left);
                if (right != null) binaryNode.Children.Add(right);
                left = binaryNode;
            }
            return left;
        }

        private VB6SyntaxNode ParsePrimaryExpression()
        {
            var token = GetToken();
            if (token == null) return null;

            if (token.Type == TokenType.NumberLiteral || token.Type == TokenType.StringLiteral)
            {
                return new VB6SyntaxNode { Type = NodeType.Expression, Value = token.Value };
            }

            if (token.Type == TokenType.Identifier)
            {
                var node = new VB6SyntaxNode { Type = NodeType.Expression, Value = token.Value };
                if (Match("("))
                {
                    while (PeekToken() != null && PeekToken().Value != ")")
                    {
                        var arg = ParseExpression();
                        if (arg != null) node.Children.Add(arg);
                        if (!Match(",")) break;
                    }
                    Match(")");
                }
                return node;
            }

            if (token.Value == "(")
            {
                var expr = ParseExpression();
                Match(")");
                return expr;
            }

            return null;
        }

        private int GetOperatorPrecedence(string op)
        {
            return op switch
            {
                "*" or "/" => 3,
                "+" or "-" or "&" => 2,
                "=" or "<" or ">" or "<=" or ">=" or "<>" => 1,
                _ => 0
            };
        }

        private VB6SyntaxNode ParseProperty()
        {
            SkipToken(); // Skip 'Property'
            var kind = GetToken()?.Value; // Get/Let/Set
            var name = GetToken()?.Value ?? "UnknownProperty";
            var propNode = new VB6SyntaxNode { Type = NodeType.Property, Value = name };
            propNode.Attributes["PropertyKind"] = kind ?? "Get";

            ParseParameters(propNode);
            if (Match("As"))
            {
                propNode.Attributes["Type"] = GetToken()?.Value ?? "Variant";
            }

            ParseMethodBody(propNode, "Property");
            return propNode;
        }

        private VB6SyntaxNode ParseVariableDeclaration(bool isParameter = false)
        {
            var startToken = PeekToken();
            if (!isParameter) SkipToken(); // Skip 'Dim', 'Public', etc.
            
            var name = GetToken()?.Value ?? "UnknownVariable";
            var varNode = new VB6SyntaxNode { Type = NodeType.Variable, Value = name };
            
            if (isParameter) varNode.Attributes["IsParameter"] = "true";
            if (startToken != null) varNode.Attributes["Accessibility"] = startToken.Value;

            if (Match("As"))
            {
                varNode.Attributes["Type"] = GetToken()?.Value ?? "Variant";
            }
            else
            {
                varNode.Attributes["Type"] = "Variant";
            }
            
            return varNode;
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
<<<<<<< HEAD
//Made with Santa Claude


>>>>>>>
=======
>>>>>>> 2e0740d (The prototype files)

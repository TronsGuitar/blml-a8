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
        private readonly Stack<HashSet<string>> localIdentifierScopes = new Stack<HashSet<string>>();

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
                currentTokenIndex = 0;
                localIdentifierScopes.Clear();

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
                // Capture full exception details including stack trace to aid debugging
                result.Errors.Add($"Transpilation failed: {ex}");
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
                int prevIndex = currentTokenIndex;
                var declaration = ParseDeclaration();
                if (declaration != null)
                {
                    moduleNode.Children.Add(declaration);
                }

                if (currentTokenIndex == prevIndex)
                {
                    throw new Exception($"Infinite loop detected at token '{tokens[currentTokenIndex].Value}' (Line {tokens[currentTokenIndex].Line}, Index {currentTokenIndex})! ParseDeclaration did not consume any tokens.");
                }
            }

            return moduleNode;
        }

        private VB6SyntaxNode ParseDeclaration()
        {
            var token = PeekToken();
#pragma warning disable CS8603 // Possible null reference return.
            if (token == null) return null;
#pragma warning restore CS8603 // Possible null reference return.

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
                case "enum":
                    return ParseEnum();
                case "declare":
                    return ParseDeclare();
                case "version":
                    return ParseVersion();
                case "begin":
                    return ParseBeginBlock();
                case "attribute":
                    return ParseAttribute();
                case "dim":
                case "private":
                case "public":
                case "friend":
                case "static":
                    var accessibility = token.Value;
                    var nextToken = PeekToken(1);
                    if (nextToken != null)
                    {
                        switch (nextToken.Value.ToLowerInvariant())
                        {
                            case "function":
                                SkipToken();
                                return ParseFunction(accessibility);
                            case "sub":
                                SkipToken();
                                return ParseSub(accessibility);
                            case "property":
                                SkipToken();
                                return ParseProperty(accessibility);
                            case "enum":
                                SkipToken();
                                return ParseEnum(accessibility);
                            case "declare":
                                SkipToken();
                                return ParseDeclare(accessibility);
                        }
                    }
                    return ParseVariableDeclaration();
                default:
                    SkipToken();
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        private VB6SyntaxNode ParseProperty(string? accessibility = null)
        {
            SkipToken(); // Skip 'Property'

            var propertyKind = GetToken()?.Value ?? "Get";
            var name = GetToken()?.Value ?? "UnknownProperty";
            var propertyNode = new VB6SyntaxNode
            {
                Type = NodeType.Property,
                Value = name
            };

            propertyNode.Attributes["PropertyKind"] = propertyKind;

            if (!string.IsNullOrWhiteSpace(accessibility))
            {
                propertyNode.Attributes["Accessibility"] = accessibility;
            }

            EnterLocalScope();
            RegisterIdentifier(name);
            ParseParameters(propertyNode);
            if (Match("As"))
            {
                propertyNode.Attributes["ReturnType"] = GetToken()?.Value ?? "Variant";
            }

            ParseMethodBody(propertyNode, "Property");
            ExitLocalScope();
            return propertyNode;
        }

        private VB6SyntaxNode ParseEnum(string? accessibility = null)
        {
            SkipToken(); // Skip 'Enum'
            var name = GetToken()?.Value ?? "UnknownEnum";
            var enumNode = new VB6SyntaxNode { Type = NodeType.Enum, Value = name };
            if (!string.IsNullOrWhiteSpace(accessibility))
            {
                enumNode.Attributes["Accessibility"] = accessibility;
            }

            while (PeekToken() != null)
            {
                if (PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    var next = tokens.ElementAtOrDefault(currentTokenIndex + 1);
                    if (next != null && next.Value.Equals("Enum", StringComparison.OrdinalIgnoreCase))
                    {
                        SkipToken(); // Skip 'End'
                        SkipToken(); // Skip 'Enum'
                        break;
                    }
                }

                var memberToken = PeekToken();
                if (memberToken == null) break;
                if (memberToken.Type != BLML.Phase1Foundation.Lexer.TokenType.Identifier &&
                    memberToken.Type != BLML.Phase1Foundation.Lexer.TokenType.Keyword)
                {
                    SkipToken(); // Defensive: skip a stray token rather than looping forever
                    continue;
                }

                var memberName = GetToken()!.Value;
                var memberNode = new VB6SyntaxNode { Type = NodeType.EnumMember, Value = memberName };
                if (Match("="))
                {
                    var valueExpr = ParseExpression();
                    if (valueExpr != null) memberNode.Children.Add(valueExpr);
                }
                enumNode.Children.Add(memberNode);
            }

            return enumNode;
        }

        private VB6SyntaxNode ParseDeclare(string? accessibility = null)
        {
            SkipToken(); // Skip 'Declare'
            var isFunction = PeekToken()?.Value.Equals("Function", StringComparison.OrdinalIgnoreCase) == true;
            SkipToken(); // Skip 'Function' or 'Sub'

            var name = GetToken()?.Value ?? "UnknownDeclare";
            var declareNode = new VB6SyntaxNode { Type = NodeType.Declare, Value = name };
            declareNode.Attributes["IsFunction"] = isFunction.ToString();
            if (!string.IsNullOrWhiteSpace(accessibility))
            {
                declareNode.Attributes["Accessibility"] = accessibility;
            }

            if (Match("Lib"))
            {
                declareNode.Attributes["Lib"] = GetToken()?.Value ?? string.Empty;
            }
            if (Match("Alias"))
            {
                declareNode.Attributes["Alias"] = GetToken()?.Value ?? string.Empty;
            }

            ParseParameters(declareNode);

            if (Match("As"))
            {
                declareNode.Attributes["ReturnType"] = GetToken()?.Value ?? "Variant";
            }

            return declareNode;
        }

        private VB6SyntaxNode ParseClass()
        {
            SkipToken(); // Skip 'Class'
            var name = GetToken()?.Value ?? "UnknownClass";
            return new VB6SyntaxNode { Type = NodeType.Class, Value = name };
        }

        private VB6SyntaxNode ParseVersion()
        {
            var versionToken = PeekToken();
            var line = versionToken?.Line ?? 0;
            SkipToken(); // Skip 'VERSION'

            var versionNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "VERSION" };

            var text = "";
            while (PeekToken() != null && PeekToken().Line == line)
            {
                text += GetToken()?.Value;
            }
            versionNode.Attributes["Content"] = text;

            return versionNode;
        }

        private VB6SyntaxNode ParseBeginBlock()
        {
            var beginToken = PeekToken();
            var line = beginToken?.Line ?? 0;
            SkipToken(); // Skip 'Begin'

            var beginNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "BeginBlock" };

            var controlInfo = "";
            while (PeekToken() != null && PeekToken().Line == line)
            {
                var t = GetToken();
                if (t != null) controlInfo += t.Value;
            }
            beginNode.Attributes["Control"] = controlInfo;

            while (PeekToken() != null)
            {
                if (PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if this End belongs to a begin block. In forms, End indicates close of block.
                    // Also form property assignments don't use 'End'.
                    var endLine = PeekToken().Line;
                    SkipToken(); // Consume End
                    
                    // Consume any extra tokens on End line
                    while (PeekToken() != null && PeekToken().Line == endLine)
                    {
                        SkipToken();
                    }
                    break;
                }
                else if (PeekToken().Value.Equals("Begin", StringComparison.OrdinalIgnoreCase))
                {
                    beginNode.Children.Add(ParseBeginBlock());
                }
                else
                {
                    var propAssign = ParseFormProperty();
                    if (propAssign != null) beginNode.Children.Add(propAssign);
                }
            }

            return beginNode;
        }

        private VB6SyntaxNode ParseFormProperty()
        {
            var token = PeekToken();
            if (token == null) return null;

            var propNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "FormProperty" };
            var line = token.Line;

            var leftSide = "";
            var rightSide = "";
            bool isRight = false;

            while (PeekToken() != null && PeekToken().Line == line)
            {
                var t = GetToken();
                if (t == null) break;

                if (!isRight && t.Value == "=")
                {
                    isRight = true;
                    continue;
                }

                if (isRight) rightSide += t.Type == BLML.Phase1Foundation.Lexer.TokenType.StringLiteral ? $"\"{t.Value}\"" : t.Value;
                else leftSide += t.Value;
            }

            propNode.Attributes["Property"] = leftSide.Trim();
            propNode.Attributes["Value"] = rightSide.Trim();
            return propNode;
        }

        private VB6SyntaxNode ParseAttribute()
        {
            var attrToken = PeekToken();
            var line = attrToken?.Line ?? 0;
            SkipToken(); // Skip 'Attribute'

            var attrNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "Attribute" };

            var leftSide = "";
            var rightSide = "";
            bool isRight = false;

            while (PeekToken() != null && PeekToken().Line == line)
            {
                var t = GetToken();
                if (t == null) break;

                if (!isRight && t.Value == "=")
                {
                    isRight = true;
                    continue;
                }

                if (isRight) rightSide += t.Type == BLML.Phase1Foundation.Lexer.TokenType.StringLiteral ? $"\"{t.Value}\"" : t.Value;
                else leftSide += t.Value;
            }

            attrNode.Attributes["Property"] = leftSide.Trim();
            attrNode.Attributes["Value"] = rightSide.Trim();

            return attrNode;
        }

        private VB6SyntaxNode ParseFunction(string? accessibility = null)
        {
            SkipToken(); // Skip 'Function'
            var name = GetToken()?.Value ?? "UnknownFunction";
            var funcNode = new VB6SyntaxNode { Type = NodeType.Function, Value = name };

            if (!string.IsNullOrWhiteSpace(accessibility))
            {
                funcNode.Attributes["Accessibility"] = accessibility;
            }

            EnterLocalScope();
            RegisterIdentifier(name);
            ParseParameters(funcNode);
            if (Match("As"))
            {
                funcNode.Attributes["ReturnType"] = GetToken()?.Value ?? "Variant";
            }

            ParseMethodBody(funcNode, "Function");
            ExitLocalScope();
            return funcNode;
        }

        private VB6SyntaxNode ParseSub(string? accessibility = null)
        {
            SkipToken(); // Skip 'Sub'
            var name = GetToken()?.Value ?? "UnknownSub";
            var subNode = new VB6SyntaxNode { Type = NodeType.Sub, Value = name };

            if (!string.IsNullOrWhiteSpace(accessibility))
            {
                subNode.Attributes["Accessibility"] = accessibility;
            }

            EnterLocalScope();
            RegisterIdentifier(name);
            ParseParameters(subNode);
            ParseMethodBody(subNode, "Sub");
            ExitLocalScope();
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

        private VB6SyntaxNode ParseVariableDeclaration(bool v)
        {
            var variableNode = new VB6SyntaxNode
            {
                Type = NodeType.Variable
            };

            if (v)
            {
                if (Match("Optional"))
                {
                    variableNode.Attributes["Optional"] = "true";
                }

                if (Match("ParamArray"))
                {
                    variableNode.Attributes["ParamArray"] = "true";
                }

                if (Match("ByVal"))
                {
                    variableNode.Attributes["ByVal"] = "true";
                }
                else if (Match("ByRef"))
                {
                    variableNode.Attributes["ByVal"] = "false";
                }

                variableNode.Attributes["IsParameter"] = "true";
            }
            else
            {
                var declarationKeyword = PeekToken()?.Value;
                if (declarationKeyword != null)
                {
                    switch (declarationKeyword.ToLowerInvariant())
                    {
                        case "public":
                        case "private":
                        case "friend":
                        case "static":
                            variableNode.Attributes["Accessibility"] = declarationKeyword;
                            SkipToken();
                            if (PeekToken()?.Value.Equals("Dim", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                SkipToken();
                            }
                            break;
                        case "dim":
                            SkipToken();
                            break;
                    }
                }
            }

            variableNode.Value = GetToken()?.Value ?? "UnnamedVariable";
            RegisterIdentifier(variableNode.Value);
            variableNode.Attributes.TryAdd("Type", "Variant");

            if (Match("("))
            {
                variableNode.Attributes["IsArray"] = "true";
                while (PeekToken() != null && !Match(")"))
                {
                    int prevIdx = currentTokenIndex;
                    var dimension = ParseExpression();
                    if (dimension != null)
                    {
                        variableNode.Children.Add(dimension);
                    }

                    if (!Match(",") && currentTokenIndex == prevIdx)
                    {
                        SkipToken(); // Safety: skip unrecognized token to prevent infinite loop
                    }
                }
            }

            if (Match("As"))
            {
                variableNode.Attributes["Type"] = GetToken()?.Value ?? "Variant";
            }

            if (Match("="))
            {
                var initialValue = ParseExpression();
                if (initialValue != null)
                {
                    if (v)
                    {
                        variableNode.Attributes["DefaultValue"] = initialValue.Value;
                    }
                    else
                    {
                        variableNode.Attributes["InitialValue"] = initialValue.Value;
                    }

                    variableNode.Children.Add(initialValue);
                }
            }

            return variableNode;
        }

        private void ParseMethodBody(VB6SyntaxNode methodNode, string endKeyword)
        {
            while (PeekToken() != null)
            {
                int prevIndex = currentTokenIndex;
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

                if (currentTokenIndex == prevIndex)
                {
                    throw new Exception($"Infinite loop detected in ParseMethodBody ({endKeyword}) at token '{tokens[currentTokenIndex].Value}' (Line {tokens[currentTokenIndex].Line}, Index {currentTokenIndex})! ParseStatement did not consume any tokens.");
                }
            }
        }

        private VB6SyntaxNode ParseStatement()
        {
            var token = PeekToken();
#pragma warning disable CS8603 // Possible null reference return.
            if (token == null) return null;
#pragma warning restore CS8603 // Possible null reference return.

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
                case "with":
                    return ParseWithStatement();
                case "set":
                case "let":
                    SkipToken(); // Skip Set/Let
                    return ParseAssignment();
                case "dim":
                case "static":
                    return ParseVariableDeclaration();
                case "redim":
                    return ParseReDimStatement();
                case "exit":
                    return ParseExitStatement();
                default:
                    // A leading '.' inside a With block refers to the With target implicitly
                    // (e.g. `.Name = "x"` means `withTarget.Name = "x"`); the target identifier
                    // and the '=' are two tokens apart here instead of one, so it needs its own
                    // lookahead rather than falling through to the generic expression parse below
                    // (which would otherwise consume the '=' as a comparison operator, not an assignment).
                    if (token.Value == ".")
                    {
                        var afterMember = tokens.ElementAtOrDefault(currentTokenIndex + 2);
                        if (afterMember != null && afterMember.Value == "=")
                        {
                            return ParseWithMemberAssignment();
                        }

                        var withExpr = ParseExpression();
                        if (withExpr != null)
                        {
                            return new VB6SyntaxNode { Type = NodeType.Statement, Value = "Expression", Children = { withExpr } };
                        }
                        SkipToken();
#pragma warning disable CS8603 // Possible null reference return.
                        return null;
#pragma warning restore CS8603 // Possible null reference return.
                    }

                    // If it's an identifier followed by an equal sign, it's an assignment
                    if (IsIdentifierLike(token))
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
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        private VB6SyntaxNode ParseWithMemberAssignment()
        {
            var target = ParsePrimaryExpression(); // consumes '.Member'
            if (Match("="))
            {
                var expr = ParseExpression();
                var assignNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "=" };
                if (target != null) assignNode.Children.Add(target);
                if (expr != null) assignNode.Children.Add(expr);
                return assignNode;
            }
#pragma warning disable CS8603 // Possible null reference return.
            return target;
#pragma warning restore CS8603 // Possible null reference return.
        }

        private VB6SyntaxNode ParseWithStatement()
        {
            SkipToken(); // Skip 'With'
            var target = ParseExpression();

            var withNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "With" };
            if (target != null) withNode.Children.Add(target);

            var bodyNode = new VB6SyntaxNode { Type = NodeType.Statement, Value = "WithBody" };
            while (PeekToken() != null)
            {
                if (PeekToken().Value.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    var next = tokens.ElementAtOrDefault(currentTokenIndex + 1);
                    if (next != null && next.Value.Equals("With", StringComparison.OrdinalIgnoreCase))
                    {
                        SkipToken(); // Skip 'End'
                        SkipToken(); // Skip 'With'
                        break;
                    }
                }

                var stmt = ParseStatement();
                if (stmt != null) bodyNode.Children.Add(stmt);
            }
            withNode.Children.Add(bodyNode);

            return withNode;
        }

        private VB6SyntaxNode ParseReDimStatement()
        {
            SkipToken(); // Skip 'ReDim'

            var redimNode = new VB6SyntaxNode
            {
                Type = NodeType.Statement,
                Value = "ReDim"
            };

            if (Match("Preserve"))
            {
                redimNode.Attributes["Preserve"] = "True";
            }

            redimNode.Attributes["VariableName"] = GetToken()?.Value ?? string.Empty;

            if (Match("("))
            {
                while (PeekToken() != null && !Match(")"))
                {
                    var dimension = ParseExpression();
                    if (dimension != null)
                    {
                        redimNode.Children.Add(dimension);
                    }

                    Match(",");
                }
            }

            return redimNode;
        }

        private VB6SyntaxNode ParseVariableDeclaration()
        {
            return ParseVariableDeclaration(false);
        }

        private VB6SyntaxNode ParseExitStatement()
        {
            SkipToken(); // Skip 'Exit'
            var exitKind = GetToken()?.Value ?? string.Empty; // 'For', 'Do', 'Sub', 'Function', etc.
            var exitNode = new VB6SyntaxNode
            {
                Type = NodeType.Statement,
                Value = "Exit"
            };
            exitNode.Attributes["ExitKind"] = exitKind;
            return exitNode;
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
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
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
            if (nextToken != null && nextToken.Type == BLML.Phase1Foundation.Lexer.TokenType.Identifier &&
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

        private VB6SyntaxNode ParseBinaryExpression(int parentPrecedence)
        {
            var left = ParsePrimaryExpression();
            if (left == null)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }

            while (true)
            {
                var operatorToken = PeekToken();
                var precedence = GetBinaryOperatorPrecedence(operatorToken);
                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }

                SkipToken();
                var right = ParseBinaryExpression(precedence);
                if (right == null)
                {
                    break;
                }

                left = new VB6SyntaxNode
                {
                    Type = NodeType.Expression,
                    Value = operatorToken!.Value,
                    Children = { left, right }
                };
            }

            return left;
        }

        /// <summary>
        /// Parses one call argument, recognizing VB6 named-argument syntax
        /// (`name:=value`, e.g. `MsgBox Prompt:="Hi"`) ahead of a plain positional
        /// expression. The lexer tokenizes `:=` as a single operator, so a match there
        /// unambiguously identifies this as a named argument rather than, say, a
        /// boolean expression named `name` followed by an unrelated `:` statement
        /// separator.
        /// </summary>
        private VB6SyntaxNode ParseCallArgument()
        {
            var token = PeekToken();
            var next = PeekToken(1);
            if (token != null && next != null && next.Value == ":=" &&
                (token.Type == BLML.Phase1Foundation.Lexer.TokenType.Identifier || token.Type == BLML.Phase1Foundation.Lexer.TokenType.Keyword))
            {
                var argName = GetToken()!.Value;
                SkipToken(); // Skip ':='
                var valueExpr = ParseExpression();

                var namedArgNode = new VB6SyntaxNode
                {
                    Type = NodeType.Expression,
                    Value = argName,
                    Attributes = new Dictionary<string, string> { ["ExpressionKind"] = "NamedArgument" }
                };
                if (valueExpr != null) namedArgNode.Children.Add(valueExpr);
                return namedArgNode;
            }

            return ParseExpression();
        }

        private VB6SyntaxNode ParsePrimaryExpression()
        {
            var token = PeekToken();
            if (token == null)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }

            // A leading '.' is an implicit member reference to the enclosing With block's
            // target (e.g. `.Name` inside `With obj ... End With`) - resolved to
            // `<withTarget>.Name` at code-generation time, since the AST alone doesn't
            // carry which With block a given expression is nested in.
            if (token.Value == ".")
            {
                SkipToken(); // Consume '.'
                var memberName = GetToken()?.Value ?? "UnknownMember";
                var memberNode = new VB6SyntaxNode
                {
                    Type = NodeType.Expression,
                    Value = memberName,
                    Attributes = new Dictionary<string, string> { ["ExpressionKind"] = "WithMemberAccess" }
                };

                if (Match("("))
                {
                    memberNode.Attributes["ExpressionKind"] = "WithMemberInvocation";
                    while (PeekToken() != null && !Match(")"))
                    {
                        int prevIdx = currentTokenIndex;
                        var argument = ParseCallArgument();
                        if (argument != null) memberNode.Children.Add(argument);

                        if (!Match(",") && currentTokenIndex == prevIdx)
                        {
                            SkipToken(); // Safety: skip unrecognized token to prevent infinite loop
                        }
                    }
                }

                return memberNode;
            }

            // Handle unary operators: -, Not
            if (token.Value == "-" || token.Value.Equals("Not", StringComparison.OrdinalIgnoreCase))
            {
                SkipToken(); // Consume the unary operator
                var operand = ParsePrimaryExpression();
                if (operand == null)
                {
                    // Unary operator with no operand — return as standalone expression
                    return new VB6SyntaxNode
                    {
                        Type = NodeType.Expression,
                        Value = token.Value,
                        Attributes = new Dictionary<string, string> { ["ExpressionKind"] = "UnaryOperator" }
                    };
                }
                return new VB6SyntaxNode
                {
                    Type = NodeType.Expression,
                    Value = token.Value,
                    Attributes = new Dictionary<string, string> { ["ExpressionKind"] = "UnaryOperator" },
                    Children = { operand }
                };
            }

            if (Match("("))
            {
                var expression = ParseExpression();
                Match(")");
                return expression;
            }

            if (token.Type == BLML.Phase1Foundation.Lexer.TokenType.NumberLiteral)
            {
                SkipToken();
                return new VB6SyntaxNode
                {
                    Type = NodeType.Expression,
                    Value = token.Value,
                    Attributes = new Dictionary<string, string>
                    {
                        ["LiteralKind"] = "Number"
                    }
                };
            }

            if (token.Type == BLML.Phase1Foundation.Lexer.TokenType.StringLiteral)
            {
                SkipToken();
                return new VB6SyntaxNode
                {
                    Type = NodeType.Expression,
                    Value = token.Value,
                    Attributes = new Dictionary<string, string>
                    {
                        ["LiteralKind"] = "String"
                    }
                };
            }

            if (token.Type == BLML.Phase1Foundation.Lexer.TokenType.Identifier || token.Type == BLML.Phase1Foundation.Lexer.TokenType.Keyword)
            {
                SkipToken();

                var expressionNode = new VB6SyntaxNode
                {
                    Type = NodeType.Expression,
                    Value = token.Value
                };

                if (token.Value.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                    token.Value.Equals("False", StringComparison.OrdinalIgnoreCase))
                {
                    expressionNode.Attributes["LiteralKind"] = "Boolean";
                }

                if (Match("("))
                {
                    expressionNode.Attributes["ExpressionKind"] = "Invocation";
                    while (PeekToken() != null && !Match(")"))
                    {
                        int prevIdx = currentTokenIndex;
                        var argument = ParseCallArgument();
                        if (argument != null)
                        {
                            expressionNode.Children.Add(argument);
                        }

                        if (!Match(",") && currentTokenIndex == prevIdx)
                        {
                            SkipToken(); // Safety: skip unrecognized token to prevent infinite loop
                        }
                    }
                }

                return expressionNode;
            }

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        private static int GetBinaryOperatorPrecedence(VB6Token? token)
        {
            if (token == null)
            {
                return 0;
            }

            return token.Value switch
            {
                "*" or "/" or "Mod" => 5,
                "+" or "-" or "&" => 4,
                "=" or "<>" or "<" or ">" or "<=" or ">=" => 3,
                "And" => 2,
                "Or" => 1,
                _ => 0
            };
        }

        private VB6Token PeekToken()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return currentTokenIndex < tokens.Count ? tokens[currentTokenIndex] : null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        private VB6Token PeekToken(int offset)
        {
#pragma warning disable CS8603 // Possible null reference return.
            var index = currentTokenIndex + offset;
            return index < tokens.Count ? tokens[index] : null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        private void SkipToken()
        {
            currentTokenIndex++;
        }

        private VB6Token GetToken()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return currentTokenIndex < tokens.Count ? tokens[currentTokenIndex++] : null;
#pragma warning restore CS8603 // Possible null reference return.
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

        private void EnterLocalScope()
        {
            localIdentifierScopes.Push(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        private void ExitLocalScope()
        {
            if (localIdentifierScopes.Count > 0)
            {
                localIdentifierScopes.Pop();
            }
        }

        private void RegisterIdentifier(string? identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier) || localIdentifierScopes.Count == 0)
            {
                return;
            }

            localIdentifierScopes.Peek().Add(identifier);
        }

        private bool IsIdentifierLike(VB6Token token)
        {
            return token.Type == BLML.Phase1Foundation.Lexer.TokenType.Identifier ||
                   (token.Type == BLML.Phase1Foundation.Lexer.TokenType.Keyword && IsLocallyDeclaredIdentifier(token.Value));
        }

        private bool IsLocallyDeclaredIdentifier(string identifier)
        {
            foreach (var scope in localIdentifierScopes)
            {
                if (scope.Contains(identifier))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
//Made with Santa Claude

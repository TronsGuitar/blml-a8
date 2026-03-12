using BLML.Phase1Foundation.SymbolTable;

namespace BLML.Phase1Foundation.AST
{
    public class AstBuilder
    {
        public ModuleNode BuildModule(VB6SyntaxNode syntaxNode)
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (syntaxNode == null) return null;
#pragma warning restore CS8603 // Possible null reference return.

            if (syntaxNode.Type != NodeType.Module)
                throw new ArgumentException("Root node must be a Module", nameof(syntaxNode));

            var module = new ModuleNode
            {
                Name = syntaxNode.Value ?? "UnknownModule"
            };

            foreach (var child in syntaxNode.Children)
            {
                var declaration = BuildDeclaration(child);
                if (declaration != null)
                {
                    module.Declarations.Add(declaration);
                }
            }

            return module;
        }

        private DeclarationNode BuildDeclaration(VB6SyntaxNode node)
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (node == null) return null;
#pragma warning restore CS8603 // Possible null reference return.

            switch (node.Type)
            {
                case NodeType.Function:
                case NodeType.Sub:
                    return BuildMethod(node);
                case NodeType.Variable:
                    return BuildVariable(node);
                case NodeType.Property:
                    return BuildProperty(node);
                default:
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        private MethodDeclarationNode BuildMethod(VB6SyntaxNode node)
        {
            var method = new MethodDeclarationNode
            {
                Name = node.Value,
                IsFunction = node.Type == NodeType.Function,
                Accessibility = DetermineAccessibility(node)
            };

            if (node.Attributes.TryGetValue("ReturnType", out string retType))
                method.ReturnType = retType;
            else
                method.ReturnType = method.IsFunction ? "Variant" : "void";

            foreach (var child in node.Children)
            {
                if (child.Type == NodeType.Variable && child.Attributes.ContainsKey("IsParameter"))
                {
                    method.Parameters.Add(BuildParameter(child));
                }
                else if (child.Type == NodeType.Statement || child.Type == NodeType.Variable)
                {
                    method.Body.Add(BuildStatement(child));
                }
            }

            return method;
        }

        private ParameterNode BuildParameter(VB6SyntaxNode node)
        {
            ExpressionNode? defaultValueExpression = null;

            if (node.Attributes.ContainsKey("DefaultValue") && node.Children.Count > 0)
            {
                defaultValueExpression = BuildExpression(node.Children[0]);
            }

            return new ParameterNode
            {
                Name = node.Value,
                Type = node.Attributes.GetValueOrDefault("Type", "Variant"),
                IsByRef = !node.Attributes.GetValueOrDefault("ByVal", "false").Equals("true", StringComparison.OrdinalIgnoreCase),
                IsOptional = node.Attributes.GetValueOrDefault("Optional", "false").Equals("true", StringComparison.OrdinalIgnoreCase),
                DefaultValue = node.Attributes.GetValueOrDefault("DefaultValue", ""),
                DefaultValueExpression = defaultValueExpression
            };
        }

        private PropertyDeclarationNode BuildProperty(VB6SyntaxNode node)
        {
            var property = new PropertyDeclarationNode
            {
                Name = node.Value,
                Accessibility = DetermineAccessibility(node),
                Type = node.Attributes.GetValueOrDefault("ReturnType", "Variant"),
                PropertyKind = ParsePropertyKind(node.Attributes.GetValueOrDefault("PropertyKind", "Get"))
            };

            foreach (var child in node.Children)
            {
                if (child.Type == NodeType.Variable && child.Attributes.ContainsKey("IsParameter"))
                {
                    property.Parameters.Add(BuildParameter(child));
                }
                else if (child.Type == NodeType.Statement || child.Type == NodeType.Variable)
                {
                    property.Body.Add(BuildStatement(child));
                }
            }

            if ((property.PropertyKind == PropertyProcedureKind.Let || property.PropertyKind == PropertyProcedureKind.Set) &&
                string.Equals(property.Type, "Variant", StringComparison.OrdinalIgnoreCase) &&
                property.Parameters.Count > 0)
            {
                property.Type = property.Parameters[^1].Type;
            }

            return property;
        }

        private VariableDeclarationNode BuildVariable(VB6SyntaxNode node)
        {
            var varDecl = new VariableDeclarationNode
            {
                Name = node.Value,
                Type = node.Attributes.GetValueOrDefault("Type", "Variant"),
                Accessibility = DetermineAccessibility(node),
                InitialValue = node.Attributes.GetValueOrDefault("InitialValue", ""),
                IsArray = node.Attributes.GetValueOrDefault("IsArray", "false").Equals("true", StringComparison.OrdinalIgnoreCase)
            };

            // Add array dimensions
            if (varDecl.IsArray)
            {
                foreach (var child in node.Children)
                {
                    var dimExpr = BuildExpression(child);
                    if (dimExpr != null) varDecl.ArrayDimensions.Add(dimExpr);
                }
            }

            return varDecl;
        }

        private StatementNode BuildStatement(VB6SyntaxNode node)
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (node == null) return null;
#pragma warning restore CS8603 // Possible null reference return.

            switch (node.Type)
            {
                case NodeType.Variable:
                    return new VariableDeclarationNode
                    {
                        Name = node.Value,
                        Type = node.Attributes.GetValueOrDefault("Type", "Variant"),
                        Accessibility = VB6Accessibility.Private
                    };
                case NodeType.Statement:
                    if (node.Value == "=")
                    {
                        return new AssignmentNode
                        {
                            Target = BuildExpression(node.Children[0]),
                            Value = BuildExpression(node.Children[1])
                        };
                    }
                    if (node.Value == "If")
                    {
                        var ifStmt = new IfStatementNode
                        {
                            Condition = BuildExpression(node.Children[0])
                        };

                        var thenBlock = node.Children.FirstOrDefault(c => c.Value == "Then");
                        if (thenBlock != null)
                        {
                            foreach (var stmt in thenBlock.Children)
                            {
                                var buildStmt = BuildStatement(stmt);
                                if (buildStmt != null) ifStmt.TrueBlock.Statements.Add(buildStmt);
                            }
                        }

                        var elseBlock = node.Children.FirstOrDefault(c => c.Value == "Else");
                        if (elseBlock != null)
                        {
                            ifStmt.ElseBlock = new BlockNode();
                            foreach (var stmt in elseBlock.Children)
                            {
                                var buildStmt = BuildStatement(stmt);
                                if (buildStmt != null) ifStmt.ElseBlock.Statements.Add(buildStmt);
                            }
                        }
                        return ifStmt;
                    }
                    if (node.Value == "Expression")
                    {
                        return new ExpressionStatementNode
                        {
                            Expression = BuildExpression(node.Children[0])
                        };
                    }
                    if (node.Value == "For")
                    {
                        return BuildForStatement(node);
                    }
                    if (node.Value == "While")
                    {
                        return BuildWhileStatement(node);
                    }
                    if (node.Value == "Do")
                    {
                        return BuildDoLoopStatement(node);
                    }
                    if (node.Value == "Select")
                    {
                        return BuildSelectCaseStatement(node);
                    }
                    if (node.Value == "Exit")
                    {
                        return new ExitStatementNode
                        {
                            ExitKind = node.Attributes.GetValueOrDefault("ExitKind", string.Empty)
                        };
                    }
                    if (node.Value == "OnError")
                    {
                        var kind = node.Attributes.GetValueOrDefault("OnErrorKind", "ResumeNext");
                        if (kind == "GoTo")
                        {
                            return new OnErrorGoToStatementNode
                            {
                                Label = node.Attributes.GetValueOrDefault("Label", "0")
                            };
                        }
                        return new OnErrorResumeNextStatementNode();
                    }
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
                default:
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        private ExpressionNode BuildExpression(VB6SyntaxNode node)
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (node == null) return null;
#pragma warning restore CS8603 // Possible null reference return.

            if (node.Type == NodeType.Expression)
            {
                if (node.Attributes.TryGetValue("LiteralKind", out var literalKind))
                {
                    return literalKind switch
                    {
                        "Number" when int.TryParse(node.Value, out var i) => new LiteralExpressionNode { Value = i },
                        "Number" when double.TryParse(node.Value, out var d) => new LiteralExpressionNode { Value = d },
                        "String" => new LiteralExpressionNode { Value = node.Value },
                        "Boolean" when bool.TryParse(node.Value, out var b) => new LiteralExpressionNode { Value = b },
                        _ => new IdentifierExpressionNode { Name = node.Value }
                    };
                }

                if (SymbolTableBuilder.PredefinedConstants.TryGetValue(node.Value, out var constantValue))
                {
                    return new LiteralExpressionNode { Value = constantValue! };
                }

                if (node.Attributes.GetValueOrDefault("ExpressionKind") == "Invocation")
                {
                    var invoke = new InvocationExpressionNode
                    {
                        Target = new IdentifierExpressionNode { Name = node.Value }
                    };
                    foreach (var arg in node.Children)
                    {
                        var buildExpr = BuildExpression(arg);
                        if (buildExpr != null) invoke.Arguments.Add(buildExpr);
                    }
                    return invoke;
                }

                if (node.Children.Count == 2)
                {
                    return new BinaryExpressionNode
                    {
                        Left = BuildExpression(node.Children[0]),
                        Operator = node.Value,
                        Right = BuildExpression(node.Children[1])
                    };
                }

                // If it looks like a function call or identifier
                if (node.Children.Count > 0)
                {
                    var invoke = new InvocationExpressionNode
                    {
                        Target = new IdentifierExpressionNode { Name = node.Value }
                    };
                    foreach (var arg in node.Children)
                    {
                        var buildExpr = BuildExpression(arg);
                        if (buildExpr != null) invoke.Arguments.Add(buildExpr);
                    }
                    return invoke;
                }

                return new IdentifierExpressionNode { Name = node.Value };
            }

            return new IdentifierExpressionNode { Name = node.Value };
        }

        private ForStatementNode BuildForStatement(VB6SyntaxNode node)
        {
            var forStmt = new ForStatementNode
            {
                LoopVariable = node.Attributes.GetValueOrDefault("LoopVariable", "i")
            };

            // First two children are start and end expressions
            if (node.Children.Count >= 2)
            {
                forStmt.StartValue = BuildExpression(node.Children[0])!;
                forStmt.EndValue = BuildExpression(node.Children[1])!;
            }

            // Check for step value (third child if not ForBody)
            int bodyIndex = 2;
            if (node.Children.Count > 2 && node.Children[2].Value != "ForBody")
            {
                forStmt.StepValue = BuildExpression(node.Children[2]);
                bodyIndex = 3;
            }

            // Build body
            var bodyNode = node.Children.FirstOrDefault(c => c.Value == "ForBody");
            if (bodyNode != null)
            {
                foreach (var stmt in bodyNode.Children)
                {
                    var buildStmt = BuildStatement(stmt);
                    if (buildStmt != null) forStmt.Body.Statements.Add(buildStmt);
                }
            }

            return forStmt;
        }

        private WhileStatementNode BuildWhileStatement(VB6SyntaxNode node)
        {
            var whileStmt = new WhileStatementNode();

            // First child is the condition
            if (node.Children.Count > 0 && node.Children[0].Value != "WhileBody")
            {
                whileStmt.Condition = BuildExpression(node.Children[0])!;
            }

            // Build body
            var bodyNode = node.Children.FirstOrDefault(c => c.Value == "WhileBody");
            if (bodyNode != null)
            {
                foreach (var stmt in bodyNode.Children)
                {
                    var buildStmt = BuildStatement(stmt);
                    if (buildStmt != null) whileStmt.Body.Statements.Add(buildStmt);
                }
            }

            return whileStmt;
        }

        private DoLoopStatementNode BuildDoLoopStatement(VB6SyntaxNode node)
        {
            var doStmt = new DoLoopStatementNode
            {
                IsDoWhile = node.Attributes.GetValueOrDefault("IsDoWhile", "False").Equals("True", StringComparison.OrdinalIgnoreCase),
                IsUntil = node.Attributes.GetValueOrDefault("IsUntil", "False").Equals("True", StringComparison.OrdinalIgnoreCase)
            };

            // First non-body child is the condition (if any)
            var conditionNode = node.Children.FirstOrDefault(c => c.Value != "DoBody");
            if (conditionNode != null)
            {
                doStmt.Condition = BuildExpression(conditionNode);
            }

            // Build body
            var bodyNode = node.Children.FirstOrDefault(c => c.Value == "DoBody");
            if (bodyNode != null)
            {
                foreach (var stmt in bodyNode.Children)
                {
                    var buildStmt = BuildStatement(stmt);
                    if (buildStmt != null) doStmt.Body.Statements.Add(buildStmt);
                }
            }

            return doStmt;
        }

        private SelectCaseStatementNode BuildSelectCaseStatement(VB6SyntaxNode node)
        {
            var selectStmt = new SelectCaseStatementNode();

            // First child is the test expression
            if (node.Children.Count > 0 && node.Children[0].Type == NodeType.Expression)
            {
                selectStmt.TestExpression = BuildExpression(node.Children[0])!;
            }

            // Process Case clauses
            foreach (var child in node.Children.Where(c => c.Value == "Case" || c.Value == "CaseElse"))
            {
                if (child.Value == "CaseElse")
                {
                    selectStmt.CaseElseBlock = new BlockNode();
                    foreach (var stmt in child.Children)
                    {
                        var buildStmt = BuildStatement(stmt);
                        if (buildStmt != null) selectStmt.CaseElseBlock.Statements.Add(buildStmt);
                    }
                }
                else
                {
                    var caseClause = new CaseClauseNode();

                    // Check for comparison or range
                    caseClause.IsComparison = child.Attributes.GetValueOrDefault("IsComparison", "False")
                        .Equals("True", StringComparison.OrdinalIgnoreCase);
                    caseClause.IsRange = child.Attributes.GetValueOrDefault("IsRange", "False")
                        .Equals("True", StringComparison.OrdinalIgnoreCase);

                    if (caseClause.IsComparison)
                    {
                        caseClause.ComparisonOperator = child.Attributes.GetValueOrDefault("ComparisonOperator", "=");
                    }

                    // Get case values (non-body children)
                    var bodyNode = child.Children.FirstOrDefault(c => c.Value == "CaseBody");
                    foreach (var valueChild in child.Children.Where(c => c.Value != "CaseBody"))
                    {
                        var expr = BuildExpression(valueChild);
                        if (expr != null)
                        {
                            if (caseClause.IsRange && caseClause.Values.Count == 1)
                            {
                                caseClause.RangeEnd = expr;
                            }
                            else
                            {
                                caseClause.Values.Add(expr);
                            }
                        }
                    }

                    // Build body
                    if (bodyNode != null)
                    {
                        foreach (var stmt in bodyNode.Children)
                        {
                            var buildStmt = BuildStatement(stmt);
                            if (buildStmt != null) caseClause.Body.Statements.Add(buildStmt);
                        }
                    }

                    selectStmt.Cases.Add(caseClause);
                }
            }

            return selectStmt;
        }

        private VB6Accessibility DetermineAccessibility(VB6SyntaxNode node)
        {
            if (node.Attributes.TryGetValue("Accessibility", out string acc))
            {
                return acc.ToLowerInvariant() switch
                {
                    "public" => VB6Accessibility.Public,
                    "friend" => VB6Accessibility.Friend,
                    "static" => VB6Accessibility.Static,
                    _ => VB6Accessibility.Private
                };
            }
            return VB6Accessibility.Private;
        }

        private static PropertyProcedureKind ParsePropertyKind(string propertyKind)
        {
            return propertyKind.ToLowerInvariant() switch
            {
                "let" => PropertyProcedureKind.Let,
                "set" => PropertyProcedureKind.Set,
                _ => PropertyProcedureKind.Get
            };
        }
    }

}
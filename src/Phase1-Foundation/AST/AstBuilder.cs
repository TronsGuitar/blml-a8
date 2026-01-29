using System;
using System.Collections.Generic;
using System.Linq;

namespace BLML.Phase1Foundation.AST
{
    public class AstBuilder
    {
        public ModuleNode BuildModule(VB6SyntaxNode syntaxNode)
        {
            if (syntaxNode == null) return null;
            
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
            if (node == null) return null;

            switch (node.Type)
            {
                case NodeType.Function:
                case NodeType.Sub:
                    return BuildMethod(node);
                case NodeType.Variable:
                    return BuildVariable(node);
                case NodeType.Property:
                    return BuildMethod(node); // In AST, properties can be treated as special methods or handled separately
                default:
                    return null;
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
                else if (child.Type == NodeType.Statement)
                {
                    method.Body.Add(BuildStatement(child));
                }
            }

            return method;
        }

        private ParameterNode BuildParameter(VB6SyntaxNode node)
        {
            return new ParameterNode
            {
                Name = node.Value,
                Type = node.Attributes.GetValueOrDefault("Type", "Variant"),
                IsByRef = !node.Attributes.GetValueOrDefault("ByVal", "false").Equals("true", StringComparison.OrdinalIgnoreCase),
                IsOptional = node.Attributes.GetValueOrDefault("Optional", "false").Equals("true", StringComparison.OrdinalIgnoreCase),
                DefaultValue = node.Attributes.GetValueOrDefault("DefaultValue", "")
            };
        }

        private VariableDeclarationNode BuildVariable(VB6SyntaxNode node)
        {
            return new VariableDeclarationNode
            {
                Name = node.Value,
                Type = node.Attributes.GetValueOrDefault("Type", "Variant"),
                Accessibility = DetermineAccessibility(node),
                InitialValue = node.Attributes.GetValueOrDefault("InitialValue", "")
            };
        }

        private StatementNode BuildStatement(VB6SyntaxNode node)
        {
            if (node == null) return null;

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
                    return null;
                default:
                    return null;
            }
        }

        private ExpressionNode BuildExpression(VB6SyntaxNode node)
        {
            if (node == null) return null;

            if (node.Type == NodeType.Expression)
            {
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
    }

}

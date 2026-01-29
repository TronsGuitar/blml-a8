using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BLML.Phase1Foundation.AST;

namespace BLML.Phase1Foundation.Parser
{
    public class VB6CodeGenerator
    {
        public string GenerateCSharpCode(ModuleNode module)
        {
            if (module == null) return string.Empty;

            var compilation = CSharpCompilation.Create("VB6Converted")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var syntaxTree = CSharpSyntaxTree.Create(
                GenerateCompilationUnit(module).NormalizeWhitespace()
            );

            return syntaxTree.ToString();
        }

        private CompilationUnitSyntax GenerateCompilationUnit(ModuleNode module)
        {
            var usings = new List<UsingDirectiveSyntax>
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
            };

            var members = new List<MemberDeclarationSyntax>();
            
            // In C#, we usually wrap logic in a class
            var classDecl = SyntaxFactory.ClassDeclaration(module.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            foreach (var decl in module.Declarations)
            {
                var member = GenerateMember(decl);
                if (member != null)
                {
                    classDecl = classDecl.AddMembers(member);
                }
            }

            members.Add(classDecl);

            return SyntaxFactory.CompilationUnit()
                .AddUsings(usings.ToArray())
                .AddMembers(members.ToArray());
        }

        private MemberDeclarationSyntax GenerateMember(DeclarationNode node)
        {
            return node switch
            {
                MethodDeclarationNode method => GenerateMethod(method),
                VariableDeclarationNode variable => GenerateField(variable),
                _ => null
            };
        }

        private MethodDeclarationSyntax GenerateMethod(MethodDeclarationNode node)
        {
            var returnType = ParseVB6Type(node.ReturnType);
            
            var parameters = node.Parameters.Select(p => 
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                    .WithType(ParseVB6Type(p.Type))
                    .AddModifiers(p.IsByRef ? SyntaxFactory.Token(SyntaxKind.RefKeyword) : default)
            ).ToArray();

            var bodyStatements = node.Body.Select(GenerateStatement).Where(s => s != null).ToArray();
            var body = SyntaxFactory.Block(bodyStatements);

            var method = SyntaxFactory.MethodDeclaration(returnType, node.Name)
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
                .WithBody(body);

            // Add modifiers
            if (node.Accessibility == VB6Accessibility.Public)
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            else if (node.Accessibility == VB6Accessibility.Friend)
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
            else
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            if (node.Accessibility == VB6Accessibility.Static)
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            return method;
        }

        private FieldDeclarationSyntax GenerateField(VariableDeclarationNode node)
        {
            var variable = SyntaxFactory.VariableDeclaration(ParseVB6Type(node.Type))
                .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(node.Name)));

            var field = SyntaxFactory.FieldDeclaration(variable);

            if (node.Accessibility == VB6Accessibility.Public)
                field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            else if (node.Accessibility == VB6Accessibility.Friend)
                field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
            else
                field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            return field;
        }

        private StatementSyntax GenerateStatement(StatementNode node)
        {
            if (node is ExpressionStatementNode exprStmt)
            {
                var expr = GenerateExpression(exprStmt.Expression);
                return SyntaxFactory.ExpressionStatement(expr);
            }
            if (node is AssignmentNode assign)
            {
                var target = GenerateExpression(assign.Target);
                var value = GenerateExpression(assign.Value);
                var kind = SyntaxKind.SimpleAssignmentExpression;
                return SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(kind, target, value));
            }
            if (node is IfStatementNode ifStmt)
            {
                var condition = GenerateExpression(ifStmt.Condition);
                var trueBlock = SyntaxFactory.Block(ifStmt.TrueBlock.Statements.Select(GenerateStatement));
                var elseClause = ifStmt.ElseBlock != null 
                    ? SyntaxFactory.ElseClause(SyntaxFactory.Block(ifStmt.ElseBlock.Statements.Select(GenerateStatement))) 
                    : null;
                
                return SyntaxFactory.IfStatement(condition, trueBlock, elseClause);
            }
            if (node is VariableDeclarationNode varDecl)
            {
                // Local variable declaration
                var variable = SyntaxFactory.VariableDeclaration(ParseVB6Type(varDecl.Type))
                    .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(varDecl.Name)));
                return SyntaxFactory.LocalDeclarationStatement(variable);
            }

            return SyntaxFactory.EmptyStatement();
        }

        private ExpressionSyntax GenerateExpression(ExpressionNode node)
        {
            if (node is LiteralExpressionNode literal)
            {
                if (literal.Value is string s)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s));
                if (literal.Value is int i)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
                if (literal.Value is double d)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d));
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            if (node is IdentifierExpressionNode ident)
            {
                return SyntaxFactory.IdentifierName(ident.Name);
            }
            if (node is BinaryExpressionNode binary)
            {
                var kind = binary.Operator switch
                {
                    "+" => SyntaxKind.AddExpression,
                    "-" => SyntaxKind.SubtractExpression,
                    "*" => SyntaxKind.MultiplyExpression,
                    "/" => SyntaxKind.DivideExpression,
                    "&" => SyntaxKind.AddExpression, // Concatenation
                    "=" => SyntaxKind.EqualsExpression,
                    "<>" => SyntaxKind.NotEqualsExpression,
                    "<" => SyntaxKind.LessThanExpression,
                    ">" => SyntaxKind.GreaterThanExpression,
                    "<=" => SyntaxKind.LessThanOrEqualExpression,
                    ">=" => SyntaxKind.GreaterThanOrEqualExpression,
                    _ => SyntaxKind.None
                };

                if (kind == SyntaxKind.None) return SyntaxFactory.IdentifierName($"/* Unsupported Op: {binary.Operator} */");

                return SyntaxFactory.BinaryExpression(kind, GenerateExpression(binary.Left), GenerateExpression(binary.Right));
            }
            if (node is InvocationExpressionNode invoke)
            {
                var args = invoke.Arguments.Select(a => SyntaxFactory.Argument(GenerateExpression(a))).ToArray();
                return SyntaxFactory.InvocationExpression(GenerateExpression(invoke.Target))
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args)));
            }
            return SyntaxFactory.IdentifierName("/* Unsupported Expression */");
        }

        private TypeSyntax ParseVB6Type(string vb6Type)
        {
            switch (vb6Type?.ToLowerInvariant())
            {
                case "string": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
                case "integer": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
                case "long": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword));
                case "single": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword));
                case "double": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
                case "boolean": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));
                case "date": return SyntaxFactory.IdentifierName("DateTime");
                case "variant":
                case "object": return SyntaxFactory.IdentifierName("object");
                case "void": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
                default: return SyntaxFactory.IdentifierName(vb6Type ?? "object");
            }
        }
    }
}

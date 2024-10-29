using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VB6MethodTests
{
    [TestClass]
    public class VB6MethodTests
    {
        [TestMethod]
        public void GenerateCodeUsingRoslyn()
        {
            // Create the test class
            ClassDeclarationSyntax testClass = CSharpSyntaxFactory.ClassDeclaration("VB6MethodTests")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ClassKeyword)))
                .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName("TestClass")))))
                .WithMembers(List(GenerateTestMethods()));

            // Create the namespace
            NamespaceDeclarationSyntax testNamespace = CSharpSyntaxFactory.NamespaceDeclaration("VB6MethodTests")
                .WithMembers(List(testClass));

            // Create the compilation unit
            CompilationUnitSyntax compilationUnit = CSharpSyntaxFactory.CompilationUnit()
                .WithUsings(List(
                    CSharpSyntaxFactory.UsingDirective(IdentifierName("Microsoft.VisualStudio.TestTools.UnitTesting")),
                    CSharpSyntaxFactory.UsingDirective(IdentifierName("System"))
                ))
                .WithMembers(List(testNamespace));

            // Verify the generated code
            string generatedCode = compilationUnit.ToFullString();
            Console.WriteLine(generatedCode);
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateTestMethods()
        {
            yield return GenerateTestMethod("TestError", "TestError");
            yield return GenerateTestMethod("TestLet", "TestLet");
            yield return GenerateTestMethod("TestTestSub", "TestTestSub");
            yield return GenerateTestMethod("TestProcessArray", "TestProcessArray");
            yield return GenerateTestMethod("TestProcessData", "TestProcessData");
            yield return GenerateTestMethod("TestGetWindowText", "TestGetWindowText");
            yield return GenerateTestMethod("TestValue", "TestValue");
            yield return GenerateTestMethod("TestProcessItems", "TestProcessItems");
            yield return GenerateTestMethod("TestChDrive", "TestChDrive");
            yield return GenerateTestMethod("TestChDir", "TestChDir");
            yield return GenerateTestMethod("TestLinkPoke", "TestLinkPoke");
            yield return GenerateTestMethod("TestCreateObject", "TestCreateObject");
            yield return GenerateTestMethod("TestMove", "TestMove");
        }

        private MethodDeclarationSyntax GenerateTestMethod(string methodName, string testName)
        {
            // Generate the method body
            BlockSyntax methodBody = CSharpSyntaxFactory.Block(
                // Arrange
                CSharpSyntaxFactory.LocalDeclarationStatement(
                    VariableDeclaration(
                        PredefinedType(Token(SyntaxKind.IntKeyword)),
                        SingleVariableDesignation(IdentifierName("error"))
                    )
                ),
                CSharpSyntaxFactory.ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("error"),
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(100))
                    )
                ),
                // Act
                CSharpSyntaxFactory.ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("error"),
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(100))
                    )
                ),
                // Assert
                CSharpSyntaxFactory.ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Assert"),
                            IdentifierName("AreEqual")
                        )
                    )
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new[] {
                                    Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(100))),
                                    Argument(IdentifierName("error"))
                                }
                            )
                        )
                    )
                )
            );

            // Generate the test method
            return CSharpSyntaxFactory.MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier(methodName)
            )
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.TestMethodAttribute)))
            .WithBody(methodBody);
        }

        private static SyntaxToken Token(SyntaxKind kind)
        {
            return SyntaxFactory.Token(kind);
        }

        private static SeparatedSyntaxList<SyntaxToken> TokenList(params SyntaxToken[] tokens)
        {
            return SyntaxFactory.TokenList(tokens);
        }

        private static BaseListSyntax BaseList(params SeparatedSyntaxList<BaseTypeSyntax> baseLists)
        {
            return SyntaxFactory.BaseList(baseLists);
        }

        private static SeparatedSyntaxList<BaseTypeSyntax> SingletonSeparatedList(BaseTypeSyntax type)
        {
            return SyntaxFactory.SingletonSeparatedList(type);
        }

        private static SimpleBaseTypeSyntax SimpleBaseType(NameSyntax name)
        {
            return SyntaxFactory.SimpleBaseType(name);
        }

        private static ListSyntax<MemberDeclarationSyntax> List(IEnumerable<MemberDeclarationSyntax> members)
        {
            return SyntaxFactory.List(members);
        }

        private static NamespaceDeclarationSyntax NamespaceDeclaration(string name)
        {
            return SyntaxFactory.NamespaceDeclaration(IdentifierName(name));
        }

        private static ClassDeclarationSyntax ClassDeclaration(string name)
        {
            return SyntaxFactory.ClassDeclaration(name);
        }

        private static CompilationUnitSyntax CompilationUnit()
        {
            return SyntaxFactory.CompilationUnit();
        }

        private static UsingDirectiveSyntax UsingDirective(NameSyntax name)
        {
            return SyntaxFactory.UsingDirective(name);
        }

        private static IdentifierNameSyntax IdentifierName(string identifier)
        {
            return SyntaxFactory.IdentifierName(identifier);
        }

        private static LiteralExpressionSyntax Literal(object value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, params VariableDeclaratorSyntax[] variables)
        {
            return SyntaxFactory.VariableDeclaration(type, SyntaxFactory.SeparatedList(variables));
        }

        private static VariableDeclaratorSyntax SingleVariableDesignation(IdentifierNameSyntax identifier)
        {
            return SyntaxFactory.VariableDeclarator(identifier);
        }

        private static AssignmentExpressionSyntax AssignmentExpression(
            SyntaxKind kind,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            return SyntaxFactory.AssignmentExpression(kind, left, right);
        }

        private static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax expression)
        {
            return SyntaxFactory.InvocationExpression(expression);
        }

        private static ArgumentListSyntax ArgumentList(params ArgumentSyntax[] arguments)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
        }

        private static ArgumentSyntax Argument(ExpressionSyntax expression)
        {
            return SyntaxFactory.Argument(expression);
        }

        private static BlockSyntax Block(params StatementSyntax[] statements)
        {
            return SyntaxFactory.Block(statements);
        }

        private static ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression)
        {
            return SyntaxFactory.ExpressionStatement(expression);
        }

        private static LocalDeclarationStatementSyntax LocalDeclarationStatement(VariableDeclarationSyntax declaration)
        {
            return SyntaxFactory.LocalDeclarationStatement(declaration);
        }

        private static MemberAccessExpressionSyntax MemberAccessExpression(
            SyntaxKind kind,
            ExpressionSyntax expression,
            IdentifierNameSyntax name)
        {
            return SyntaxFactory.MemberAccessExpression(kind, expression, name);
        }

        private static MethodDeclarationSyntax MethodDeclaration(
            TypeSyntax returnType,
            IdentifierNameSyntax identifier)
        {
            return SyntaxFactory.MethodDeclaration(returnType, identifier);
        }
    }
}

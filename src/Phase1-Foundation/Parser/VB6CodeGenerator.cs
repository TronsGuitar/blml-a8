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
        public string GenerateCSharpCode(VB6SyntaxNode node)
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

        private string GenerateExpression(VB6SyntaxNode node)
        {
            if (node.Type == NodeType.Expression)
            {
                if (BuiltInFunctionHandler.IsBuiltInFunction(node.Value))
                {
                    var args = node.Children.Select(GenerateExpression).ToArray();
                    return BuiltInFunctionHandler.GenerateCShrapCall(node.Value, args);
                }
                // Handle other expressions...
                return node.Value;
            }
            return node.Value;
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
    }
}

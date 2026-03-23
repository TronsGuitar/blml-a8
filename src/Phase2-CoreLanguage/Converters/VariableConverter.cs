using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase2CoreLanguage.Converters
{
    public class VariableConverter
    {
        public VariableConverter()
        {
        }

        public VariableDeclarationSyntax ConvertDeclaration(string name, string vbType, bool isArray = false)
        {
            var typeSyntax = ParseVB6Type(vbType);
            
            if (isArray)
            {
                // For simplicity, handle as generic List or Array
                // In a real migration, we'd need to know if it's fixed or dynamic
                typeSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("List"))
                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(typeSyntax)));
            }

            return SyntaxFactory.VariableDeclaration(typeSyntax)
                .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(name)));
        }

        private TypeSyntax ParseVB6Type(string vb6Type)
        {
            switch (vb6Type?.ToLowerInvariant())
            {
                case "string": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
                case "integer": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ShortKeyword)); // VB6 Integer is 16-bit
                case "long": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)); // VB6 Long is 32-bit
                case "single": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword));
                case "double": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
                case "boolean": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));
                case "date": return SyntaxFactory.IdentifierName("DateTime");
                case "variant":
                case "object": return SyntaxFactory.IdentifierName("object");
                case "byte": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword));
                case "currency": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DecimalKeyword));
                default: return SyntaxFactory.IdentifierName(vb6Type ?? "object");
            }
        }
    }
}

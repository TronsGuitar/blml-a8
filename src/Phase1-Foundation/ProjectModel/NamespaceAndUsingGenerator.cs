using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase1Foundation.ProjectModel
{
    public static class NamespaceAndUsingGenerator
    {
        public static List<UsingDirectiveSyntax> GenerateUsings(VB6Project? project, bool includeCommonFrameworkUsings)
        {
            var namespaces = new HashSet<string>(StringComparer.Ordinal)
            {
                "System"
            };

            if (includeCommonFrameworkUsings)
            {
                namespaces.Add("System.Collections.Generic");
                namespaces.Add("System.Linq");
                namespaces.Add("System.Windows.Forms");
            }

            if (project != null && (project.Forms.Count > 0 || project.UserControls.Count > 0))
            {
                namespaces.Add("System.Drawing");
            }

            return namespaces
                .Select(static ns => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns)))
                .ToList();
        }

        public static BaseNamespaceDeclarationSyntax WrapInNamespace(string? namespaceName, params MemberDeclarationSyntax[] members)
        {
            var sanitizedNamespace = SanitizeNamespace(namespaceName);
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(sanitizedNamespace))
                .AddMembers(members);
        }

        private static string SanitizeNamespace(string? namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                return "BLML.Generated";
            }

            var parts = namespaceName
                .Split(new[] { '.', ' ', '-', '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(SanitizeIdentifier)
                .Where(static part => !string.IsNullOrWhiteSpace(part))
                .ToArray();

            return parts.Length == 0 ? "BLML.Generated" : string.Join('.', parts);
        }

        private static string SanitizeIdentifier(string value)
        {
            var characters = value.Where(static c => char.IsLetterOrDigit(c) || c == '_').ToArray();
            if (characters.Length == 0)
            {
                return "Generated";
            }

            var identifier = new string(characters);
            if (char.IsDigit(identifier[0]))
            {
                identifier = "_" + identifier;
            }

            return identifier;
        }
    }
}

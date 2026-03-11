using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace BLML.Phase7Optimization.CodeCleanup
{
    public class DeadCodeRemover
    {
        private static readonly Regex CommentedOutCodeRegex = new(
            @"^//\s*(if\s*\(|for(each)?\s*\(|while\s*\(|return\b|var\s+|int\s+|string\s+|bool\s+|public\s+|private\s+|protected\s+|internal\s+|class\s+|void\s+|[A-Za-z_][A-Za-z0-9_<>]*\s+[A-Za-z_][A-Za-z0-9_]*\s*\(|[A-Za-z_][A-Za-z0-9_]*\s*=).*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] LegacyMarkerSnippets =
        {
            "converted from vb6",
            "legacy marker",
            "todo vb6",
            "made with santa claude"
        };

        public DeadCodeRemovalResult AnalyzeAndClean(string csharpCode)
        {
            ArgumentNullException.ThrowIfNull(csharpCode);

            var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
            var root = syntaxTree.GetRoot();

            var unusedPrivateMembers = FindUnusedMembers(root, SyntaxKind.PrivateKeyword);
            var potentiallyDeadPublicMembers = FindUnusedMembers(root, SyntaxKind.PublicKeyword);
            var unreachableStatements = FindUnreachableStatements(root);
            var removableCommentLines = FindRemovableCommentLines(csharpCode);

            return new DeadCodeRemovalResult
            {
                UnusedPrivateMembers = unusedPrivateMembers,
                PotentiallyDeadPublicMembers = potentiallyDeadPublicMembers,
                UnreachableStatementLines = unreachableStatements,
                RemovedCommentLineNumbers = removableCommentLines,
                CleanedCode = RemoveCommentedOutCodeAndLegacyMarkers(csharpCode, removableCommentLines)
            };
        }

        public string RemoveCommentedOutCodeAndLegacyMarkers(string csharpCode)
        {
            ArgumentNullException.ThrowIfNull(csharpCode);
            return RemoveCommentedOutCodeAndLegacyMarkers(csharpCode, FindRemovableCommentLines(csharpCode));
        }

        private static IReadOnlyList<string> FindUnusedMembers(SyntaxNode root, SyntaxKind accessibilityKind)
        {
            var members = new List<string>();

            foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                if (!field.Modifiers.Any(accessibilityKind))
                {
                    continue;
                }

                foreach (var variable in field.Declaration.Variables)
                {
                    if (CountIdentifierOccurrences(root, variable.Identifier.ValueText) == 1)
                    {
                        members.Add(variable.Identifier.ValueText);
                    }
                }
            }

            foreach (var property in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
            {
                if (property.Modifiers.Any(accessibilityKind) && CountIdentifierOccurrences(root, property.Identifier.ValueText) == 1)
                {
                    members.Add(property.Identifier.ValueText);
                }
            }

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (method.Modifiers.Any(accessibilityKind) && CountIdentifierOccurrences(root, method.Identifier.ValueText) == 1)
                {
                    members.Add(method.Identifier.ValueText);
                }
            }

            return members
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        private static int CountIdentifierOccurrences(SyntaxNode root, string identifier)
        {
            return root.DescendantTokens().Count(token => token.IsKind(SyntaxKind.IdentifierToken) && token.ValueText == identifier);
        }

        private static IReadOnlyList<int> FindUnreachableStatements(SyntaxNode root)
        {
            var lines = new List<int>();

            foreach (var block in root.DescendantNodes().OfType<BlockSyntax>())
            {
                var blockHasTerminator = false;

                foreach (var statement in block.Statements)
                {
                    if (statement is LabeledStatementSyntax)
                    {
                        blockHasTerminator = false;
                    }

                    if (blockHasTerminator)
                    {
                        lines.Add(statement.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
                    }

                    if (IsTerminatingStatement(statement))
                    {
                        blockHasTerminator = true;
                    }
                }
            }

            return lines.Distinct().OrderBy(line => line).ToArray();
        }

        private static bool IsTerminatingStatement(StatementSyntax statement)
        {
            return statement is ReturnStatementSyntax or ThrowStatementSyntax or BreakStatementSyntax or ContinueStatementSyntax or GotoStatementSyntax;
        }

        private static IReadOnlyList<int> FindRemovableCommentLines(string csharpCode)
        {
            var lines = csharpCode.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var removableLines = new List<int>();

            for (var index = 0; index < lines.Length; index++)
            {
                var trimmed = lines[index].Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                if (IsCommentedOutCode(trimmed) || ContainsLegacyMarker(trimmed))
                {
                    removableLines.Add(index + 1);
                }
            }

            return removableLines;
        }

        private static bool IsCommentedOutCode(string trimmedLine)
        {
            return CommentedOutCodeRegex.IsMatch(trimmedLine);
        }

        private static bool ContainsLegacyMarker(string trimmedLine)
        {
            return trimmedLine.StartsWith("//", StringComparison.Ordinal) &&
                   LegacyMarkerSnippets.Any(snippet => trimmedLine.Contains(snippet, StringComparison.OrdinalIgnoreCase));
        }

        private static string RemoveCommentedOutCodeAndLegacyMarkers(string csharpCode, IReadOnlyList<int> removableLines)
        {
            var removalSet = removableLines.ToHashSet();
            var lines = csharpCode.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

            return string.Join(Environment.NewLine, lines.Where((_, index) => !removalSet.Contains(index + 1)));
        }
    }

    public sealed class DeadCodeRemovalResult
    {
        public IReadOnlyList<string> UnusedPrivateMembers { get; init; } = Array.Empty<string>();

        public IReadOnlyList<string> PotentiallyDeadPublicMembers { get; init; } = Array.Empty<string>();

        public IReadOnlyList<int> UnreachableStatementLines { get; init; } = Array.Empty<int>();

        public IReadOnlyList<int> RemovedCommentLineNumbers { get; init; } = Array.Empty<int>();

        public string CleanedCode { get; init; } = string.Empty;
    }
}

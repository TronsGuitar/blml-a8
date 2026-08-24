using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase6Advanced.Collections
{
    /// <summary>
    /// Converts VB6's `Collection` object. VB6 Collections are unusual: every element
    /// can optionally have a string key (`coll.Add item, "key"`) alongside its
    /// 1-based positional index, so there is no single C# collection type that's a
    /// faithful drop-in replacement - `List&lt;T&gt;` has no keys, `Dictionary&lt;TKey,TValue&gt;`
    /// has no positional index. Rather than guess, this exposes both targets
    /// (<see cref="ConvertNewCollection"/> takes a `usesKeys` flag) and leaves the
    /// choice to the caller, who can determine it by checking whether any `.Add` call
    /// on the collection supplies a key argument - VB6CodeGenerator does not currently
    /// call into this (see Phase6-Advanced/README.md's "Not implemented yet" section).
    /// </summary>
    public class CollectionConverter
    {
        public ExpressionSyntax ConvertNewCollection(bool usesKeys) =>
            usesKeys
                ? SyntaxFactory.ParseExpression("new System.Collections.Generic.Dictionary<string, object>()")
                : SyntaxFactory.ParseExpression("new System.Collections.Generic.List<object>()");

        /// <summary>`coll.Add item` -&gt; `list.Add(item)`; `coll.Add item, "key"` -&gt; `dict["key"] = item`.</summary>
        public StatementSyntax ConvertAdd(string collectionVar, ExpressionSyntax item, ExpressionSyntax? key)
        {
            if (key is null)
            {
                return SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{collectionVar}.Add({item})"));
            }

            var indexer = SyntaxFactory.ElementAccessExpression(SyntaxFactory.IdentifierName(collectionVar))
                .AddArgumentListArguments(SyntaxFactory.Argument(key));
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, indexer, item));
        }

        /// <summary>`coll.Remove(1)` -&gt; `list.RemoveAt(0)` (VB6 is 1-based); `coll.Remove("key")` -&gt; `dict.Remove("key")`.</summary>
        public StatementSyntax ConvertRemove(string collectionVar, ExpressionSyntax indexOrKey, bool isKeyed)
        {
            if (isKeyed)
            {
                return SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{collectionVar}.Remove({indexOrKey})"));
            }

            var zeroBasedIndex = SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, indexOrKey, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1)));
            return SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(collectionVar), SyntaxFactory.IdentifierName("RemoveAt")))
                .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.ParenthesizedExpression(zeroBasedIndex))));
        }

        /// <summary>`coll(1)` / `coll.Item(1)` -&gt; `list[0]` (VB6 is 1-based); `coll("key")` -&gt; `dict["key"]`.</summary>
        public ExpressionSyntax ConvertItemAccess(string collectionVar, ExpressionSyntax indexOrKey, bool isKeyed)
        {
            var key = isKeyed
                ? indexOrKey
                : SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, indexOrKey, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))));

            return SyntaxFactory.ElementAccessExpression(SyntaxFactory.IdentifierName(collectionVar))
                .AddArgumentListArguments(SyntaxFactory.Argument(key));
        }

        /// <summary>`coll.Count` -&gt; `list.Count` / `dict.Count` - identical member name on both C# targets, so no branching needed.</summary>
        public ExpressionSyntax ConvertCount(string collectionVar) =>
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(collectionVar), SyntaxFactory.IdentifierName("Count"));
    }
}

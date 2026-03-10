using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase7Optimization.Refactoring
{
    public class LinqOptimizer
    {
        public IReadOnlyList<LinqOptimizationSuggestion> SuggestOptimizations(string csharpCode)
        {
            ArgumentNullException.ThrowIfNull(csharpCode);

            var root = CSharpSyntaxTree.ParseText(csharpCode).GetRoot();
            var suggestions = new List<LinqOptimizationSuggestion>();

            foreach (var forEach in root.DescendantNodes().OfType<ForEachStatementSyntax>())
            {
                var statements = ExtractStatements(forEach.Statement);

                if (TryCreateCountSuggestion(forEach, statements, out var countSuggestion))
                {
                    suggestions.Add(countSuggestion!);
                }

                if (TryCreateSumSuggestion(forEach, statements, out var sumSuggestion))
                {
                    suggestions.Add(sumSuggestion!);
                }

                if (TryCreateProjectionSuggestion(forEach, statements, out var projectionSuggestion))
                {
                    suggestions.Add(projectionSuggestion!);
                }
            }

            return suggestions;
        }

        private static IReadOnlyList<StatementSyntax> ExtractStatements(StatementSyntax statement)
        {
            return statement is BlockSyntax block
                ? block.Statements
                : new[] { statement };
        }

        private static bool TryCreateCountSuggestion(
            ForEachStatementSyntax forEach,
            IReadOnlyList<StatementSyntax> statements,
            out LinqOptimizationSuggestion? suggestion)
        {
            suggestion = null;

            if (statements.Count != 1)
            {
                return false;
            }

            var source = forEach.Expression.ToString();
            var loopVariable = forEach.Identifier.ValueText;

            if (TryGetCounterIncrement(statements[0], out var counterName))
            {
                suggestion = CreateSuggestion(
                    "Count",
                    forEach,
                    $"Manual count loop over '{source}' can be replaced with Count().",
                    $"var {counterName} = {source}.Count();");
                return true;
            }

            if (statements[0] is IfStatementSyntax ifStatement &&
                TryGetCounterIncrement(GetSingleStatement(ifStatement.Statement), out counterName))
            {
                suggestion = CreateSuggestion(
                    "Count",
                    forEach,
                    $"Conditional count loop over '{source}' can be replaced with Count(predicate).",
                    $"var {counterName} = {source}.Count({loopVariable} => {ifStatement.Condition});");
                return true;
            }

            return false;
        }

        private static bool TryCreateSumSuggestion(
            ForEachStatementSyntax forEach,
            IReadOnlyList<StatementSyntax> statements,
            out LinqOptimizationSuggestion? suggestion)
        {
            suggestion = null;

            if (statements.Count != 1 || statements[0] is not ExpressionStatementSyntax expressionStatement)
            {
                return false;
            }

            if (expressionStatement.Expression is AssignmentExpressionSyntax assignment &&
                assignment.Kind() == SyntaxKind.AddAssignmentExpression &&
                assignment.Left is IdentifierNameSyntax accumulator)
            {
                suggestion = CreateSuggestion(
                    "Sum",
                    forEach,
                    $"Manual accumulation into '{accumulator.Identifier.ValueText}' can be replaced with Sum().",
                    $"var {accumulator.Identifier.ValueText} = {forEach.Expression}.Sum({forEach.Identifier.ValueText} => {assignment.Right});");
                return true;
            }

            return false;
        }

        private static bool TryCreateProjectionSuggestion(
            ForEachStatementSyntax forEach,
            IReadOnlyList<StatementSyntax> statements,
            out LinqOptimizationSuggestion? suggestion)
        {
            suggestion = null;
            var source = forEach.Expression.ToString();
            var loopVariable = forEach.Identifier.ValueText;

            if (statements.Count != 1)
            {
                return false;
            }

            if (TryGetAddInvocation(statements[0], out var targetCollection, out var projectionExpression))
            {
                suggestion = CreateSuggestion(
                    "Projection",
                    forEach,
                    $"Manual projection loop into '{targetCollection}' can be replaced with Select().",
                    $"var {targetCollection} = {source}.Select({loopVariable} => {projectionExpression}).ToList();");
                return true;
            }

            if (statements[0] is IfStatementSyntax ifStatement &&
                TryGetAddInvocation(GetSingleStatement(ifStatement.Statement), out targetCollection, out projectionExpression))
            {
                suggestion = CreateSuggestion(
                    "Projection",
                    forEach,
                    $"Filtered projection loop into '{targetCollection}' can be replaced with Where().Select().",
                    $"var {targetCollection} = {source}.Where({loopVariable} => {ifStatement.Condition}).Select({loopVariable} => {projectionExpression}).ToList();");
                return true;
            }

            return false;
        }

        private static bool TryGetCounterIncrement(StatementSyntax statement, out string counterName)
        {
            counterName = string.Empty;

            return statement switch
            {
                ExpressionStatementSyntax
                {
                    Expression: PostfixUnaryExpressionSyntax
                    {
                        RawKind: (int)SyntaxKind.PostIncrementExpression,
                        Operand: IdentifierNameSyntax identifier
                    }
                } => TryAssignCounterName(identifier, out counterName),
                ExpressionStatementSyntax
                {
                    Expression: PrefixUnaryExpressionSyntax
                    {
                        RawKind: (int)SyntaxKind.PreIncrementExpression,
                        Operand: IdentifierNameSyntax identifier
                    }
                } => TryAssignCounterName(identifier, out counterName),
                _ => false
            };
        }

        private static bool TryGetAddInvocation(StatementSyntax statement, out string collectionName, out string projectionExpression)
        {
            collectionName = string.Empty;
            projectionExpression = string.Empty;

            if (statement is not ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax memberAccess,
                        ArgumentList.Arguments.Count: 1
                    } invocation
                })
            {
                return false;
            }

            if (!memberAccess.Name.Identifier.ValueText.Equals("Add", StringComparison.Ordinal))
            {
                return false;
            }

            collectionName = memberAccess.Expression.ToString();
            projectionExpression = invocation.ArgumentList.Arguments[0].ToString();
            return true;
        }

        private static StatementSyntax GetSingleStatement(StatementSyntax statement)
        {
            return statement is BlockSyntax block && block.Statements.Count == 1
                ? block.Statements[0]
                : statement;
        }

        private static bool TryAssignCounterName(IdentifierNameSyntax identifier, out string counterName)
        {
            counterName = identifier.Identifier.ValueText;
            return true;
        }

        private static LinqOptimizationSuggestion CreateSuggestion(
            string category,
            ForEachStatementSyntax forEach,
            string description,
            string suggestedReplacement)
        {
            return new LinqOptimizationSuggestion
            {
                Category = category,
                Description = description,
                SuggestedReplacement = suggestedReplacement,
                LineNumber = forEach.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            };
        }
    }

    public sealed class LinqOptimizationSuggestion
    {
        public string Category { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string SuggestedReplacement { get; init; } = string.Empty;

        public int LineNumber { get; init; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BLML.Phase1Foundation.AST;

namespace BLML.Phase2CoreLanguage.Converters
{
    public class ControlFlowConverter
    {
        // This converter specializes in complex control flow transformations.
        // It separates logic from the main CodeGenerator to keep concerns separated.

        public ControlFlowConverter()
        {
        }

        public StatementSyntax ConvertIfStatement(IfStatementNode node, Func<StatementNode, StatementSyntax> statementGenerator)
        {
            var condition = GenerateExpression(node.Condition);
            var trueBlock = SyntaxFactory.Block(node.TrueBlock.Statements.Select(statementGenerator));
            var elseClause = node.ElseBlock != null 
                ? SyntaxFactory.ElseClause(SyntaxFactory.Block(node.ElseBlock.Statements.Select(statementGenerator))) 
                : null;
            
            return SyntaxFactory.IfStatement(condition, trueBlock, elseClause);
        }

        public StatementSyntax ConvertSelectCase(SelectCaseStatementNode node, Func<StatementNode, StatementSyntax> statementGenerator)
        {
            // Analyze complexity
            var hasComplexCases = node.Cases.Any(c => c.IsRange || c.IsComparison || c.Values.Count > 1);
            var testExpr = GenerateExpression(node.TestExpression);

            if (hasComplexCases)
            {
                return GenerateSelectCaseAsIfElse(node, testExpr, statementGenerator);
            }
            else
            {
                return GenerateSelectCaseAsSwitch(node, testExpr, statementGenerator);
            }
        }

        private StatementSyntax GenerateSelectCaseAsSwitch(SelectCaseStatementNode node, ExpressionSyntax testExpr, Func<StatementNode, StatementSyntax> statementGenerator)
        {
            var sections = new List<SwitchSectionSyntax>();

            foreach (var caseClause in node.Cases)
            {
                if (caseClause.Values.Count > 0)
                {
                    var labels = caseClause.Values.Select(v =>
                        (SwitchLabelSyntax)SyntaxFactory.CaseSwitchLabel(GenerateExpression(v))).ToList();

                    var statements = caseClause.Body.Statements
                        .Select(statementGenerator)
                        .Where(s => s != null)
                        .ToList();
                    statements.Add(SyntaxFactory.BreakStatement());

                    sections.Add(SyntaxFactory.SwitchSection(
                        SyntaxFactory.List(labels),
                        SyntaxFactory.List(statements)));
                }
            }

            if (node.CaseElseBlock != null)
            {
                var defaultStatements = node.CaseElseBlock.Statements
                    .Select(statementGenerator)
                    .Where(s => s != null)
                    .ToList();
                defaultStatements.Add(SyntaxFactory.BreakStatement());

                sections.Add(SyntaxFactory.SwitchSection(
                    SyntaxFactory.SingletonList<SwitchLabelSyntax>(SyntaxFactory.DefaultSwitchLabel()),
                    SyntaxFactory.List(defaultStatements)));
            }

            return SyntaxFactory.SwitchStatement(testExpr, SyntaxFactory.List(sections));
        }

        private StatementSyntax GenerateSelectCaseAsIfElse(SelectCaseStatementNode node, ExpressionSyntax testExpr, Func<StatementNode, StatementSyntax> statementGenerator)
        {
            StatementSyntax result = null;

            // Build from bottom up (reverse) to wrap in Else clauses
            foreach (var caseClause in node.Cases.AsEnumerable().Reverse())
            {
                ExpressionSyntax condition = null;

                if (caseClause.IsComparison)
                {
                    var op = caseClause.ComparisonOperator switch
                    {
                        ">" => SyntaxKind.GreaterThanExpression,
                        "<" => SyntaxKind.LessThanExpression,
                        ">=" => SyntaxKind.GreaterThanOrEqualExpression,
                        "<=" => SyntaxKind.LessThanOrEqualExpression,
                        "<>" => SyntaxKind.NotEqualsExpression,
                        _ => SyntaxKind.EqualsExpression
                    };
                    condition = SyntaxFactory.BinaryExpression(op, testExpr, GenerateExpression(caseClause.Values[0]));
                }
                else if (caseClause.IsRange)
                {
                    var startExpr = GenerateExpression(caseClause.Values[0]);
                    var endExpr = GenerateExpression(caseClause.RangeEnd);
                    condition = SyntaxFactory.BinaryExpression(
                        SyntaxKind.LogicalAndExpression,
                        SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression, testExpr, startExpr),
                        SyntaxFactory.BinaryExpression(SyntaxKind.LessThanOrEqualExpression, testExpr, endExpr));
                }
                else if (caseClause.Values.Count > 1)
                {
                    condition = caseClause.Values
                        .Select(v => (ExpressionSyntax)SyntaxFactory.BinaryExpression(
                            SyntaxKind.EqualsExpression, testExpr, GenerateExpression(v)))
                        .Aggregate((left, right) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, left, right));
                }
                else if (caseClause.Values.Count == 1)
                {
                    condition = SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression, testExpr, GenerateExpression(caseClause.Values[0]));
                }

                if (condition != null)
                {
                    var bodyStatements = caseClause.Body.Statements.Select(statementGenerator).Where(s => s != null);
                    var body = SyntaxFactory.Block(bodyStatements);

                    var elseClause = result != null ? SyntaxFactory.ElseClause(result) : null;
                    result = SyntaxFactory.IfStatement(condition, body, elseClause);
                }
            }

            if (node.CaseElseBlock != null && result is IfStatementSyntax ifResult)
            {
                var elseStatements = node.CaseElseBlock.Statements.Select(statementGenerator).Where(s => s != null);
                var elseBody = SyntaxFactory.Block(elseStatements);
                result = AddElseToIfChain(ifResult, SyntaxFactory.ElseClause(elseBody));
            }

            return result ?? SyntaxFactory.EmptyStatement();
        }

        private IfStatementSyntax AddElseToIfChain(IfStatementSyntax ifStmt, ElseClauseSyntax elseClause)
        {
            if (ifStmt.Else == null)
            {
                return ifStmt.WithElse(elseClause);
            }
            else if (ifStmt.Else.Statement is IfStatementSyntax nestedIf)
            {
                return ifStmt.WithElse(SyntaxFactory.ElseClause(AddElseToIfChain(nestedIf, elseClause)));
            }
            return ifStmt;
        }

        private ExpressionSyntax GenerateExpression(ExpressionNode node)
        {
            // Simple expression generation helper for the converter. 
            // In a real scenario, this might call back to the main generator or have a shared ExpressionGenerator.
            if (node is LiteralExpressionNode literal)
            {
                if (literal.Value is string s) return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s));
                if (literal.Value is int i) return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            if (node is IdentifierExpressionNode ident) return SyntaxFactory.IdentifierName(ident.Name);
            
            return SyntaxFactory.IdentifierName("expr"); // Fallback
        }
    }
}

using BLML.Phase1Foundation.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase6Advanced;

public static class PropertyProcedureGenerator
{
    public static PropertyDeclarationSyntax? TryGenerateProperty(
        IReadOnlyList<PropertyDeclarationNode> procedures,
        Func<string, TypeSyntax> parseType,
        Func<StatementNode, StatementSyntax> generateStatement,
        Func<ExpressionNode, ExpressionSyntax> generateExpression)
    {
        ArgumentNullException.ThrowIfNull(procedures);
        ArgumentNullException.ThrowIfNull(parseType);
        ArgumentNullException.ThrowIfNull(generateStatement);
        ArgumentNullException.ThrowIfNull(generateExpression);

        if (procedures.Count == 0)
        {
            return null;
        }

        var getter = procedures.FirstOrDefault(p => p.PropertyKind == PropertyProcedureKind.Get);
        var setter = procedures.FirstOrDefault(p => p.PropertyKind is PropertyProcedureKind.Let or PropertyProcedureKind.Set);

        if (getter is null && setter is null)
        {
            return null;
        }

        var accessors = new List<AccessorDeclarationSyntax>();

        var getterAccessor = getter is not null
            ? BuildGetter(getter, generateStatement, generateExpression)
            : null;
        if (getterAccessor is not null)
        {
            accessors.Add(getterAccessor);
        }

        var setterAccessor = setter is not null
            ? BuildSetter(setter, generateStatement)
            : null;
        if (setterAccessor is not null)
        {
            accessors.Add(setterAccessor);
        }

        if (accessors.Count == 0)
        {
            return null;
        }

        var propertyType = getter?.Type;
        if (string.IsNullOrWhiteSpace(propertyType) || string.Equals(propertyType, "void", StringComparison.OrdinalIgnoreCase))
        {
            propertyType = setter?.Type;
        }

        if (string.IsNullOrWhiteSpace(propertyType) || string.Equals(propertyType, "void", StringComparison.OrdinalIgnoreCase))
        {
            propertyType = "object";
        }

        return SyntaxFactory.PropertyDeclaration(parseType(propertyType), procedures[0].Name)
            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)));
    }

    private static AccessorDeclarationSyntax? BuildGetter(
        PropertyDeclarationNode getter,
        Func<StatementNode, StatementSyntax> generateStatement,
        Func<ExpressionNode, ExpressionSyntax> generateExpression)
    {
        var statements = new List<StatementSyntax>();
        var foundReturn = false;

        foreach (var statement in getter.Body)
        {
            if (statement is AssignmentNode assignment &&
                assignment.Target is IdentifierExpressionNode identifier &&
                string.Equals(identifier.Name, getter.Name, StringComparison.OrdinalIgnoreCase))
            {
                statements.Add(SyntaxFactory.ReturnStatement(generateExpression(assignment.Value)));
                foundReturn = true;
                continue;
            }

            statements.Add(generateStatement(statement));
        }

        if (!foundReturn)
        {
            return null;
        }

        return SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithBody(SyntaxFactory.Block(statements));
    }

    private static AccessorDeclarationSyntax BuildSetter(
        PropertyDeclarationNode setter,
        Func<StatementNode, StatementSyntax> generateStatement)
    {
        var valueParameter = setter.Parameters.LastOrDefault();
        var statements = setter.Body
            .Select(generateStatement)
            .Select(statement => RewriteValueParameter(statement, valueParameter?.Name))
            .ToList();

        return SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            .WithBody(SyntaxFactory.Block(statements));
    }

    private static StatementSyntax RewriteValueParameter(StatementSyntax statement, string? parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName) || string.Equals(parameterName, "value", StringComparison.Ordinal))
        {
            return statement;
        }

        return (StatementSyntax)new SetterValueRewriter(parameterName).Visit(statement)!;
    }

    private sealed class SetterValueRewriter(string parameterName) : CSharpSyntaxRewriter
    {
        private readonly string _parameterName = parameterName;

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (string.Equals(node.Identifier.ValueText, _parameterName, StringComparison.OrdinalIgnoreCase))
            {
                return SyntaxFactory.IdentifierName("value").WithTriviaFrom(node);
            }

            return base.VisitIdentifierName(node);
        }
    }
}

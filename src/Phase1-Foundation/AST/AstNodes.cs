using System;
using System.Collections.Generic;

namespace BLML.Phase1Foundation.AST
{
    public abstract class AstNode
    {
        public List<AstNode> Children { get; } = new List<AstNode>();
    }

    public class ModuleNode : AstNode
    {
        public string Name { get; set; }
        public List<DeclarationNode> Declarations { get; } = new List<DeclarationNode>();
    }

    public enum VB6Accessibility
    {
        Public,
        Private,
        Friend,
        Static
    }

    public abstract class StatementNode : AstNode { }

    public abstract class DeclarationNode : StatementNode
    {
        public string Name { get; set; }
        public VB6Accessibility Accessibility { get; set; }
    }

    public class VariableDeclarationNode : DeclarationNode
    {
        public string Type { get; set; }
        public string InitialValue { get; set; }
        public bool IsArray { get; set; }
        public List<ExpressionNode> ArrayDimensions { get; } = new List<ExpressionNode>(); // For fixed-size arrays
    }

    public class MethodDeclarationNode : DeclarationNode
    {
        public bool IsFunction { get; set; }
        public string ReturnType { get; set; }
        public List<ParameterNode> Parameters { get; } = new List<ParameterNode>();
        public List<StatementNode> Body { get; } = new List<StatementNode>();
    }

    public enum PropertyProcedureKind
    {
        Get,
        Let,
        Set
    }

    public class PropertyDeclarationNode : DeclarationNode
    {
        public PropertyProcedureKind PropertyKind { get; set; }
        public string Type { get; set; }
        public List<ParameterNode> Parameters { get; } = new List<ParameterNode>();
        public List<StatementNode> Body { get; } = new List<StatementNode>();
    }

    public class ParameterNode : AstNode
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsByRef { get; set; }
        public bool IsOptional { get; set; }
        public string DefaultValue { get; set; }
        public ExpressionNode? DefaultValueExpression { get; set; }
    }

    public class ExpressionStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; set; }
    }

    public abstract class ExpressionNode : AstNode { }

    public class LiteralExpressionNode : ExpressionNode
    {
        public object Value { get; set; }
    }

    public class IdentifierExpressionNode : ExpressionNode
    {
        public string Name { get; set; }
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public string Operator { get; set; }
        public ExpressionNode Right { get; set; }
    }

    public class AssignmentNode : StatementNode
    {
        public ExpressionNode Target { get; set; }
        public ExpressionNode Value { get; set; }
    }

    public class BlockNode : AstNode
    {
        public List<StatementNode> Statements { get; } = new List<StatementNode>();
    }

    public class IfStatementNode : StatementNode
    {
        public ExpressionNode Condition { get; set; }
        public BlockNode TrueBlock { get; set; } = new BlockNode();
        public BlockNode? ElseBlock { get; set; }
    }

    public class InvocationExpressionNode : ExpressionNode
    {
        public ExpressionNode Target { get; set; }
        public List<ExpressionNode> Arguments { get; } = new List<ExpressionNode>();
    }

    // Loop statement nodes
    public class ForStatementNode : StatementNode
    {
        public string LoopVariable { get; set; } = string.Empty;
        public ExpressionNode StartValue { get; set; } = null!;
        public ExpressionNode EndValue { get; set; } = null!;
        public ExpressionNode? StepValue { get; set; }
        public BlockNode Body { get; set; } = new BlockNode();
    }

    public class WhileStatementNode : StatementNode
    {
        public ExpressionNode Condition { get; set; } = null!;
        public BlockNode Body { get; set; } = new BlockNode();
    }

    public class DoLoopStatementNode : StatementNode
    {
        public ExpressionNode? Condition { get; set; }
        public bool IsDoWhile { get; set; } // true = Do While/Until, false = Loop While/Until
        public bool IsUntil { get; set; }   // true = Until, false = While
        public BlockNode Body { get; set; } = new BlockNode();
    }

    // Select/Case statement nodes
    public class SelectCaseStatementNode : StatementNode
    {
        public ExpressionNode TestExpression { get; set; } = null!;
        public List<CaseClauseNode> Cases { get; } = new List<CaseClauseNode>();
        public BlockNode? CaseElseBlock { get; set; }
    }

    public class CaseClauseNode : AstNode
    {
        public List<ExpressionNode> Values { get; } = new List<ExpressionNode>();
        public bool IsRange { get; set; } // For "Case 1 To 10"
        public ExpressionNode? RangeEnd { get; set; } // End value for range
        public bool IsComparison { get; set; } // For "Case Is > 5"
        public string? ComparisonOperator { get; set; }
        public BlockNode Body { get; set; } = new BlockNode();
    }

    // Array-related nodes
    public class ArrayAccessExpressionNode : ExpressionNode
    {
        public ExpressionNode Array { get; set; } = null!;
        public List<ExpressionNode> Indices { get; } = new List<ExpressionNode>();
    }

    public class ReDimStatementNode : StatementNode
    {
        public string VariableName { get; set; } = string.Empty;
        public bool Preserve { get; set; }
        public List<ExpressionNode> NewDimensions { get; } = new List<ExpressionNode>();
    }
}

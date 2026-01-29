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
    }

    public class MethodDeclarationNode : DeclarationNode
    {
        public bool IsFunction { get; set; }
        public string ReturnType { get; set; }
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
}

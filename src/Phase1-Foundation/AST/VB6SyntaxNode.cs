namespace BLML.Phase1Foundation.AST
{
    public enum NodeType
    {
        Module,
        Class,
        Function,
        Sub,
        Property,
        Declaration,
        Statement,
        Expression,
        Type,
        Variable
    }

    public class VB6SyntaxNode
    {
        public NodeType Type { get; set; }
        public string Value { get; set; }
        public List<VB6SyntaxNode> Children { get; set; } = new List<VB6SyntaxNode>();
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
}

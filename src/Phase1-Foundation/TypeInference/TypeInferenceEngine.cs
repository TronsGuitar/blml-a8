using BLML.Phase1Foundation.AST;

namespace BLML.Phase1Foundation.TypeInference
{
    public class TypeInferenceEngine
    {
        private readonly List<string> errors = new List<string>();
        private readonly Dictionary<string, VB6SyntaxNode> symbolTable;

        public TypeInferenceEngine(Dictionary<string, VB6SyntaxNode> symbolTable)
        {
            this.symbolTable = symbolTable;
        }

        public List<string> PerformSemanticAnalysis(VB6SyntaxNode node)
        {
            errors.Clear();
            AnalyzeNode(node);
            return errors;
        }

        private void AnalyzeNode(VB6SyntaxNode node)
        {
            if (node == null) return;

            // Check for undefined variables in identifiers used in expressions/statements
            // Note: This logic depends on the specific node structure for usage vs declaration

            // Check for type compatibility in assignments
            if (node.Type == NodeType.Statement && node.Value == "=" && node.Children.Count == 2)
            {
                var target = node.Children[0];
                var expr = node.Children[1];

                string targetType = InferType(target);
                string exprType = InferType(expr);

                // VB6 is permissive, but we can flag obvious issues or document them
                if (IsIncompatible(targetType, exprType))
                {
                    errors.Add($"Potential type mismatch: Cannot assign {exprType} to {targetType} (at {node.Value})");
                }
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    AnalyzeNode(child);
                }
            }
        }

        public string InferType(VB6SyntaxNode node)
        {
            if (node == null) return "Variant";

            // If it's a declaration node, return its declared type
            if (node.Attributes.TryGetValue("Type", out string declaredType))
            {
                return declaredType;
            }

            // If it's an identifier, look it up in the symbol table
            if (symbolTable != null && symbolTable.TryGetValue(node.Value, out var declNode))
            {
                if (declNode.Attributes.TryGetValue("Type", out string symbolType))
                {
                    return symbolType;
                }
            }

            // Literal inference
            if (node.Value.StartsWith("\"")) return "String";
            if (node.Value.StartsWith("#")) return "Date";
            if (int.TryParse(node.Value, out _)) return "Integer";
            if (double.TryParse(node.Value, out _)) return "Double";

            // Expression inference
            if (node.Type == NodeType.Expression)
            {
                // If binary expression, infer from operands
                if (node.Children.Count == 2)
                {
                    string left = InferType(node.Children[0]);
                    string right = InferType(node.Children[1]);
                    return CombineTypes(left, right);
                }
            }

            return "Variant";
        }

        private string CombineTypes(string t1, string t2)
        {
            if (t1 == "String" || t2 == "String") return "String"; // & operator usually
            if (t1 == "Double" || t2 == "Double") return "Double";
            if (t1 == "Single" || t2 == "Single") return "Single";
            if (t1 == "Long" || t2 == "Long") return "Long";
            if (t1 == "Integer" && t2 == "Integer") return "Integer";
            return "Variant";
        }

        private bool IsIncompatible(string target, string source)
        {
            if (target == "Variant") return false;
            if (target == source) return false;
            // Add more compatibility logic (e.g., Integer -> Long is fine, Long -> Integer is risky)
            return false;
        }
    }
}

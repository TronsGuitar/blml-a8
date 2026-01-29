using System;
using System.Collections.Generic;
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
            AnalyzeNode(node);
            return errors;
        }

        private void AnalyzeNode(VB6SyntaxNode node)
        {
            // Check for undefined variables
            if (node.Type == NodeType.Variable && (symbolTable == null || !symbolTable.ContainsKey(node.Value)))
            {
                errors.Add($"Undefined variable: {node.Value}");
            }

            // Check for type compatibility
            if (node.Type == NodeType.Expression)
            {
                // Implement type checking logic here
            }

            foreach (var child in node.Children)
            {
                AnalyzeNode(child);
            }
        }
    }
}

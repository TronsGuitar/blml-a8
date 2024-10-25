using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VB6ToASTParser
{
    public class Token
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public Token(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }

    public class ASTNode
    {
        public string NodeType { get; set; }
        public string Value { get; set; }
        public List<ASTNode> Children { get; set; }

        public ASTNode(string nodeType, string value)
        {
            NodeType = nodeType;
            Value = value;
            Children = new List<ASTNode>();
        }
    }

    public class VB6Parser
    {
        private readonly List<string> _keywords = new List<string> { "If", "Else", "End", "Function", "Sub", "Dim", "For", "Next", "While", "Wend", "Do", "Loop", "Select", "Case" };
        private readonly Dictionary<string, string> _operators = new Dictionary<string, string>
        {
            { "+", "Addition" },
            { "-", "Subtraction" },
            { "*", "Multiplication" },
            { "/", "Division" },
            { "=", "Assignment" },
            { "<", "LessThan" },
            { ">", "GreaterThan" }
        };

        public List<Token> Scan(string vb6Code)
        {
            var tokens = new List<Token>();

            // Regular expression for splitting words
            var regex = new Regex("\b\w+\b|[+\-*/=<>]|");
            var matches = regex.Matches(vb6Code);

            foreach (Match match in matches)
            {
                var value = match.Value;
                if (_keywords.Contains(value))
                {
                    tokens.Add(new Token("Keyword", value));
                }
                else if (_operators.ContainsKey(value))
                {
                    tokens.Add(new Token("Operator", value));
                }
                else if (Regex.IsMatch(value, "\d+"))
                {
                    tokens.Add(new Token("Literal", value));
                }
                else if (Regex.IsMatch(value, "\w+"))
                {
                    tokens.Add(new Token("Identifier", value));
                }
            }

            return tokens;
        }

        public ASTNode Parse(List<Token> tokens)
        {
            // Start the parsing process
            var root = new ASTNode("Program", "Root");
            int index = 0;
            while (index < tokens.Count)
            {
                var statement = ParseStatement(tokens, ref index);
                if (statement != null)
                {
                    root.Children.Add(statement);
                }
            }
            return root;
        }

        private ASTNode ParseStatement(List<Token> tokens, ref int index)
        {
            if (tokens[index].Type == "Keyword")
            {
                switch (tokens[index].Value)
                {
                    case "If":
                        return ParseIfStatement(tokens, ref index);
                    case "Dim":
                        return ParseVariableDeclaration(tokens, ref index);
                    // Add more cases for other statements here.
                }
            }

            return null;
        }

        private ASTNode ParseIfStatement(List<Token> tokens, ref int index)
        {
            var ifNode = new ASTNode("IfStatement", tokens[index].Value);
            index++; // Consume 'If'
            ifNode.Children.Add(ParseExpression(tokens, ref index));
            index++; // Consume 'Then'
            while (index < tokens.Count && tokens[index].Value != "End")
            {
                var childStatement = ParseStatement(tokens, ref index);
                if (childStatement != null)
                {
                    ifNode.Children.Add(childStatement);
                }
            }
            index++; // Consume 'End'
            return ifNode;
        }

        private ASTNode ParseVariableDeclaration(List<Token> tokens, ref int index)
        {
            var varNode = new ASTNode("VariableDeclaration", tokens[index].Value);
            index++; // Consume 'Dim'
            if (tokens[index].Type == "Identifier")
            {
                varNode.Children.Add(new ASTNode("Identifier", tokens[index].Value));
                index++; // Consume identifier
            }
            return varNode;
        }

        private ASTNode ParseExpression(List<Token> tokens, ref int index)
        {
            var exprNode = new ASTNode("Expression", tokens[index].Value);
            index++; // Consume expression
            return exprNode;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var vb6Code = "Dim x If x > 0 Then End";
            var parser = new VB6Parser();

            // Tokenizing VB6 Code
            var tokens = parser.Scan(vb6Code);
            Console.WriteLine("Tokens:");
            foreach (var token in tokens)
            {
                Console.WriteLine($"{token.Type}: {token.Value}");
            }

            // Parsing VB6 Code to AST
            var ast = parser.Parse(tokens);
            Console.WriteLine("\nAbstract Syntax Tree:");
            PrintAST(ast, 0);
        }

        static void PrintAST(ASTNode node, int level)
        {
            Console.WriteLine(new string(' ', level * 2) + $"{node.NodeType}: {node.Value}");
            foreach (var child in node.Children)
            {
                PrintAST(child, level + 1);
            }
        }
    }
}

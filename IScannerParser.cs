using System.Collections.Generic;

namespace VB6ParserLibrary
{
    // Represents a token generated by the scanner.
    public interface IToken
    {
        string Kind { get; }
        string Text { get; }
        int StartIndex { get; }
        int EndIndex { get; }
    }

    // Provides functionality to convert source code into a series of tokens.
    public interface IScanner
    {
        IEnumerable<IToken> Scan(string source);
    }

    // Represents a node within the abstract syntax tree.
    public interface IAstNode
    {
        string NodeType { get; }
        IReadOnlyList<IAstNode> Children { get; }
        IToken? AssociatedToken { get; }
    }

    // Provides methods to parse source code or tokens into an AST.
    public interface IParser
    {
        IAstNode Parse(string source);
        IAstNode Parse(IEnumerable<IToken> tokens);
    }
}

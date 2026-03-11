using BLML.Phase1Foundation.AST;

namespace BLML.Phase1Foundation.SymbolTable
{
    public class SymbolTableBuilder
    {
        private readonly Dictionary<string, VB6SyntaxNode> symbolTable = new Dictionary<string, VB6SyntaxNode>(StringComparer.OrdinalIgnoreCase);

        public static readonly Dictionary<string, object> PredefinedConstants = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            #region Data Type Constants
            { "vbNull", 1 },
            { "vbEmpty", 0 },
            { "vbInteger", 2 },
            { "vbLong", 3 },
            { "vbSingle", 4 },
            { "vbDouble", 5 },
            { "vbCurrency", 6 },
            { "vbDate", 7 },
            { "vbString", 8 },
            { "vbObject", 9 },
            { "vbError", 10 },
            { "vbBoolean", 11 },
            { "vbVariant", 12 },
            { "vbDataObject", 13 },
            { "vbDecimal", 14 },
            { "vbByte", 17 },
            { "vbArray", 8192 },
            #endregion

            #region Date and Time Constants
            { "vbSunday", 1 },
            { "vbMonday", 2 },
            { "vbTuesday", 3 },
            { "vbWednesday", 4 },
            { "vbThursday", 5 },
            { "vbFriday", 6 },
            { "vbSaturday", 7 },
            { "vbUseSystemDayOfWeek", 0 },
            { "vbFirstJan1", 1 },
            { "vbFirstFourDays", 2 },
            { "vbFirstFullWeek", 3 },
            #endregion

            #region String Constants
            { "vbNullChar", '\0' },
            { "vbCr", '\r' },
            { "vbLf", '\n' },
            { "vbCrLf", "\r\n" },
            { "vbTab", '\t' },
            { "vbBack", '\b' },
            { "vbFormFeed", '\f' },
            { "vbVerticalTab", '\v' },
            { "vbNullString", null },
            #endregion

            #region Color Constants
            { "vbBlack", 0x000000 },
            { "vbRed", 0xFF },
            { "vbGreen", 0xFF00 },
            { "vbYellow", 0xFFFF },
            { "vbBlue", 0xFF0000 },
            { "vbMagenta", 0xFF00FF },
            { "vbCyan", 0xFFFF00 },
            { "vbWhite", 0xFFFFFF },
            #endregion

            #region Miscellaneous Constants
            { "vbObjectError", unchecked((int)0x80040000) },
            { "vbTrue", true },
            { "vbFalse", false },
            #endregion

            #region Tristate Constants
            { "vbUseDefault", -2 },
            { "vbTriStateTrue", -1 },
            { "vbTriStateFalse", 0 },
            #endregion

            #region Comparison Constants
            { "vbBinaryCompare", 0 },
            { "vbTextCompare", 1 },
            { "vbDatabaseCompare", 2 },
            #endregion

            #region File I/O Constants
            { "vbNormal", 0 },
            { "vbReadOnly", 1 },
            { "vbHidden", 2 },
            { "vbSystem", 4 },
            { "vbArchive", 32 },
            { "vbAlias", 64 },
            #endregion

            #region Mode Constants
            { "vbFormControlMenu", 0 },
            { "vbModal", 1 },
            { "vbModeless", 0 },
            #endregion
        };

        public Dictionary<string, VB6SyntaxNode> BuildSymbolTable(VB6SyntaxNode node)
        {
            TraverseForSymbols(node);
            return symbolTable;
        }

        private void TraverseForSymbols(VB6SyntaxNode node)
        {
            if (node == null) return;

            switch (node.Type)
            {
                case NodeType.Variable:
                case NodeType.Function:
                case NodeType.Sub:
                case NodeType.Property:
                    symbolTable[node.Value] = node;
                    break;
            }

            // Handle specific declarations
            if (node.Type == NodeType.Variable && node.Attributes.ContainsKey("WithEvents"))
            {
                HandleWithEvents(node);
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    TraverseForSymbols(child);
                }
            }
        }

        public void HandleWithEvents(VB6SyntaxNode node)
        {
            // Transform VB6 WithEvents to C# event handling infrastructure
            // Mark the node as an event source
            if (node.Attributes != null)
            {
                node.Attributes["IsEventSource"] = "true";

                // Logic to register event handlers will be handled by the TypeInference or Translation phase, 
                // checking against this flag or looking up the type's events.
            }
        }

        public void HandleDefaultProperties(VB6SyntaxNode node)
        {
            // Handle VB6 default properties
            // This is primarily for usage analysis.
            // If the node is an identifier used in an expression, we need to check if it refers to a variable
            // that is an object with a default property.

            // This logic requires type information which might not be fully available during symbol table construction.
            // However, we can tag variables that we know are controls/objects.
        }
    }
}

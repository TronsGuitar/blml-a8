using System;
using System.Runtime.InteropServices;
// Example Usage:
IntPtr parserPtr = IntPtr.Zero;
IntPtr languagePtr = IntPtr.Zero;

try
{
    // Get the language pointer from your specific DLL
    languagePtr = NativeMethods.GetLanguage();
    if (languagePtr == IntPtr.Zero)
    {
        Console.WriteLine("Error: Could not load language.");
        return;
    }

    // Create a new parser instance
    parserPtr = NativeMethods.ts_parser_new();
    if (parserPtr == IntPtr.Zero)
    {
        Console.WriteLine("Error: Could not create parser.");
        return;
    }

    // Set the language for the parser
    bool languageSet = NativeMethods.ts_parser_set_language(parserPtr, languagePtr);
    if (!languageSet)
    {
        Console.WriteLine("Error: Could not set parser language.");
        // Potentially indicates an ABI version mismatch between the core
        // library and the language grammar DLL.
        return;
    }

    Console.WriteLine("Successfully loaded language and initialized parser.");

    // --- Now you can use the parser ---
    // Example: Parse a string (requires importing ts_parser_parse_string, etc.)
    // string sourceCode = "your code here";
    // byte sourceBytes = System.Text.Encoding.UTF8.GetBytes(sourceCode);
    // IntPtr treePtr = NativeMethods.ts_parser_parse_string(parserPtr, IntPtr.Zero, sourceBytes, (uint)sourceBytes.Length);
    //... process the tree...
    // NativeMethods.ts_tree_delete(treePtr);

}
finally
{
    // Clean up the parser
    if (parserPtr!= IntPtr.Zero)
    {
        NativeMethods.ts_parser_delete(parserPtr);
    }
    // Note: You generally don't delete the language pointer itself,
    // as it's often a static pointer managed by the DLL.
}

internal static class NativeMethods
{
    // --- Core Tree-sitter functions (assuming tree-sitter.dll is also available) ---

    [DllImport("tree-sitter.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_parser_new();

    [DllImport("tree-sitter.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_parser_set_language(IntPtr parser, IntPtr language);

    // Add other necessary core functions like ts_parser_parse_string, ts_tree_root_node, etc. [7]
    //... see tree_sitter/api.h for the full list [10]

    [DllImport("tree-sitter.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_parser_delete(IntPtr parser);

    // --- Language-specific function from your compiled DLL ---

    // Replace "tree-sitter-mylanguage.dll" with the actual filename of your DLL.
    // Replace "tree_sitter_mylanguage" with the actual exported function name.
    // If the C# method name matches the exported name, EntryPoint is optional.
    [DllImport("tree-sitter-mylanguage.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "tree_sitter_mylanguage")]
    public static extern IntPtr GetLanguage(); // Renamed for clarity in C#
}

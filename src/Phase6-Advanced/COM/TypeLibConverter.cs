using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase6Advanced.COM
{
    /// <summary>
    /// COM/type-library interop conversion (ProjectPlan.md Phase 6 "COM &amp; Interop",
    /// items 138-140). Replaces the file that used to sit here, which was a throwaway
    /// `class Program { static void Main }` sample hardcoded to `C:\Path\To\Your\...`
    /// and built on `System.Runtime.InteropServices.TypeLibConverter` /
    /// `ITypeLibImporterNotifySink` - APIs that only ever existed in .NET Framework and
    /// are not available on .NET 8 at all, so that file could never have compiled once
    /// re-included in the build.
    ///
    /// Type-library import itself (item 138) still needs the classic Windows SDK
    /// `tlbimp.exe` - there is no managed .NET 8 API replacement - so
    /// <see cref="GenerateInteropAssembly"/> shells out to it via
    /// <see cref="LibraryInspector"/> (already implemented, and already .NET-8-safe
    /// since it only uses reflection and Process, not the removed TypeLibConverter
    /// API). What was missing was anything wiring that into VB6 code conversion:
    /// <see cref="ConvertCreateObjectCall"/> handles the late-bound path (item 139),
    /// which needs no interop assembly at all.
    /// </summary>
    public class TypeLibConverter
    {
        private readonly LibraryInspector _inspector = new();

        /// <summary>
        /// Generates (or loads a previously-generated) .NET interop assembly for a
        /// referenced .tlb/.ocx/.dll and returns the types/properties/methods it
        /// exposes, for use in early-bound conversion or reference documentation.
        /// </summary>
        public ReferencedLibrary GenerateInteropAssembly(string typeLibOrOcxOrDllPath)
        {
            _inspector.InspectLibrary(typeLibOrOcxOrDllPath);
            return _inspector.ReferencedLibrary;
        }

        /// <summary>
        /// Converts `CreateObject("Excel.Application")` to the late-bound C# equivalent
        /// that needs no interop assembly: `Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"))`.
        /// This is the safe default conversion target - it works for any registered
        /// COM ProgID without first running tlbimp - at the cost of losing
        /// compile-time member checking (VB6's own `CreateObject` has exactly the same
        /// late-bound tradeoff, so this preserves the original code's actual behavior
        /// rather than silently upgrading it to something stricter).
        /// </summary>
        public ExpressionSyntax ConvertCreateObjectCall(string progId)
        {
            var progIdLiteral = SyntaxFactory.Literal(progId);
            return SyntaxFactory.ParseExpression(
                $"System.Activator.CreateInstance(System.Type.GetTypeFromProgID({progIdLiteral.ToString()}))");
        }

        /// <summary>
        /// Detects a `CreateObject("ProgId")` invocation shape without depending on the
        /// specific AST types the main VB6 pipeline uses, so callers can pattern-match
        /// on their own expression tree and only invoke <see cref="ConvertCreateObjectCall"/>
        /// once they've confirmed the shape.
        /// </summary>
        public static bool IsCreateObjectCall(string functionName) =>
            string.Equals(functionName, "CreateObject", System.StringComparison.OrdinalIgnoreCase);
    }
}

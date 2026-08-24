using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase6Advanced.LateBinding
{
    /// <summary>
    /// Converts VB6 late binding - a `Variant`/`Object`-typed variable (typically from
    /// `CreateObject`, or simply never given an `As` type) whose members are resolved
    /// at runtime rather than compile time - to C#'s `dynamic`, the direct equivalent
    /// with the same late-bound-member tradeoff. This is deliberately narrow: it
    /// converts the *type*, not individual member-access call sites, since a bare
    /// `obj.Foo()` needs no syntactic change at all once `obj` is declared `dynamic` -
    /// C# resolves it at runtime automatically, same as VB6 does.
    /// </summary>
    public class DynamicConverter
    {
        private static readonly HashSet<string> LateBoundVb6Types = new(StringComparer.OrdinalIgnoreCase) { "Variant", "Object" };

        /// <summary>True when a VB6 type declaration (`As Variant`, `As Object`, or no `As` clause at all) should become C# `dynamic` rather than a concrete type.</summary>
        public bool ShouldUseDynamic(string? vb6Type) =>
            string.IsNullOrWhiteSpace(vb6Type) || LateBoundVb6Types.Contains(vb6Type);

        public TypeSyntax ConvertToDynamicType() => SyntaxFactory.IdentifierName("dynamic");
    }
}

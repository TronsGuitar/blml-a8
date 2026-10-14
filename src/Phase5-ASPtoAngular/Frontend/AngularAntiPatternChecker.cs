using System.Text.RegularExpressions;

namespace BLML.Phase5ASPtoAngular.Frontend
{
    public enum AntiPatternSeverity { Error, Warning }

    public class AntiPatternFinding
    {
        public string Rule { get; set; } = string.Empty;
        public AntiPatternSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lints generated Angular output against the modern (Angular 17+) conventions
    /// this generator targets - standalone components, `@if`/`@for`/`@switch` instead
    /// of `*ngIf`/`*ngFor`/`*ngSwitch`, `inject()` over constructor injection, typed
    /// Reactive Forms over `[(ngModel)]`, and no untracked `.subscribe()` or `any`.
    ///
    /// This runs on EVERY component ComponentGenerator produces (see its unit tests) -
    /// the point is a build-time guarantee that the generator's own output never
    /// regresses into the patterns it was written specifically to avoid, not a general
    /// purpose linter for hand-written Angular code.
    /// </summary>
    public class AngularAntiPatternChecker
    {
        private static readonly Regex LegacyStructuralDirective = new(@"\*ng(If|For|Switch)\b", RegexOptions.Compiled);
        private static readonly Regex ForBlockStart = new(@"@for\s*\(", RegexOptions.Compiled);
        private static readonly Regex NgModelBinding = new(@"\[\(ngModel\)\]", RegexOptions.Compiled);
        private static readonly Regex AnyType = new(@":\s*any\b", RegexOptions.Compiled);
        private static readonly Regex Subscribe = new(@"\.subscribe\s*\(", RegexOptions.Compiled);
        private static readonly Regex TakeUntilDestroyedOrAsyncPipe = new(@"takeUntilDestroyed|async\s*pipe|\|\s*async", RegexOptions.Compiled);
        private static readonly Regex ComponentDecorator = new(@"@Component\s*\(\s*\{", RegexOptions.Compiled);
        private static readonly Regex StandaloneTrue = new(@"standalone\s*:\s*true", RegexOptions.Compiled);
        private static readonly Regex NgModule = new(@"@NgModule\b", RegexOptions.Compiled);
        private static readonly Regex ConstructorInjection = new(@"constructor\s*\(\s*(private|public|protected|readonly)\b", RegexOptions.Compiled);
        private static readonly Regex InjectFunction = new(@"\binject\s*\(", RegexOptions.Compiled);
        private static readonly Regex DirectDomAccess = new(@"\bdocument\.|ElementRef\b", RegexOptions.Compiled);

        public List<AntiPatternFinding> CheckTemplate(string html)
        {
            var findings = new List<AntiPatternFinding>();

            foreach (Match m in LegacyStructuralDirective.Matches(html))
            {
                findings.Add(new AntiPatternFinding
                {
                    Rule = "no-legacy-structural-directives",
                    Severity = AntiPatternSeverity.Error,
                    Message = $"Found legacy structural directive '*ng{m.Groups[1].Value}' - use the built-in @if/@for/@switch control flow instead."
                });
            }

            foreach (Match m in ForBlockStart.Matches(html))
            {
                var header = ExtractBalancedParenContent(html, m.Index + m.Length - 1);
                if (header != null && !header.Contains("track"))
                {
                    findings.Add(new AntiPatternFinding
                    {
                        Rule = "for-requires-track",
                        Severity = AntiPatternSeverity.Error,
                        Message = $"@for block '@for ({header})' has no track expression - required for correct change detection."
                    });
                }
            }

            if (NgModelBinding.IsMatch(html))
            {
                findings.Add(new AntiPatternFinding
                {
                    Rule = "no-ngmodel-two-way-binding",
                    Severity = AntiPatternSeverity.Warning,
                    Message = "Found [(ngModel)] two-way binding - prefer a typed Reactive Form (FormGroup/FormControl) for form input."
                });
            }

            return findings;
        }

        /// <summary>
        /// Given the index of an opening '(', returns the text between it and its
        /// matching ')' (paren depth tracked, so a nested call like `rsItems()` inside
        /// the @for header doesn't fool a naive "up to the first )" scan). Returns null
        /// if the parens are unbalanced.
        /// </summary>
        private static string? ExtractBalancedParenContent(string text, int openParenIndex)
        {
            int depth = 0;
            for (int i = openParenIndex; i < text.Length; i++)
            {
                if (text[i] == '(') depth++;
                else if (text[i] == ')')
                {
                    depth--;
                    if (depth == 0) return text.Substring(openParenIndex + 1, i - openParenIndex - 1);
                }
            }
            return null;
        }

        public List<AntiPatternFinding> CheckComponent(string typescript)
        {
            var findings = new List<AntiPatternFinding>();

            if (NgModule.IsMatch(typescript))
            {
                findings.Add(new AntiPatternFinding
                {
                    Rule = "no-ngmodule",
                    Severity = AntiPatternSeverity.Error,
                    Message = "Found @NgModule - this generator targets standalone components only, with no module wiring."
                });
            }

            if (ComponentDecorator.IsMatch(typescript) && !StandaloneTrue.IsMatch(typescript))
            {
                findings.Add(new AntiPatternFinding
                {
                    Rule = "component-must-be-standalone",
                    Severity = AntiPatternSeverity.Error,
                    Message = "@Component is missing 'standalone: true'."
                });
            }

            if (AnyType.IsMatch(typescript))
            {
                findings.Add(new AntiPatternFinding
                {
                    Rule = "no-any-type",
                    Severity = AntiPatternSeverity.Warning,
                    Message = "Found ': any' - use a specific type (the generated DTO, a primitive, or a named interface)."
                });
            }

            if (Subscribe.IsMatch(typescript) && !TakeUntilDestroyedOrAsyncPipe.IsMatch(typescript))
            {
                findings.Add(new AntiPatternFinding
                {
                    Rule = "no-unmanaged-subscribe",
                    Severity = AntiPatternSeverity.Warning,
                    Message = "Found .subscribe() with no visible takeUntilDestroyed()/async pipe - this leaks the subscription. Prefer toSignal()/the async pipe, or add takeUntilDestroyed()."
                });
            }

            if (ConstructorInjection.IsMatch(typescript) && !InjectFunction.IsMatch(typescript))
            {
                findings.Add(new AntiPatternFinding
                {
                    Rule = "prefer-inject-function",
                    Severity = AntiPatternSeverity.Warning,
                    Message = "Found constructor-parameter dependency injection - prefer the inject() function, this generator's house style."
                });
            }

            if (DirectDomAccess.IsMatch(typescript))
            {
                findings.Add(new AntiPatternFinding
                {
                    Rule = "no-direct-dom-access",
                    Severity = AntiPatternSeverity.Warning,
                    Message = "Found direct DOM access (document./ElementRef) - prefer template bindings and signals."
                });
            }

            return findings;
        }
    }
}

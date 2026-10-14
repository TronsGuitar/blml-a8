using System.Text;

namespace BLML.Phase5ASPtoAngular.Frontend
{
    public class GeneratedComponent
    {
        public string TypeScript { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public string Spec { get; set; } = string.Empty;
        public List<AntiPatternFinding> Findings { get; } = new();
    }

    /// <summary>
    /// Emits a standalone Angular component (no NgModule) in the modern Angular 17+
    /// house style this whole generator targets: `inject()` for DI (never
    /// constructor-parameter injection), `toSignal()` over an HttpClient call for
    /// server-derived list state (never a raw `.subscribe()`), and the template
    /// TemplateConverter already renders with `@if`/`@for`. Every component this
    /// produces is run back through AngularAntiPatternChecker before being returned,
    /// so a regression here fails loudly in ComponentGenerator's own tests instead of
    /// shipping.
    /// </summary>
    public class ComponentGenerator
    {
        private readonly AngularAntiPatternChecker _checker = new();

        public GeneratedComponent GenerateComponent(
            string componentClassName,
            string selector,
            string apiResourcePath,
            string dtoInterfaceName,
            string templateHtml,
            IReadOnlyList<string>? recordsetSignalNames = null)
        {
            recordsetSignalNames ??= Array.Empty<string>();

            var ts = new StringBuilder();
            ts.AppendLine("import { Component, inject } from '@angular/core';");
            ts.AppendLine("import { HttpClient } from '@angular/common/http';");
            ts.AppendLine("import { toSignal } from '@angular/core/rxjs-interop';");
            ts.AppendLine($"import {{ {dtoInterfaceName} }} from './{ToKebabCase(dtoInterfaceName)}.model';");
            ts.AppendLine();
            ts.AppendLine("@Component({");
            ts.AppendLine($"  selector: '{selector}',");
            ts.AppendLine("  standalone: true,");
            ts.AppendLine($"  templateUrl: './{ToKebabCase(componentClassName)}.component.html'");
            ts.AppendLine("})");
            ts.AppendLine($"export class {componentClassName} {{");
            ts.AppendLine("  private readonly http = inject(HttpClient);");
            ts.AppendLine();
            foreach (var signalName in recordsetSignalNames)
            {
                ts.AppendLine($"  readonly {signalName} = toSignal(");
                ts.AppendLine($"    this.http.get<{dtoInterfaceName}[]>('{apiResourcePath}'),");
                ts.AppendLine($"    {{ initialValue: [] as {dtoInterfaceName}[] }}");
                ts.AppendLine("  );");
                ts.AppendLine();
            }
            ts.AppendLine("}");

            var spec = new StringBuilder();
            spec.AppendLine("import { TestBed } from '@angular/core/testing';");
            spec.AppendLine("import { provideHttpClientTesting } from '@angular/common/http/testing';");
            spec.AppendLine("import { provideHttpClient } from '@angular/common/http';");
            spec.AppendLine($"import {{ {componentClassName} }} from './{ToKebabCase(componentClassName)}.component';");
            spec.AppendLine();
            spec.AppendLine($"describe('{componentClassName}', () => {{");
            spec.AppendLine("  beforeEach(() => TestBed.configureTestingModule({");
            spec.AppendLine($"    imports: [{componentClassName}],");
            spec.AppendLine("    providers: [provideHttpClient(), provideHttpClientTesting()]");
            spec.AppendLine("  }));");
            spec.AppendLine();
            spec.AppendLine("  it('should create', () => {");
            spec.AppendLine($"    const fixture = TestBed.createComponent({componentClassName});");
            spec.AppendLine("    expect(fixture.componentInstance).toBeTruthy();");
            spec.AppendLine("  });");
            spec.AppendLine("});");

            var result = new GeneratedComponent { TypeScript = ts.ToString(), Html = templateHtml, Spec = spec.ToString() };
            result.Findings.AddRange(_checker.CheckComponent(result.TypeScript));
            result.Findings.AddRange(_checker.CheckTemplate(result.Html));
            return result;
        }

        public string GenerateDtoInterface(string interfaceName, IReadOnlyList<string> fieldNames)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"export interface {interfaceName} {{");
            foreach (var field in fieldNames)
            {
                sb.AppendLine($"  {AspExpressionToTypeScript.ToCamelCase(field)}: unknown; // TODO: verify type - inferred from ASP `rs(\"{field}\")` usage, no schema was available.");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string ToKebabCase(string pascalOrCamel)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < pascalOrCamel.Length; i++)
            {
                var c = pascalOrCamel[i];
                if (char.IsUpper(c) && i > 0) sb.Append('-');
                sb.Append(char.ToLowerInvariant(c));
            }
            var result = sb.ToString();
            const string suffix = "-component";
            return result.EndsWith(suffix) ? result[..^suffix.Length] : result;
        }
    }
}

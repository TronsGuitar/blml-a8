using System.Text.RegularExpressions;
using BLML.Phase1Foundation.AST;
using BLML.Phase4DataAccess.Models;
using BLML.Phase5ASPtoAngular.Analysis;
using BLML.Phase5ASPtoAngular.AspParser;
using BLML.Phase5ASPtoAngular.Backend.ApiGenerator;
using BLML.Phase5ASPtoAngular.Backend.Infrastructure;
using BLML.Phase5ASPtoAngular.Database;
using BLML.Phase5ASPtoAngular.Frontend;

namespace BLML.Phase5ASPtoAngular
{
    /// <summary>
    /// Top-level orchestrator that runs every Phase 5 piece end-to-end over a folder
    /// of classic ASP pages: parse -> analyze -> generate .NET 8 Web API (Api/) +
    /// standalone Angular app (ClientApp/) + SQL Server migration scripts (Database/).
    /// This is the Phase 5 equivalent of what CommandLineInterface's "convert-project"
    /// does for Phases 1-4 - the thing that makes the individual generators a working
    /// pipeline instead of a set of independently-testable but disconnected classes.
    ///
    /// Scoped to the common case: one representative database query per page (the
    /// typical classic-ASP list/detail page shape). A page with multiple distinct
    /// queries only gets its first one converted; everything else it does is still
    /// parsed, analyzed, and rendered into the template, just not wired to a generated
    /// API method.
    /// </summary>
    public class AspProjectConverter
    {
        public class ConversionResult
        {
            public List<string> GeneratedFiles { get; } = new();
            public List<string> Warnings { get; } = new();
        }

        private readonly AspParser.AspParser _aspParser = new();
        private readonly DatabaseCallAnalyzer _dbAnalyzer = new();
        private readonly SessionVariableTracker _sessionTracker = new();
        private readonly PageFlowAnalyzer _pageFlowAnalyzer = new();
        private readonly DtoGenerator _dtoGenerator = new();
        private readonly ServiceGenerator _serviceGenerator = new();
        private readonly ControllerGenerator _controllerGenerator = new();
        private readonly AuthConverter _authConverter = new();
        private readonly MiddlewareGenerator _middlewareGenerator = new();
        private readonly EFCoreGenerator _efCoreGenerator = new();
        private readonly MigrationScripts _migrationScripts = new();
        private readonly TemplateConverter _templateConverter = new();
        private readonly ComponentGenerator _componentGenerator = new();
        private readonly RoutingGenerator _routingGenerator = new();

        public ConversionResult ConvertDirectory(string inputDir, string outputDir)
        {
            var result = new ConversionResult();
            var apiDir = Path.Combine(outputDir, "Api");
            var clientDir = Path.Combine(outputDir, "ClientApp", "src", "app");
            var dbDir = Path.Combine(outputDir, "Database");

            var aspFiles = Directory.GetFiles(inputDir, "*.asp", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToList();
            var serviceClassNames = new List<string>();
            var allFlowEdges = new List<PageFlowEdge>();
            var allSessionKeys = new Dictionary<string, SessionVariableInfo>(StringComparer.OrdinalIgnoreCase);
            var tables = new List<TableMetadata>();

            foreach (var filePath in aspFiles)
            {
                var pageName = Path.GetFileName(filePath);
                var resourceName = ToResourceName(pageName);
                var content = File.ReadAllText(filePath);
                var page = _aspParser.Parse(content, filePath, inputDir);
                result.Warnings.AddRange(page.ParseWarnings.Select(w => $"{pageName}: {w}"));

                allFlowEdges.AddRange(_pageFlowAnalyzer.Analyze(page.Statements, pageName));
                foreach (var kv in _sessionTracker.Catalog(page.Statements))
                {
                    if (!allSessionKeys.ContainsKey(kv.Key)) allSessionKeys[kv.Key] = kv.Value;
                }

                var recordsetSignalNames = new List<string>();
                var adoObjects = _dbAnalyzer.Analyze(page.Statements);
                var primaryQuery = adoObjects.SelectMany(o => o.CallSites.Select(s => (obj: o, site: s))).FirstOrDefault();

                if (primaryQuery.site != null)
                {
                    var fields = _dbAnalyzer.FindFieldReferences(page.Statements, primaryQuery.obj.VariableName);
                    var dtoName = resourceName + "Dto";
                    var table = _efCoreGenerator.BuildTableMetadata(
                        primaryQuery.site.TablesReferenced.FirstOrDefault() ?? resourceName, fields);
                    tables.Add(table);

                    WriteFile(result, apiDir, "Dtos", $"{dtoName}.cs", _dtoGenerator.GenerateDto(dtoName, fields));

                    var (verb, isMutation) = ControllerGenerator.DeriveVerbFromSql(primaryQuery.site.SqlText);
                    var serviceMethodName = (isMutation ? verb[..1] + verb[1..].ToLowerInvariant() : "Get") + resourceName;
                    var serviceClassName = resourceName + "Service";
                    serviceClassNames.Add(serviceClassName);

                    var methodSpec = new ServiceMethodSpec { MethodName = serviceMethodName, Site = primaryQuery.site, ResultFields = fields, IsMutation = isMutation };
                    WriteFile(result, apiDir, "Services", $"{serviceClassName}.cs",
                        _serviceGenerator.GenerateServiceClass(serviceClassName, dtoName, new[] { methodSpec }));

                    var action = new ControllerActionSpec { MethodName = serviceMethodName, ServiceMethodName = serviceMethodName, HttpVerb = verb };
                    var controllerClassName = resourceName + "Controller";
                    WriteFile(result, apiDir, "Controllers", $"{controllerClassName}.cs",
                        _controllerGenerator.GenerateController(resourceName.ToLowerInvariant(), controllerClassName, serviceClassName, dtoName, new[] { action }));

                    // matches TemplateConverter's own "{camelRs}Items" naming convention for the recordset signal it references
                    recordsetSignalNames.Add(AspExpressionToTypeScript.ToCamelCase(primaryQuery.obj.VariableName) + "Items");
                }

                var templateHtml = _templateConverter.Convert(page.Statements);
                result.Warnings.AddRange(_templateConverter.Warnings.Select(w => $"{pageName}: {w}"));

                var componentClassName = resourceName + "Component";
                var kebabName = ComponentGenerator.ToKebabCase(resourceName);
                var dtoInterfaceName = resourceName + "Dto";
                var generatedComponent = _componentGenerator.GenerateComponent(
                    componentClassName, $"app-{kebabName}", $"/api/{resourceName.ToLowerInvariant()}", dtoInterfaceName, templateHtml, recordsetSignalNames);

                if (generatedComponent.Findings.Count > 0)
                {
                    result.Warnings.AddRange(generatedComponent.Findings.Select(f => $"{pageName}: [{f.Rule}] {f.Message}"));
                }

                WriteFile(result, clientDir, kebabName, $"{kebabName}.component.ts", generatedComponent.TypeScript);
                WriteFile(result, clientDir, kebabName, $"{kebabName}.component.html", generatedComponent.Html);
                WriteFile(result, clientDir, kebabName, $"{kebabName}.component.spec.ts", generatedComponent.Spec);
                if (primaryQuery.site != null)
                {
                    var fields = _dbAnalyzer.FindFieldReferences(page.Statements, primaryQuery.obj.VariableName);
                    WriteFile(result, clientDir, kebabName, $"{ComponentGenerator.ToKebabCase(dtoInterfaceName)}.model.ts",
                        _componentGenerator.GenerateDtoInterface(dtoInterfaceName, fields));
                }
            }

            if (aspFiles.Count > 0)
            {
                var homePage = Path.GetFileName(aspFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals("index", StringComparison.OrdinalIgnoreCase)) ?? aspFiles[0]);
                WriteFile(result, clientDir, "", "app.routes.ts", _routingGenerator.GenerateRoutes(allFlowEdges, homePage));
            }

            if (tables.Count > 0)
            {
                WriteFile(result, dbDir, "", "schema.sql", _migrationScripts.GenerateCreateScripts(tables));
                WriteFile(result, dbDir, "", "bulk-copy.cs", _migrationScripts.GenerateBulkCopyScripts(tables));
                WriteFile(result, apiDir, "Models", "Entities.cs", _efCoreGenerator.GenerateEntities(tables));
                WriteFile(result, apiDir, "Data", "ApiDbContext.cs", _efCoreGenerator.GenerateDbContext("ApiDbContext", tables));
            }

            var identityKeys = allSessionKeys.Keys.Where(k => k.Contains("user", StringComparison.OrdinalIgnoreCase) || k.Contains("role", StringComparison.OrdinalIgnoreCase)).ToList();
            if (identityKeys.Count > 0)
            {
                WriteFile(result, apiDir, "Services", "AuthService.cs", _authConverter.GenerateAuthService(identityKeys));
            }

            if (serviceClassNames.Count > 0)
            {
                WriteFile(result, apiDir, "", "Program.cs", _middlewareGenerator.GenerateProgramCs(serviceClassNames, "http://localhost:4200"));
            }

            var globalAsaPath = Path.Combine(inputDir, "Global.asa");
            if (File.Exists(globalAsaPath))
            {
                var globalAsa = new GlobalAsaParser().Parse(File.ReadAllText(globalAsaPath));
                result.Warnings.AddRange(globalAsa.Warnings.Select(w => $"Global.asa: {w}"));
            }

            return result;
        }

        private static void WriteFile(ConversionResult result, string baseDir, string subDir, string fileName, string content)
        {
            var dir = string.IsNullOrEmpty(subDir) ? baseDir : Path.Combine(baseDir, subDir);
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, fileName);
            File.WriteAllText(path, content);
            result.GeneratedFiles.Add(path);
        }

        private static string ToResourceName(string aspFileName)
        {
            var name = aspFileName.EndsWith(".asp", StringComparison.OrdinalIgnoreCase) ? aspFileName[..^4] : aspFileName;
            var cleaned = Regex.Replace(name, "[^A-Za-z0-9]", " ");
            var parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? "Page" : string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
        }
    }
}

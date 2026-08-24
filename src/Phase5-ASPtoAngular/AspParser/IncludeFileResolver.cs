namespace BLML.Phase5ASPtoAngular.AspParser
{
    /// <summary>
    /// Resolves `&lt;!--#include file="x.asp"--&gt;` / `virtual="/x.asp"` directives.
    /// `file=` is relative to the including file's own directory (and can use `../`);
    /// `virtual=` is relative to the site's application root. Detects circular includes
    /// rather than recursing forever, since a real classic-ASP codebase (e.g. a shared
    /// header/footer pair that both pull in a common nav file) can easily form a cycle
    /// if someone slips up.
    /// </summary>
    public class IncludeFileResolver
    {
        private readonly string _applicationRoot;

        public IncludeFileResolver(string applicationRoot)
        {
            _applicationRoot = applicationRoot;
        }

        public class ResolveResult
        {
            public string Content { get; set; } = string.Empty;
            public List<string> ResolvedFiles { get; } = new();
            public List<string> Warnings { get; } = new();
        }

        public ResolveResult ResolveIncludes(string content, string currentFilePath)
        {
            var result = new ResolveResult();
            var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { NormalizePath(currentFilePath) };
            result.Content = ResolveRecursive(content, currentFilePath, visiting, result);
            return result;
        }

        private string ResolveRecursive(string content, string currentFilePath, HashSet<string> visiting, ResolveResult result)
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize(content);
            var includeRegions = regions.Where(r => r.Type == AspRegionType.Include).ToList();
            if (includeRegions.Count == 0) return content;

            var sb = new System.Text.StringBuilder();
            foreach (var region in regions)
            {
                if (region.Type != AspRegionType.Include)
                {
                    sb.Append(RegionToText(region));
                    continue;
                }

                var resolvedPath = ResolvePath(region, currentFilePath);
                if (resolvedPath is null || !File.Exists(resolvedPath))
                {
                    result.Warnings.Add($"Could not resolve include '{region.IncludePath}' referenced from '{currentFilePath}'.");
                    continue;
                }

                var normalized = NormalizePath(resolvedPath);
                if (visiting.Contains(normalized))
                {
                    result.Warnings.Add($"Circular include detected: '{region.IncludePath}' referenced from '{currentFilePath}'.");
                    continue;
                }

                var includedContent = File.ReadAllText(resolvedPath);
                result.ResolvedFiles.Add(resolvedPath);
                visiting.Add(normalized);
                sb.Append(ResolveRecursive(includedContent, resolvedPath, visiting, result));
                visiting.Remove(normalized);
            }

            return sb.ToString();
        }

        private string? ResolvePath(AspRegion region, string currentFilePath)
        {
            if (string.IsNullOrEmpty(region.IncludePath)) return null;

            if (region.IncludeIsVirtual)
            {
                var relative = region.IncludePath.TrimStart('/', '\\');
                return Path.Combine(_applicationRoot, relative);
            }

            var baseDir = Path.GetDirectoryName(currentFilePath) ?? _applicationRoot;
            return Path.GetFullPath(Path.Combine(baseDir, region.IncludePath));
        }

        private static string NormalizePath(string path) => Path.GetFullPath(path).TrimEnd('\\', '/').ToLowerInvariant();

        /// <summary>Reconstructs the original markup for a non-include region so re-splicing doesn't lose text.</summary>
        private static string RegionToText(AspRegion region) => region.Type switch
        {
            AspRegionType.Html => region.Text,
            AspRegionType.OutputExpression => $"<%={region.Text}%>",
            AspRegionType.Directive => $"<%@{region.Text}%>",
            AspRegionType.ServerComment => $"<%--{region.Text}--%>",
            AspRegionType.CodeBlock => $"<%{region.Text}%>",
            _ => region.Text
        };
    }
}

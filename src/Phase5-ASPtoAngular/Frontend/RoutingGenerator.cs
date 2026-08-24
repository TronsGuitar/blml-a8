using System.Text;
using BLML.Phase5ASPtoAngular.Analysis;

namespace BLML.Phase5ASPtoAngular.Frontend
{
    /// <summary>
    /// Turns PageFlowAnalyzer's page graph into a standalone Angular route table
    /// (`provideRouter(routes)`, lazy `loadComponent` per route - no NgModule-based
    /// lazy loading, matching this generator's standalone-only house style). One ASP
    /// page name is treated as the home page and mapped to the empty path with the
    /// rest redirecting to it as a catch-all.
    /// </summary>
    public class RoutingGenerator
    {
        public string GenerateRoutes(IEnumerable<PageFlowEdge> edges, string homePageName)
        {
            var pages = new List<string> { homePageName };
            foreach (var edge in edges)
            {
                if (!pages.Contains(edge.FromPage, StringComparer.OrdinalIgnoreCase)) pages.Add(edge.FromPage);
                if (!pages.Contains(edge.ToPage, StringComparer.OrdinalIgnoreCase)) pages.Add(edge.ToPage);
            }

            var sb = new StringBuilder();
            sb.AppendLine("import { Routes } from '@angular/router';");
            sb.AppendLine();
            sb.AppendLine("export const routes: Routes = [");
            foreach (var page in pages)
            {
                var folder = ToRouteSegment(page);
                var className = ToComponentClassName(page);
                var path = string.Equals(page, homePageName, StringComparison.OrdinalIgnoreCase) ? "" : folder;
                sb.AppendLine($"  {{ path: '{path}', loadComponent: () => import('./{folder}/{folder}.component').then(m => m.{className}) }},");
            }
            sb.AppendLine("  { path: '**', redirectTo: '' }");
            sb.AppendLine("];");
            return sb.ToString();
        }

        private static string ToRouteSegment(string pageName)
        {
            var name = pageName.EndsWith(".asp", StringComparison.OrdinalIgnoreCase) ? pageName[..^4] : pageName;
            return ComponentGenerator.ToKebabCase(name);
        }

        private static string ToComponentClassName(string pageName)
        {
            var segment = ToRouteSegment(pageName);
            var parts = segment.Split('-', StringSplitOptions.RemoveEmptyEntries);
            var pascal = string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
            return pascal + "Component";
        }
    }
}

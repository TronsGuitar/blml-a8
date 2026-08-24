using System;
using System.Text;
using System.Xml;
using BLML.Phase1Foundation.ProjectModel;

namespace BLML.Phase1Foundation.ProjectModel
{
    public class CsprojGenerator
    {
        public string GenerateProjectFile(VB6Project vb6Project)
        {
            var sb = new StringBuilder();
            
            // Assume .NET 8.0 Windows Application if forms are present
            bool hasForms = vb6Project.Forms.Count > 0 || vb6Project.UserControls.Count > 0;
            string outputType = vb6Project.Type?.ToLowerInvariant() == "oleexe" || vb6Project.Type?.ToLowerInvariant() == "exe" 
                ? "WinExe" 
                : "Library";
                
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            sb.AppendLine("");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine($"    <OutputType>{outputType}</OutputType>");
            sb.AppendLine($"    <TargetFramework>{(hasForms ? "net8.0-windows" : "net8.0")}</TargetFramework>");
            sb.AppendLine("    <LangVersion>12</LangVersion>");
            sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
            sb.AppendLine("    <Nullable>enable</Nullable>");
            
            if (hasForms)
            {
                sb.AppendLine("    <UseWindowsForms>true</UseWindowsForms>");
            }
            if (!string.IsNullOrEmpty(vb6Project.Name))
            {
                sb.AppendLine($"    <AssemblyName>{vb6Project.Name}</AssemblyName>");
                sb.AppendLine($"    <RootNamespace>{vb6Project.Name}</RootNamespace>");
            }
            
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine("");
            
            // Packages / References mapping (Basic COM references)
            if (vb6Project.References.Count > 0 || vb6Project.Objects.Count > 0)
            {
                sb.AppendLine("  <ItemGroup>");
                // This would map VB6 Guids to specific NuGet packages or COM references.
                // For now, emit them as comments or generic COM references to be verified by a human.
                foreach (var r in vb6Project.References)
                {
                    sb.AppendLine($"    <!-- VB6 Reference: {r.Description} (GUID: {r.Guid}) -->");
                }
                foreach (var obj in vb6Project.Objects)
                {
                    sb.AppendLine($"    <!-- VB6 Object: {obj.Name} (GUID: {obj.Guid}) -->");
                }
                sb.AppendLine("  </ItemGroup>");
            }
            sb.AppendLine("</Project>");

            return sb.ToString();
        }
    }
}

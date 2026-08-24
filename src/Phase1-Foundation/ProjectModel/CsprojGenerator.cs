using System.Text;

namespace BLML.Phase1Foundation.ProjectModel
{
    public class CsprojGenerator
    {
        public string GenerateProjectFile(VB6Project project)
        {
            var projectName = string.IsNullOrWhiteSpace(project?.Name) ? "ConvertedProject" : project.Name;
            var useWindowsForms = project is not null && (project.Forms.Count > 0 || project.UserControls.Count > 0);

            var builder = new StringBuilder();
            builder.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            builder.AppendLine("  <PropertyGroup>");
            builder.AppendLine("    <TargetFramework>net8.0</TargetFramework>");
            builder.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
            builder.AppendLine("    <Nullable>enable</Nullable>");
            builder.AppendLine($"    <RootNamespace>{projectName}</RootNamespace>");

            if (useWindowsForms)
            {
                builder.AppendLine("    <UseWindowsForms>true</UseWindowsForms>");
            }

            builder.AppendLine("  </PropertyGroup>");
            builder.AppendLine("</Project>");
            return builder.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BLML.Phase1Foundation.ProjectModel
{
    public class SolutionGenerator
    {
        public string GenerateSolution(string solutionName, Dictionary<string, string> projectPaths)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("");
            sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            sb.AppendLine("# Visual Studio Version 17");
            sb.AppendLine("VisualStudioVersion = 17.0.31903.59");
            sb.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
            
            foreach (var project in projectPaths)
            {
                string projectName = project.Key;
                string projectPath = project.Value;
                // Generate deterministic GUID based on project name for the .sln structure
                string projectGuid = GenerateDeterministicGuid(projectName).ToString("B").ToUpperInvariant();
                string csharpProjectTypeGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
                
                sb.AppendLine($"Project(\"{csharpProjectTypeGuid}\") = \"{projectName}\", \"{projectPath}\", \"{projectGuid}\"");
                sb.AppendLine("EndProject");
            }

            sb.AppendLine("Global");
            sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            sb.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
            sb.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
            sb.AppendLine("\tEndGlobalSection");
            
            sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var project in projectPaths)
            {
                string projectGuid = GenerateDeterministicGuid(project.Key).ToString("B").ToUpperInvariant();
                sb.AppendLine($"\t\t{projectGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                sb.AppendLine($"\t\t{projectGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                sb.AppendLine($"\t\t{projectGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                sb.AppendLine($"\t\t{projectGuid}.Release|Any CPU.Build.0 = Release|Any CPU");
            }
            sb.AppendLine("\tEndGlobalSection");
            
            sb.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
            sb.AppendLine("\t\tHideSolutionNode = FALSE");
            sb.AppendLine("\tEndGlobalSection");
            sb.AppendLine("EndGlobal");
            
            return sb.ToString();
        }
        
        private Guid GenerateDeterministicGuid(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return new Guid(hash);
            }
        }
    }
}

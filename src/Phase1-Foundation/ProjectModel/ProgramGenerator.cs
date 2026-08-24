using System;
using System.Text;

namespace BLML.Phase1Foundation.ProjectModel
{
    /// <summary>
    /// Generates the Program.cs entry point for converted VB6 executable projects
    /// (Type=Exe / Type=OleExe), bootstrapping either the startup form or the
    /// module that declared "Sub Main".
    /// </summary>
    public class ProgramGenerator
    {
        public string GenerateProgramFile(VB6Project project, bool hasForms, string startupFormName, string subMainModuleName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            if (hasForms)
            {
                sb.AppendLine("using System.Windows.Forms;");
            }
            sb.AppendLine();
            sb.AppendLine("internal static class Program");
            sb.AppendLine("{");
            if (hasForms)
            {
                sb.AppendLine("    [STAThread]");
            }
            sb.AppendLine("    static void Main()");
            sb.AppendLine("    {");

            if (!string.IsNullOrEmpty(subMainModuleName))
            {
                sb.AppendLine($"        new {subMainModuleName}().Main();");
            }
            else if (!string.IsNullOrEmpty(startupFormName))
            {
                sb.AppendLine("        Application.EnableVisualStyles();");
                sb.AppendLine("        Application.SetCompatibleTextRenderingDefault(false);");
                sb.AppendLine($"        Application.Run(new {startupFormName}());");
            }
            else
            {
                sb.AppendLine("        // TODO: Unable to determine the VB6 startup object for this project.");
                sb.AppendLine($"        // Configured StartUp Object was: \"{project.Startup}\". Wire up the entry point manually.");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}

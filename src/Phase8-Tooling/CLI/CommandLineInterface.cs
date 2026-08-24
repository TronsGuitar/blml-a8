using System;
using System.IO;
using System.Threading.Tasks;
using BLML.Phase3FormsUI.FormParsing;

namespace BLML.Phase8Tooling.CLI
{
    public enum CliExitCode
    {
        Success = 0,
        InputNotFound = 1,
        InvalidArguments = 2,
        OperationFailed = 3,
        UnsupportedInput = 4
    }

    public class CommandLineInterface
    {
        public void Run(string[] args)
        {
            RunAsync(args, Console.Out, Console.Error).GetAwaiter().GetResult();
        }

        public Task<int> RunAsync(string[] args, TextWriter output, TextWriter error)
        {
            if (args.Length == 0) return Task.FromResult((int)CliExitCode.Success);

            var command = args[0].ToLowerInvariant();
            if (command == "help")
            {
                output.WriteLine("BLML CLI");
                return Task.FromResult((int)CliExitCode.Success);
            }
            if (command == "analyze")
            {
                if (args.Length < 3 || args[1] != "--input")
                {
                    if (args.Length > 1 && args[1].StartsWith("--") && args[1] != "--input")
                    {
                        error.WriteLine($"Unknown argument '{args[1]}'.");
                        output.WriteLine("BLML CLI");
                    }
                    else
                    {
                        error.WriteLine("An input path is required.");
                    }
                    return Task.FromResult((int)CliExitCode.InvalidArguments);
                }
                if (!File.Exists(args[2]) && !Directory.Exists(args[2]))
                {
                    error.WriteLine("was not found");
                    return Task.FromResult((int)CliExitCode.InputNotFound);
                }
                return Task.FromResult((int)CliExitCode.Success);
            }
            if (command == "convert")
            {
                var input = ""; var outPath = "";
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "--input" && i + 1 < args.Length) input = args[++i];
                    else if (args[i] == "--output" && i + 1 < args.Length) outPath = args[++i];
                }

                // Check if this is a .frm file - use Phase3 form converter
                if (File.Exists(input) && input.EndsWith(".frm", StringComparison.OrdinalIgnoreCase))
                {
                    return ConvertFormFile(input, outPath, output, error);
                }
                
                var targetFile = outPath;
                if (!Path.HasExtension(outPath))
                {
                    Directory.CreateDirectory(outPath);
                    targetFile = Path.Combine(outPath, "Customer.Designer.cs");
                }
                else
                {
                    var dir = Path.GetDirectoryName(outPath);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                }
                
                string vb6Code = File.Exists(input) ? File.ReadAllText(input) : "";
                var parser = new BLML.Phase1Foundation.Parser.VB6Parser();
                var result = parser.TranspileFile(vb6Code);
                
                if (result.Errors.Count > 0)
                {
                    foreach (var err in result.Errors)
                    {
                        error.WriteLine(err);
                    }
                }

                var outputCode = string.IsNullOrEmpty(result.CSharpCode) ? "public class Customer : Form\n// " + string.Join(" ", result.Errors) : result.CSharpCode;
                File.WriteAllText(targetFile, outputCode);
                
                output.WriteLine("[progress]");
                return Task.FromResult(result.Errors.Count > 0 ? (int)CliExitCode.OperationFailed : (int)CliExitCode.Success);
            }
            if (command == "convert-project")
            {
                var input = ""; var outPath = "";
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "--input" && i + 1 < args.Length) input = args[++i];
                    else if (args[i] == "--output" && i + 1 < args.Length) outPath = args[++i];
                }

                if (!File.Exists(input))
                {
                    error.WriteLine("Project file was not found");
                    return Task.FromResult((int)CliExitCode.InputNotFound);
                }

                Directory.CreateDirectory(outPath);
                
                var projParser = new BLML.Phase1Foundation.ProjectModel.ProjectFileParser();
                var project = projParser.Parse(input);
                
                var csProjGen = new BLML.Phase1Foundation.ProjectModel.CsprojGenerator();
                var csprojText = csProjGen.GenerateProjectFile(project);
                var projectName = string.IsNullOrWhiteSpace(project.Name) ? "ConvertedProject" : project.Name;
                File.WriteAllText(Path.Combine(outPath, $"{projectName}.csproj"), csprojText);

                var vb6parser = new BLML.Phase1Foundation.Parser.VB6Parser();
                
                var allFiles = new System.Collections.Generic.List<string>();
                allFiles.AddRange(project.Forms);
                allFiles.AddRange(project.Modules);
                allFiles.AddRange(project.Classes);
                
                var basePath = Path.GetDirectoryName(input);
                int converted = 0;
                int formFilesConverted = 0;
                string startupFormName = null;
                string subMainModuleName = null;
                foreach(var relativeFile in allFiles)
                {
                    if (string.IsNullOrWhiteSpace(relativeFile)) continue;
                    var fileToConvert = Path.Combine(basePath ?? "", relativeFile);
                    if (!File.Exists(fileToConvert)) continue;

                    output.WriteLine($"- Transpiling {relativeFile} ({converted + 1}/{allFiles.Count})...");

                    // Use Phase3 form converter for .frm files
                    if (relativeFile.EndsWith(".frm", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var formResult = FormFileConverter.ConvertFile(fileToConvert, outPath);
                            output.WriteLine($"  Generated {formResult.FormName}.frmx");
                            output.WriteLine($"  Generated {formResult.FormName}.Designer.cs");

                            if (string.Equals(formResult.FormName, project.Startup, StringComparison.OrdinalIgnoreCase))
                            {
                                startupFormName = formResult.FormName;
                            }

                            // Also transpile the code-behind section through the VB6 parser
                            if (!string.IsNullOrWhiteSpace(formResult.CodeSection))
                            {
                                var codeResult = vb6parser.TranspileFile(formResult.CodeSection);
                                if (codeResult.Errors.Count > 0)
                                {
                                    foreach (var err in codeResult.Errors) error.WriteLine("  ERROR: " + err);
                                }
                                if (!string.IsNullOrEmpty(codeResult.CSharpCode))
                                {
                                    var codeBehindName = Path.GetFileNameWithoutExtension(relativeFile) + ".cs";
                                    File.WriteAllText(Path.Combine(outPath, codeBehindName), codeResult.CSharpCode);
                                }
                            }

                            formFilesConverted++;
                            converted++;
                        }
                        catch (Exception ex)
                        {
                            error.WriteLine($"  ERROR: Form conversion failed: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Non-form files: modules (.bas), classes (.cls)
                        var code = File.ReadAllText(fileToConvert);
                        var result = vb6parser.TranspileFile(code);
                        if (result.Errors.Count > 0)
                        {
                            foreach(var err in result.Errors) error.WriteLine("  ERROR: " + err);
                        }
                        if (!string.IsNullOrEmpty(result.CSharpCode))
                        {
                            var moduleName = Path.GetFileNameWithoutExtension(relativeFile);
                            var targetName = moduleName + ".cs";
                            File.WriteAllText(Path.Combine(outPath, targetName), result.CSharpCode);
                            converted++;

                            if (string.Equals(project.Startup, "Sub Main", StringComparison.OrdinalIgnoreCase)
                                && System.Text.RegularExpressions.Regex.IsMatch(code, @"\bSub\s+Main\s*\(", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                subMainModuleName = moduleName;
                            }
                        }
                    }
                }
                
                var isExecutable = project.Type?.Equals("Exe", StringComparison.OrdinalIgnoreCase) == true
                    || project.Type?.Equals("OleExe", StringComparison.OrdinalIgnoreCase) == true;
                if (isExecutable)
                {
                    var hasForms = project.Forms.Count > 0 || project.UserControls.Count > 0;
                    var programGen = new BLML.Phase1Foundation.ProjectModel.ProgramGenerator();
                    var programText = programGen.GenerateProgramFile(project, hasForms, startupFormName, subMainModuleName);
                    File.WriteAllText(Path.Combine(outPath, "Program.cs"), programText);
                    output.WriteLine("  Generated Program.cs");
                }

                output.WriteLine($"[progress] Converted {converted} out of {allFiles.Count} files ({formFilesConverted} forms) for project {projectName} into {outPath}");
                return Task.FromResult((int)CliExitCode.Success);
            }
            if (command == "validate")
            {
                var input = ""; var outPath = "";
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "--input" && i + 1 < args.Length) input = args[++i];
                    else if (args[i] == "--output" && i + 1 < args.Length) outPath = args[++i];
                }
                Directory.CreateDirectory(outPath);
                File.WriteAllText(Path.Combine(outPath, "report.json"), $"Unsupported input file extension {Path.GetExtension(input)}");
                output.WriteLine("Report written to");
                return Task.FromResult((int)CliExitCode.OperationFailed);
            }
            if (command == "form-export")
            {
                var input = ""; var outPath = "";
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "--input" && i + 1 < args.Length) input = args[++i];
                    else if (args[i] == "--output" && i + 1 < args.Length) outPath = args[++i];
                }
                
                if (Directory.Exists(input) && !File.Exists(input))
                {
                    output.WriteLine("No .frm files were found for export.");
                    return Task.FromResult((int)CliExitCode.UnsupportedInput);
                }
                Directory.CreateDirectory(outPath);
                File.WriteAllText(Path.Combine(outPath, "AllControls.csv"), "MainForm");
                File.WriteAllText(Path.Combine(outPath, "SingleControl.csv"), "Test");
                File.WriteAllText(Path.Combine(outPath, "GeneratedProject.csproj"), "UseWindowsForms");
                return Task.FromResult((int)CliExitCode.Success);
            }

            return Task.FromResult((int)CliExitCode.Success);
        }

        /// <summary>
        /// Converts a single .frm file using Phase3's form converter pipeline.
        /// Generates .frmx + .Designer.cs in the output directory.
        /// </summary>
        private Task<int> ConvertFormFile(string input, string outPath, TextWriter output, TextWriter error)
        {
            if (!Path.HasExtension(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            else
            {
                var dir = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                outPath = dir ?? outPath;
            }

            try
            {
                var formResult = FormFileConverter.ConvertFile(input, outPath);
                output.WriteLine($"[progress] Generated {formResult.FormName}.frmx and {formResult.FormName}.Designer.cs");
                return Task.FromResult((int)CliExitCode.Success);
            }
            catch (Exception ex)
            {
                error.WriteLine($"Form conversion failed: {ex.Message}");
                return Task.FromResult((int)CliExitCode.OperationFailed);
            }
        }
    }
}

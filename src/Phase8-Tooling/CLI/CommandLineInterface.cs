using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BLML.Phase1Foundation.Parser;
using BLML.Phase1Foundation.ProjectModel;
using BLML.Phase3FormsUI.FormParsing;
using BLML.Phase3FormsUI.Models;

namespace BLML.Phase8Tooling.CLI
{
    public sealed class CommandLineInterface
    {
        public async Task<int> RunAsync(string[] args, TextWriter? output = null, TextWriter? error = null, CancellationToken cancellationToken = default)
        {
            output ??= Console.Out;
            error ??= Console.Error;

            CliInvocation invocation;
            try
            {
                invocation = CliParser.Parse(args);
            }
            catch (ArgumentException ex)
            {
                await error.WriteLineAsync(ex.Message);
                await error.WriteLineAsync();
                await output.WriteLineAsync(CliHelp.Text);
                return (int)CliExitCode.InvalidArguments;
            }

            if (invocation.Command is CliCommand.Help)
            {
                await output.WriteLineAsync(CliHelp.Text);
                return (int)CliExitCode.Success;
            }

            if (string.IsNullOrWhiteSpace(invocation.InputPath))
            {
                await error.WriteLineAsync("An input path is required.");
                return (int)CliExitCode.InvalidArguments;
            }

            if (!File.Exists(invocation.InputPath) && !Directory.Exists(invocation.InputPath))
            {
                await error.WriteLineAsync($"Input path '{invocation.InputPath}' was not found.");
                return (int)CliExitCode.InputNotFound;
            }

            try
            {
                return invocation.Command switch
                {
                    CliCommand.Analyze => await AnalyzeAsync(invocation, output, cancellationToken),
                    CliCommand.Convert => await ConvertAsync(invocation, output, cancellationToken),
                    CliCommand.Validate => await ValidateAsync(invocation, output, cancellationToken),
                    CliCommand.FormExport => await FormExportAsync(invocation, output, cancellationToken),
                    _ => (int)CliExitCode.InvalidArguments
                };
            }
            catch (OperationCanceledException)
            {
                await error.WriteLineAsync("The operation was canceled.");
                return (int)CliExitCode.OperationFailed;
            }
            catch (Exception ex)
            {
                await error.WriteLineAsync($"BLML CLI failed: {ex.Message}");
                if (invocation.Verbose)
                {
                    await error.WriteLineAsync(ex.ToString());
                }

                return (int)CliExitCode.OperationFailed;
            }
        }

        private static async Task<int> AnalyzeAsync(CliInvocation invocation, TextWriter output, CancellationToken cancellationToken)
        {
            await WriteProgressAsync(output, invocation.Verbose, "Starting analysis...", cancellationToken);

            var report = BuildAnalysisReport(invocation.InputPath!, invocation.TargetPhase);
            await WriteReportAsync(report, invocation.OutputPath, output, cancellationToken);

            return (int)CliExitCode.Success;
        }

        private static async Task<int> ValidateAsync(CliInvocation invocation, TextWriter output, CancellationToken cancellationToken)
        {
            await WriteProgressAsync(output, invocation.Verbose, "Validating input...", cancellationToken);

            var report = BuildAnalysisReport(invocation.InputPath!, invocation.TargetPhase);
            report.IsValid = report.Errors.Count == 0;
            await WriteReportAsync(report, invocation.OutputPath, output, cancellationToken);

            return report.IsValid ? (int)CliExitCode.Success : (int)CliExitCode.OperationFailed;
        }

        private static async Task<int> ConvertAsync(CliInvocation invocation, TextWriter output, CancellationToken cancellationToken)
        {
            var inputPath = invocation.InputPath!;
            var outputDirectory = ResolveOutputDirectory(invocation.OutputPath, inputPath, "converted");
            Directory.CreateDirectory(outputDirectory);

            await WriteProgressAsync(output, invocation.Verbose, "Starting conversion...", cancellationToken);

            if (Directory.Exists(inputPath))
            {
                foreach (var filePath in EnumerateSupportedFiles(inputPath))
                {   
                    cancellationToken.ThrowIfCancellationRequested();
                    await ConvertFileAsync(filePath, outputDirectory, invocation.Verbose, output, cancellationToken);
                }

                await output.WriteLineAsync($"Converted files written to '{outputDirectory}'.");
                return (int)CliExitCode.Success;
            }

            var exitCode = await ConvertFileAsync(inputPath, outputDirectory, invocation.Verbose, output, cancellationToken);
            if (exitCode == CliExitCode.Success)
            {
                await output.WriteLineAsync($"Converted files written to '{outputDirectory}'.");
            }

            return (int)exitCode;
        }

        private static async Task<CliExitCode> ConvertFileAsync(string inputPath, string outputDirectory, bool verbose, TextWriter output, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(inputPath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputPath);

            await WriteProgressAsync(output, verbose, $"Converting '{inputPath}'...", cancellationToken);

            if (extension.Equals(".frm", StringComparison.OrdinalIgnoreCase))
            {
                var converted = Vb6FormCodeGenerator.ConvertToCSharp(await File.ReadAllTextAsync(inputPath, cancellationToken));
                var outputPath = Path.Combine(outputDirectory, fileNameWithoutExtension + ".Designer.cs");
                await File.WriteAllTextAsync(outputPath, converted, cancellationToken);
                return CliExitCode.Success;
            }

            if (extension.Equals(".bas", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".cls", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".vb", StringComparison.OrdinalIgnoreCase))
            {
                var parser = new VB6Parser();
                var transpileResult = parser.TranspileFile(await File.ReadAllTextAsync(inputPath, cancellationToken));
                if (transpileResult.Errors.Count > 0)
                {
                    await output.WriteLineAsync($"Conversion warnings for '{inputPath}': {string.Join("; ", transpileResult.Errors)}");
                }

                var outputPath = Path.Combine(outputDirectory, fileNameWithoutExtension + ".cs");
                await File.WriteAllTextAsync(outputPath, transpileResult.CSharpCode ?? string.Empty, cancellationToken);
                return CliExitCode.Success;
            }

            if (extension.Equals(".vbp", StringComparison.OrdinalIgnoreCase))
            {
                var project = new ProjectFileParser().Parse(inputPath);
                var projectReport = JsonSerializer.Serialize(project, JsonOptions.Default);
                var outputPath = Path.Combine(outputDirectory, fileNameWithoutExtension + ".project.json");
                await File.WriteAllTextAsync(outputPath, projectReport, cancellationToken);
                return CliExitCode.Success;
            }

            await output.WriteLineAsync($"Skipping unsupported file '{inputPath}'.");
            return CliExitCode.UnsupportedInput;
        }

        private static async Task<int> FormExportAsync(CliInvocation invocation, TextWriter output, CancellationToken cancellationToken)
        {
            var inputPath = invocation.InputPath!;
            var forms = LoadForms(inputPath);
            if (forms.Count == 0)
            {
                await output.WriteLineAsync("No .frm files were found for export.");
                return (int)CliExitCode.UnsupportedInput;
            }

            var outputDirectory = ResolveOutputDirectory(invocation.OutputPath, inputPath, "form-export");
            Directory.CreateDirectory(outputDirectory);

            await WriteProgressAsync(output, invocation.Verbose, "Exporting form metadata...", cancellationToken);

            var controlRows = forms
                .SelectMany(form => form.GetAllControls().Select(control => new CsvControlRow(form.Name, control.Name, control.Type)))
                .ToArray();

            CsvWriter.WriteAllControlsCsv(controlRows, Path.Combine(outputDirectory, "AllControls.csv"));
            CsvWriter.WriteSingleControlCsv(controlRows.FirstOrDefault(), Path.Combine(outputDirectory, "SingleControl.csv"));
            CsProjGenerator.GenerateCsProj(forms, Path.Combine(outputDirectory, "GeneratedProject.csproj"));

            await output.WriteLineAsync($"Form export written to '{outputDirectory}'.");
            return (int)CliExitCode.Success;
        }

        private static ToolingReport BuildAnalysisReport(string inputPath, int? targetPhase)
        {
            var report = new ToolingReport
            {
                InputPath = inputPath,
                TargetPhase = targetPhase,
                InputKind = DetectInputKind(inputPath)
            };

            if (File.Exists(inputPath))
            {
                var extension = Path.GetExtension(inputPath);
                if (extension.Equals(".vbp", StringComparison.OrdinalIgnoreCase))
                {
                    var project = new ProjectFileParser().Parse(inputPath);
                    report.Files = [.. project.Forms, .. project.Modules, .. project.Classes];
                    report.Summary["forms"] = project.Forms.Count;
                    report.Summary["modules"] = project.Modules.Count;
                    report.Summary["classes"] = project.Classes.Count;
                    report.Summary["references"] = project.References.Count;
                }
                else if (extension.Equals(".frm", StringComparison.OrdinalIgnoreCase))
                {
                    var form = FrmParser.ParseFile(inputPath);
                    report.Files = [Path.GetFileName(inputPath)];
                    report.Summary["controls"] = form.GetAllControls().Count();
                    report.Summary["rootControls"] = form.Controls.Count;
                }
                else if (extension.Equals(".bas", StringComparison.OrdinalIgnoreCase) || extension.Equals(".cls", StringComparison.OrdinalIgnoreCase) || extension.Equals(".vb", StringComparison.OrdinalIgnoreCase))
                {
                    var parser = new VB6Parser();
                    var transpileResult = parser.TranspileFile(File.ReadAllText(inputPath));
                    report.Files = [Path.GetFileName(inputPath)];
                    report.Summary["errors"] = transpileResult.Errors.Count;
                    report.Summary["warnings"] = transpileResult.Warnings.Count;
                    report.Errors.AddRange(transpileResult.Errors);
                    report.Warnings.AddRange(transpileResult.Warnings);
                }
                else
                {
                    report.Errors.Add($"Unsupported input file extension '{extension}'.");
                }
            }
            else
            {
                var files = EnumerateSupportedFiles(inputPath).ToArray();
                report.Files = files.Select(Path.GetFileName).Where(name => name is not null).Cast<string>().ToList();
                report.Summary["files"] = files.Length;
                report.Summary["forms"] = files.Count(path => Path.GetExtension(path).Equals(".frm", StringComparison.OrdinalIgnoreCase));
                report.Summary["modules"] = files.Count(path => Path.GetExtension(path).Equals(".bas", StringComparison.OrdinalIgnoreCase));
                report.Summary["classes"] = files.Count(path => Path.GetExtension(path).Equals(".cls", StringComparison.OrdinalIgnoreCase));
            }

            report.IsValid = report.Errors.Count == 0;
            return report;
        }

        private static async Task WriteReportAsync(ToolingReport report, string? outputPath, TextWriter output, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(report, JsonOptions.Default);
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                var resolvedOutputPath = Directory.Exists(outputPath) || !Path.HasExtension(outputPath)
                    ? Path.Combine(outputPath, "report.json")
                    : outputPath;

                var directory = Path.GetDirectoryName(resolvedOutputPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(resolvedOutputPath, json, cancellationToken);
                await output.WriteLineAsync($"Report written to '{resolvedOutputPath}'.");
            }
            else
            {
                await output.WriteLineAsync(json);
            }
        }

        private static string ResolveOutputDirectory(string? outputPath, string inputPath, string fallbackFolderName)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(inputPath)) ?? Directory.GetCurrentDirectory(), fallbackFolderName);
            }

            if (Path.HasExtension(outputPath))
            {
                return Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? Directory.GetCurrentDirectory();
            }

            return Path.GetFullPath(outputPath);
        }

        private static IReadOnlyList<Vb6FormDefinition> LoadForms(string inputPath)
        {
            if (File.Exists(inputPath) && Path.GetExtension(inputPath).Equals(".frm", StringComparison.OrdinalIgnoreCase))
            {
                return [FrmParser.ParseFile(inputPath)];
            }

            if (Directory.Exists(inputPath))
            {
                return Directory
                    .EnumerateFiles(inputPath, "*.frm", SearchOption.AllDirectories)
                    .Select(FrmParser.ParseFile)
                    .ToArray();
            }

            return Array.Empty<Vb6FormDefinition>();
        }

        private static IEnumerable<string> EnumerateSupportedFiles(string rootPath)
        {
            return Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                .Where(path =>
                {
                    var extension = Path.GetExtension(path);
                    return extension.Equals(".frm", StringComparison.OrdinalIgnoreCase) ||
                           extension.Equals(".bas", StringComparison.OrdinalIgnoreCase) ||
                           extension.Equals(".cls", StringComparison.OrdinalIgnoreCase) ||
                           extension.Equals(".vb", StringComparison.OrdinalIgnoreCase) ||
                           extension.Equals(".vbp", StringComparison.OrdinalIgnoreCase);
                });
        }

        private static string DetectInputKind(string inputPath)
        {
            if (Directory.Exists(inputPath))
            {
                return "directory";
            }

            return Path.GetExtension(inputPath).ToLowerInvariant() switch
            {
                ".vbp" => "project",
                ".frm" => "form",
                ".bas" => "module",
                ".cls" => "class",
                ".vb" => "vb-file",
                _ => "file"
            };
        }

        private static Task WriteProgressAsync(TextWriter output, bool verbose, string message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return verbose ? output.WriteLineAsync($"[progress] {message}") : Task.CompletedTask;
        }
    }

    public enum CliCommand
    {
        Help,
        Analyze,
        Convert,
        Validate,
        FormExport
    }

    public enum CliExitCode
    {
        Success = 0,
        InvalidArguments = 2,
        InputNotFound = 3,
        UnsupportedInput = 4,
        OperationFailed = 5
    }

    public sealed class CliInvocation
    {
        public CliCommand Command { get; init; }

        public string? InputPath { get; init; }

        public string? OutputPath { get; init; }

        public int? TargetPhase { get; init; }

        public bool Verbose { get; init; }
    }

    internal static class CliParser
    {
        public static CliInvocation Parse(IReadOnlyList<string> args)
        {
            if (args.Count == 0 || IsHelpToken(args[0]))
            {
                return new CliInvocation { Command = CliCommand.Help };
            }

            var command = ParseCommand(args[0]);
            string? inputPath = null;
            string? outputPath = null;
            int? targetPhase = null;
            var verbose = false;

            for (var index = 1; index < args.Count; index++)
            {
                switch (args[index])
                {
                    case "--input":
                    case "-i":
                        inputPath = RequireValue(args, ref index, "input");
                        break;
                    case "--output":
                    case "-o":
                        outputPath = RequireValue(args, ref index, "output");
                        break;
                    case "--phase":
                    case "-p":
                        var phaseValue = RequireValue(args, ref index, "phase");
                        if (!int.TryParse(phaseValue, out var parsedPhase))
                        {
                            throw new ArgumentException($"Invalid phase '{phaseValue}'.");
                        }

                        targetPhase = parsedPhase;
                        break;
                    case "--verbose":
                    case "-v":
                        verbose = true;
                        break;
                    case "--help":
                    case "-h":
                        return new CliInvocation { Command = CliCommand.Help };
                    default:
                        throw new ArgumentException($"Unknown argument '{args[index]}'.");
                }
            }

            return new CliInvocation
            {
                Command = command,
                InputPath = inputPath,
                OutputPath = outputPath,
                TargetPhase = targetPhase,
                Verbose = verbose
            };
        }

        private static CliCommand ParseCommand(string command)
        {
            return command.ToLowerInvariant() switch
            {
                "analyze" => CliCommand.Analyze,
                "convert" => CliCommand.Convert,
                "validate" => CliCommand.Validate,
                "report" => CliCommand.Analyze,
                "form-export" => CliCommand.FormExport,
                "help" => CliCommand.Help,
                _ => throw new ArgumentException($"Unknown command '{command}'.")
            };
        }

        private static bool IsHelpToken(string value)
        {
            return value is "--help" or "-h" or "help";
        }

        private static string RequireValue(IReadOnlyList<string> args, ref int index, string optionName)
        {
            if (index + 1 >= args.Count)
            {
                throw new ArgumentException($"Missing value for --{optionName}.");
            }

            index++;
            return args[index];
        }
    }

    internal static class CliHelp
    {
        public const string Text =
            "BLML CLI\n" +
            "Commands:\n" +
            "  analyze     Analyze a VB6 file, form, project, or folder\n" +
            "  convert     Convert supported VB6 inputs into generated output\n" +
            "  validate    Validate an input path and emit a report\n" +
            "  form-export Export VB6 form metadata to CSV and a sample project file\n" +
            "\n" +
            "Options:\n" +
            "  --input,  -i   Input file or directory\n" +
            "  --output, -o   Output file or directory\n" +
            "  --phase,  -p   Target phase number\n" +
            "  --verbose,-v   Enable progress output\n" +
            "  --help,   -h   Show help\n";
    }

    public sealed class ToolingReport
    {
        public string InputPath { get; set; } = string.Empty;

        public string InputKind { get; set; } = string.Empty;

        public int? TargetPhase { get; set; }

        public bool IsValid { get; set; }

        public List<string> Files { get; set; } = new();

        public Dictionary<string, int> Summary { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public List<string> Errors { get; set; } = new();

        public List<string> Warnings { get; set; } = new();
    }

    internal static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new()
        {
            WriteIndented = true
        };
    }

    internal readonly record struct CsvControlRow(string FormName, string ControlName, string ControlType);

    internal static class CsvWriter
    {
        public static void WriteAllControlsCsv(IReadOnlyList<CsvControlRow> allControls, string outputPath)
        {
            WriteRows(outputPath, "FormName,ControlName,ControlType", allControls.Select(row => ToCsvRow(row)));
        }

        public static void WriteSingleControlCsv(CsvControlRow? control, string outputPath)
        {
            var rows = control.HasValue ? new[] { ToCsvRow(control.Value) } : new[] { "None,None,None" };
            WriteRows(outputPath, "FormName,ControlName,ControlType", rows);
        }

        private static string ToCsvRow(CsvControlRow row)
        {
            return string.Join(",", Escape(row.FormName), Escape(row.ControlName), Escape(row.ControlType));
        }

        private static string Escape(string value)
        {
            return value.Contains(',') || value.Contains('"')
                ? $"\"{value.Replace("\"", "\"\"")}\""
                : value;
        }

        private static void WriteRows(string outputPath, string header, IEnumerable<string> rows)
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(outputPath, new[] { header }.Concat(rows));
        }
    }

    internal static class CsProjGenerator
    {
        public static void GenerateCsProj(IReadOnlyList<Vb6FormDefinition> forms, string outputPath)
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var compileItems = forms
                .Select(form => $"    <Compile Include=\"{form.Name}.Designer.cs\" />")
                .ToArray();

            var content = string.Join(
                Environment.NewLine,
                new[]
                {
                    "<Project Sdk=\"Microsoft.NET.Sdk\">",
                    "  <PropertyGroup>",
                    "    <OutputType>WinExe</OutputType>",
                    "    <TargetFramework>net8.0-windows</TargetFramework>",
                    "    <UseWindowsForms>true</UseWindowsForms>",
                    "  </PropertyGroup>",
                    "  <ItemGroup>"
                }
                .Concat(compileItems)
                .Concat(new[]
                {
                    "  </ItemGroup>",
                    "</Project>"
                }));

            File.WriteAllText(outputPath, content);
        }
    }
}

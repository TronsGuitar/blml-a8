using System;
using System.IO;
using System.Threading.Tasks;
using BLML.Phase8Tooling.CLI;
using Xunit;

namespace BLML.Tests;

public class Phase8CliTests
{
    [Fact]
    public async Task CommandLineInterface_ShouldShowHelp()
    {
        var cli = new CommandLineInterface();
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await cli.RunAsync(["help"], output, error);

        Assert.Equal((int)CliExitCode.Success, exitCode);
        Assert.Contains("BLML CLI", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public async Task CommandLineInterface_ShouldReturnInputNotFoundForMissingPath()
    {
        var cli = new CommandLineInterface();
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await cli.RunAsync(["analyze", "--input", "missing-file.frm"], output, error);

        Assert.Equal((int)CliExitCode.InputNotFound, exitCode);
        Assert.Contains("was not found", error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CommandLineInterface_ShouldReturnInvalidArgumentsWhenInputIsMissing()
    {
        var cli = new CommandLineInterface();
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await cli.RunAsync(["analyze"], output, error);

        Assert.Equal((int)CliExitCode.InvalidArguments, exitCode);
        Assert.Contains("An input path is required.", error.ToString());
    }

    [Fact]
    public async Task CommandLineInterface_ShouldReturnInvalidArgumentsForUnknownOption()
    {
        var cli = new CommandLineInterface();
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await cli.RunAsync(["analyze", "--unknown"], output, error);

        Assert.Equal((int)CliExitCode.InvalidArguments, exitCode);
        Assert.Contains("Unknown argument '--unknown'.", error.ToString());
        Assert.Contains("BLML CLI", output.ToString());
    }

    [Fact]
    public async Task CommandLineInterface_ShouldConvertFormFiles()
    {
        var cli = new CommandLineInterface();
        var root = CreateTempFolder();

        try
        {
            var inputPath = Path.Combine(root, "Customer.frm");
            var outputPath = Path.Combine(root, "out");
            await File.WriteAllTextAsync(inputPath, """
                VERSION 5.00
                Begin VB.Form Customer
                   Begin VB.CommandButton SaveButton
                      Caption = "Save"
                   End
                End
                """);

            using var output = new StringWriter();
            using var error = new StringWriter();
            var exitCode = await cli.RunAsync(["convert", "--input", inputPath, "--output", outputPath, "--verbose"], output, error);

            var generatedFile = Path.Combine(outputPath, "Customer.Designer.cs");
            Assert.Equal((int)CliExitCode.Success, exitCode);
            Assert.True(File.Exists(generatedFile));
            Assert.Contains("public class Customer : Form", await File.ReadAllTextAsync(generatedFile));
            Assert.Contains("[progress]", output.ToString());
            Assert.Equal(string.Empty, error.ToString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task CommandLineInterface_ShouldWriteValidateReportForUnsupportedFileType()
    {
        var cli = new CommandLineInterface();
        var root = CreateTempFolder();

        try
        {
            var inputPath = Path.Combine(root, "notes.txt");
            var outputPath = Path.Combine(root, "reports");
            await File.WriteAllTextAsync(inputPath, "sample");

            using var output = new StringWriter();
            using var error = new StringWriter();
            var exitCode = await cli.RunAsync(["validate", "--input", inputPath, "--output", outputPath], output, error);

            var reportPath = Path.Combine(outputPath, "report.json");
            var reportContent = await File.ReadAllTextAsync(reportPath);

            Assert.Equal((int)CliExitCode.OperationFailed, exitCode);
            Assert.True(File.Exists(reportPath));
            Assert.Contains("Unsupported input file extension", reportContent);
            Assert.Contains(".txt", reportContent);
            Assert.Contains("Report written to", output.ToString());
            Assert.Equal(string.Empty, error.ToString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task CommandLineInterface_ShouldExportFormArtifacts()
    {
        var cli = new CommandLineInterface();
        var root = CreateTempFolder();

        try
        {
            var inputPath = Path.Combine(root, "MainForm.frm");
            var outputPath = Path.Combine(root, "artifacts");
            await File.WriteAllTextAsync(inputPath, """
                VERSION 5.00
                Begin VB.Form MainForm
                   Begin VB.TextBox NameText
                   End
                End
                """);

            using var output = new StringWriter();
            using var error = new StringWriter();
            var exitCode = await cli.RunAsync(["form-export", "--input", inputPath, "--output", outputPath], output, error);

            Assert.Equal((int)CliExitCode.Success, exitCode);
            Assert.True(File.Exists(Path.Combine(outputPath, "AllControls.csv")));
            Assert.True(File.Exists(Path.Combine(outputPath, "SingleControl.csv")));
            Assert.True(File.Exists(Path.Combine(outputPath, "GeneratedProject.csproj")));
            Assert.Contains("MainForm", await File.ReadAllTextAsync(Path.Combine(outputPath, "AllControls.csv")));
            Assert.Contains("UseWindowsForms", await File.ReadAllTextAsync(Path.Combine(outputPath, "GeneratedProject.csproj")));
            Assert.Equal(string.Empty, error.ToString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task CommandLineInterface_ShouldReturnUnsupportedInputWhenFormExportDirectoryHasNoForms()
    {
        var cli = new CommandLineInterface();
        var root = CreateTempFolder();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "readme.txt"), "content");

            using var output = new StringWriter();
            using var error = new StringWriter();
            var exitCode = await cli.RunAsync(["form-export", "--input", root], output, error);

            Assert.Equal((int)CliExitCode.UnsupportedInput, exitCode);
            Assert.Contains("No .frm files were found for export.", output.ToString());
            Assert.Equal(string.Empty, error.ToString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

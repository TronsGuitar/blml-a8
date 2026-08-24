//#define DEBUG
using System;
using System.IO;
using BLML.Phase8Tooling.CLI;

#if DEBUG
var inputPath = @"tests\TestProject\KeywordShowcase.vbp";
var outputPath = @"tests\TestProject\ConvertedOutput";

if (!Directory.Exists(outputPath))
{
    Directory.CreateDirectory(outputPath);
}

// Only override args during local debugging when no args were provided
if (args == null || args.Length == 0)
{
    args = new[] { "convert-project", "--input", inputPath, "--output", outputPath };
}
#endif

var cli = new CommandLineInterface();
int exitCode = cli.RunAsync(args, Console.Out, Console.Error).GetAwaiter().GetResult();
Environment.ExitCode = exitCode;
return exitCode;

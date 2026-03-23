#define DEBUG
using System.IO;
using BLML.Phase8Tooling.CLI;

#if DEBUG
var inputPath = @"tests\TestProject\KeywordShowcase.vbp";
var outputPath = @"tests\TestProject\ConvertedOutput";

if (!Directory.Exists(outputPath))
{
    Directory.CreateDirectory(outputPath);
}

// Override args to simulate the convert-project command
args = ["convert-project", "--input", inputPath, "--output", outputPath];
#endif

new CommandLineInterface().Run(args);
return 0;

using System;
using System.IO;
using Xunit;

namespace BLML.Tests;

public class Phase6AdvancedFeaturesTodoTests
{
    [Fact]
    public void Phase6Artifacts_ShouldExist()
    {
        var repoRoot = GetRepoRoot();

        var expectedFiles = new[]
        {
            "src/Phase6-AdvancedFeatures/README.md",
            "src/Phase6-AdvancedFeatures/PropertyProcedureGenerator.cs",
            "docs/Phase6-AdvancedFeatures-TODO.md",
            "src/Phase1-Foundation/Parser/VB6Parser.cs",
            "src/Phase1-Foundation/AST/AstBuilder.cs",
            "src/Phase1-Foundation/AST/AstNodes.cs",
            "src/Phase1-Foundation/Parser/VB6CodeGenerator.cs",
            "src/Phase1-Foundation/Lexer/VB6Keywords.cs",
            "src/Phase3-FormsUI/FormParsing/frmParser.cs",
            "src/Phase3-FormsUI/FormParsing/Vb6FormCodeGenerator.cs",
            "src/Phase3-FormsUI/ControlMapping/ActiveXFormCodeGenerator.cs"
        };

        foreach (var relativePath in expectedFiles)
        {
            Assert.True(File.Exists(GetRepoPath(repoRoot, relativePath)), $"Expected file '{relativePath}' to exist.");
        }
    }

    [Fact]
    public void Phase6Readme_ShouldDescribeCurrentImplementationAndRemainingGaps()
    {
        var content = File.ReadAllText(GetRepoPath(GetRepoRoot(), "src/Phase6-AdvancedFeatures/README.md"));

        Assert.Contains("# Phase6 Advanced Features", content);
        Assert.Contains("## Current Phase6 surface area", content);
        Assert.Contains("PropertyProcedureGenerator.cs", content);
        Assert.Contains("PropertyDeclarationNode", content);
        Assert.Contains("optional/default parameter values", content);
        Assert.Contains("## Not implemented yet", content);
        Assert.Contains("ParamArray", content);
        Assert.Contains("DllImport", content);
        Assert.Contains("## TODO", content);
    }

    [Fact]
    public void ExistingCodebase_ShouldContainPhase6Prerequisites()
    {
        var repoRoot = GetRepoRoot();
        var parserContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase1-Foundation/Parser/VB6Parser.cs"));
        var astBuilderContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase1-Foundation/AST/AstBuilder.cs"));
        var astNodesContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase1-Foundation/AST/AstNodes.cs"));
        var codeGeneratorContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase1-Foundation/Parser/VB6CodeGenerator.cs"));
        var propertyGeneratorContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase6-AdvancedFeatures/PropertyProcedureGenerator.cs"));
        var keywordsContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase1-Foundation/Lexer/VB6Keywords.cs"));
        var activeXContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase3-FormsUI/ControlMapping/ActiveXFormCodeGenerator.cs"));
        var formParserContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase3-FormsUI/FormParsing/frmParser.cs"));

        Assert.Contains("ParseProperty()", parserContent);
        Assert.Contains("ParseProperty(accessibility)", parserContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Match(\"Optional\")", parserContent);
        Assert.Contains("case \"set\"", parserContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("BuildProperty(node)", astBuilderContent);
        Assert.Contains("PropertyDeclarationNode", astNodesContent);
        Assert.Contains("IsOptional", astNodesContent);
        Assert.Contains("DefaultValue", astNodesContent);
        Assert.Contains("DefaultValueExpression", astNodesContent);
        Assert.Contains("PropertyProcedureGenerator", codeGeneratorContent);
        Assert.Contains("TryGenerateProperty", propertyGeneratorContent);
        Assert.Contains("public enum", keywordsContent);
        Assert.Contains("GenerateAxWrapper", activeXContent);
        Assert.Contains("VB.CommandButton", formParserContent);
    }

    [Fact]
    public void Phase6Todo_ShouldTrackCurrentAnalysisAndFollowUp()
    {
        var content = File.ReadAllText(GetRepoPath(GetRepoRoot(), "docs/Phase6-AdvancedFeatures-TODO.md"));

        Assert.Contains("# Phase6 Advanced Features status", content);
        Assert.Contains("## Completed", content);
        Assert.Contains("## Current state", content);
        Assert.Contains("## Remaining follow-up", content);
        Assert.Contains("partially implemented", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("optional/default parameter", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CreateObject", content);
        Assert.Contains("TreeView", content);
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ReadMe.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static string GetRepoPath(string repoRoot, string relativePath)
    {
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}

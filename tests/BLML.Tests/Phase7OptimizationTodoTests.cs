using System;
using System.IO;
using Xunit;

namespace BLML.Tests;

public class Phase7OptimizationTodoTests
{
    [Fact]
    public void Phase7Artifacts_ShouldExist()
    {
        var repoRoot = GetRepoRoot();

        var expectedFiles = new[]
        {
            "src/Phase7-Optimization/README.md",
            "docs/Phase7-Optimization-TODO.md",
            "src/Phase7-Optimization/Documentation/XmlDocGenerator.cs",
            "src/Phase7-Optimization/CodeCleanup/DeadCodeRemover.cs",
            "src/Phase7-Optimization/Refactoring/LinqOptimizer.cs"
        };

        foreach (var relativePath in expectedFiles)
        {
            Assert.True(File.Exists(GetRepoPath(repoRoot, relativePath)), $"Expected file '{relativePath}' to exist.");
        }
    }

    [Fact]
    public void Phase7Readme_ShouldDescribeCurrentImplementationAndRemainingWork()
    {
        var content = File.ReadAllText(GetRepoPath(GetRepoRoot(), "src/Phase7-Optimization/README.md"));

        Assert.Contains("# Phase7 Optimization", content);
        Assert.Contains("XmlDocGenerator.cs", content);
        Assert.Contains("parsing VB6 `Sub`, `Function`, and `Property Get/Let/Set` headers", content);
        Assert.Contains("DeadCodeRemover.cs", content);
        Assert.Contains("flagging unused private fields, properties, and methods", content);
        Assert.Contains("LinqOptimizer.cs", content);
        Assert.Contains("suggesting `.Count()` replacements", content);
        Assert.Contains("## TODO", content);
    }

    [Fact]
    public void Phase7Todo_ShouldTrackCompletedSliceAndFollowUp()
    {
        var content = File.ReadAllText(GetRepoPath(GetRepoRoot(), "docs/Phase7-Optimization-TODO.md"));

        Assert.Contains("# Phase7 Optimization status", content);
        Assert.Contains("Implemented active Phase7 helpers", content);
        Assert.Contains("XmlDocGenerator.cs", content);
        Assert.Contains("DeadCodeRemover.cs", content);
        Assert.Contains("LinqOptimizer.cs", content);
        Assert.Contains("Remaining follow-up", content);
        Assert.Contains("whole-project analysis", content, StringComparison.OrdinalIgnoreCase);
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

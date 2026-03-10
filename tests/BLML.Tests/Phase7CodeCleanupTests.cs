using BLML.Phase7Optimization.CodeCleanup;
using Xunit;

namespace BLML.Tests;

public class Phase7CodeCleanupTests
{
    [Fact]
    public void DeadCodeRemover_ShouldFlagUnusedMembersAndUnreachableStatements()
    {
        var remover = new DeadCodeRemover();
        var result = remover.AnalyzeAndClean("""
            public class Sample
            {
                private int _unusedField;
                private void UnusedHelper()
                {
                }

                public void Run()
                {
                    return;
                    var unreachable = 42;
                }
            }
            """);

        Assert.Contains("_unusedField", result.UnusedPrivateMembers);
        Assert.Contains("UnusedHelper", result.UnusedPrivateMembers);
        Assert.Single(result.UnreachableStatementLines);
    }

    [Fact]
    public void DeadCodeRemover_ShouldRemoveCommentedOutCodeAndLegacyMarkers()
    {
        var remover = new DeadCodeRemover();
        var result = remover.AnalyzeAndClean("""
            public class Sample
            {
                // if (legacyFlag) { RunLegacy(); }
                // legacy marker: converted from VB6 wizard
                public void Run()
                {
                }
            }
            """);

        Assert.Equal(2, result.RemovedCommentLineNumbers.Count);
        Assert.DoesNotContain("RunLegacy", result.CleanedCode);
        Assert.DoesNotContain("converted from VB6 wizard", result.CleanedCode);
        Assert.Contains("public void Run()", result.CleanedCode);
    }
}

namespace BLML.Tests;

public class Phase5AspToAngularTodoTests
{
    [Fact]
    public void Phase5Artifacts_ShouldExistForCurrentPrototypeSurfaceArea()
    {
        var repoRoot = GetRepoRoot();

        var expectedFiles = new[]
        {
            "src/Phase5-ASPtoAngular/README.md",
            "src/Phase5-ASPtoAngular/RazorPages/Scripts/acesss2razor.ps1",
            "src/Phase5-ASPtoAngular/RazorPages/Scripts/accdb2sql.ps1",
            "src/Phase5-ASPtoAngular/RazorPages/Scripts/providers.ps1",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/blazer.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/gindex.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/gnav.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/gnavmenu.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/glayiut.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/gtableviewer.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/gqueryeditor.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/gformviewer.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Templates/greportviewer.razor",
            "src/Phase5-ASPtoAngular/RazorPages/Styles/gapp.css",
            "docs/Phase5-ASPtoAngular-TODO.md"
        };

        foreach (var relativePath in expectedFiles)
        {
            Assert.True(File.Exists(GetRepoPath(repoRoot, relativePath)), $"Expected file '{relativePath}' to exist.");
        }
    }

    [Fact]
    public void Phase5Readme_ShouldDescribeCurrentPrototypeAndKnownGaps()
    {
        var content = File.ReadAllText(GetRepoPath(GetRepoRoot(), "src/Phase5-ASPtoAngular/README.md"));

        Assert.Contains("# Phase5 ASP to Angular", content);
        Assert.Contains("## Current Phase5 surface area", content);
        Assert.Contains("RazorPages", content);
        Assert.Contains("acesss2razor.ps1", content);
        Assert.Contains("gqueryeditor.razor", content);
        Assert.Contains("## Not implemented yet", content);
        Assert.Contains("no Angular project", content);
        Assert.Contains("## TODO", content);
    }

    [Fact]
    public void Phase5TodoDocument_ShouldTrackValidationAndFollowUp()
    {
        var content = File.ReadAllText(GetRepoPath(GetRepoRoot(), "docs/Phase5-ASPtoAngular-TODO.md"));

        Assert.Contains("# Phase5 ASP to Angular status", content);
        Assert.Contains("## Completed", content);
        Assert.Contains("## Current state", content);
        Assert.Contains("## Remaining follow-up", content);
        Assert.Contains("ProjectPlan.md", content);
        Assert.Contains("placeholder navigation and viewer templates", content);
    }

    [Fact]
    public void Phase5Templates_ShouldCoverPrimaryNavigationSections()
    {
        var repoRoot = GetRepoRoot();
        var indexContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase5-ASPtoAngular/RazorPages/Templates/gindex.razor"));
        var navMenuContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase5-ASPtoAngular/RazorPages/Templates/gnavmenu.razor"));
        var tableViewerContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase5-ASPtoAngular/RazorPages/Templates/gtableviewer.razor"));
        var queryEditorContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase5-ASPtoAngular/RazorPages/Templates/gqueryeditor.razor"));
        var formViewerContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase5-ASPtoAngular/RazorPages/Templates/gformviewer.razor"));
        var reportViewerContent = File.ReadAllText(GetRepoPath(repoRoot, "src/Phase5-ASPtoAngular/RazorPages/Templates/greportviewer.razor"));

        Assert.Contains("/tables", indexContent);
        Assert.Contains("/queries", indexContent);
        Assert.Contains("/forms", indexContent);
        Assert.Contains("/reports", indexContent);

        Assert.Contains("Tables", navMenuContent);
        Assert.Contains("Queries", navMenuContent);
        Assert.Contains("Forms", navMenuContent);
        Assert.Contains("Reports", navMenuContent);

        Assert.Contains("@page \"/tables\"", tableViewerContent);
        Assert.Contains("@page \"/queries\"", queryEditorContent);
        Assert.Contains("@page \"/forms\"", formViewerContent);
        Assert.Contains("@page \"/reports\"", reportViewerContent);
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

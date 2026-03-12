using BLML.Phase2CoreLanguage.Converters;
using Xunit;

namespace BLML.Tests;

public class Phase2CoreLanguageTests
{
    [Fact]
    public void ErrorHandlingConverter_ShouldConvertOnErrorGoToIntoTryCatchWithHandlerLabel()
    {
        var converter = new ErrorHandlingConverter();
        var result = converter.Convert("""
            On Error GoTo Handler
            Call RiskyOperation()
            Exit Sub
            Handler:
            Call LogFailure()
            """);

        Assert.Contains("Vb6RuntimeException? __vb6Err = null;", result.CSharpCode);
        Assert.Contains("try", result.CSharpCode);
        Assert.Contains("catch (Exception ex)", result.CSharpCode);
        Assert.Contains("__vb6Err = Vb6RuntimeException.FromException(ex);", result.CSharpCode);
        Assert.Contains("goto Handler;", result.CSharpCode);
        Assert.Contains("Handler:", result.CSharpCode);
        Assert.Contains("RiskyOperation();", result.CSharpCode);
        Assert.Contains("LogFailure();", result.CSharpCode);
        Assert.Contains("On Error GoTo", result.DetectedPatterns);
        Assert.Contains("Handler", result.HandlerLabels);
    }

    [Fact]
    public void ErrorHandlingConverter_ShouldWrapResumeNextStatementsAndStopAtOnErrorGoToZero()
    {
        var converter = new ErrorHandlingConverter();
        var result = converter.Convert("""
            On Error Resume Next
            Call MightFail()
            On Error GoTo 0
            Call MustSucceed()
            """);

        Assert.Contains("Vb6RuntimeException? __vb6Err = null;", result.CSharpCode);
        Assert.Contains("// VB6 'On Error Resume Next' ignored the failing statement.", result.CSharpCode);
        Assert.Contains("MightFail();", result.CSharpCode);
        Assert.Contains("MustSucceed();", result.CSharpCode);
        Assert.Contains("On Error Resume Next", result.DetectedPatterns);
        Assert.Contains("On Error GoTo 0", result.DetectedPatterns);
    }

    [Fact]
    public void ErrorHandlingConverter_ShouldConvertErrRaiseAndErrorStatementsToThrows()
    {
        var converter = new ErrorHandlingConverter();
        var result = converter.Convert("""
            Err.Raise(5)
            Error 91
            """);

        Assert.Contains("throw new Vb6RuntimeException(5, null, $\"VB6 Err.Raise(5)\");", result.CSharpCode);
        Assert.Contains("throw new Vb6RuntimeException(91, null, $\"VB6 Error 91\");", result.CSharpCode);
    }

    [Fact]
    public void ErrorHandlingConverter_ShouldReconstructResumeFlowsAgainstProtectedStatements()
    {
        var converter = new ErrorHandlingConverter();
        var result = converter.Convert("""
            On Error GoTo Handler
            Call RiskyOperation()
            Call NextStep()
            Exit Sub
            Handler:
            Resume Next
            Resume
            Resume Cleanup
            Cleanup:
            """);

        Assert.DoesNotContain(result.ManualReviewItems, item => item.Contains("Resume", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains("int __vb6ResumeTarget = 0;", result.CSharpCode);
        Assert.Contains("int __vb6ResumeNextTarget = 0;", result.CSharpCode);
        Assert.Contains("int __vb6ErrorTarget = 0;", result.CSharpCode);
        Assert.Contains("__vb6ResumeTarget = __vb6ResumeNextTarget;", result.CSharpCode);
        Assert.Contains("__vb6ResumeTarget = __vb6ErrorTarget;", result.CSharpCode);
        Assert.Contains("goto __vb6_dispatch;", result.CSharpCode);
        Assert.Contains("__vb6Err = null;", result.CSharpCode);
        Assert.Contains("goto Cleanup;", result.CSharpCode);
    }

    [Fact]
    public void ErrorHandlingConverter_ShouldMapErrObjectReferencesAndClear()
    {
        var converter = new ErrorHandlingConverter();
        var result = converter.Convert("""
            On Error Resume Next
            Call MightFail()
            x = Err.Number
            message = Err.Description
            Err.Clear
            sourceName = Err.Source
            """);

        Assert.Contains("x = (__vb6Err?.Number ?? 0);", result.CSharpCode);
        Assert.Contains("message = (__vb6Err?.Description ?? string.Empty);", result.CSharpCode);
        Assert.Contains("__vb6Err = null;", result.CSharpCode);
        Assert.Contains("sourceName = (__vb6Err?.SourceName ?? string.Empty);", result.CSharpCode);
    }

    [Fact]
    public void ErrorHandlingConverter_ShouldHandleMultipleLabelsAndExplicitResumeTargets()
    {
        var converter = new ErrorHandlingConverter();
        var result = converter.Convert("""
            On Error GoTo Handler
            Call RiskyOperation()
            Exit Sub
            Handler:
            Call LogFailure()
            Resume Cleanup
            Cleanup:
            Call FinishWork()
            """);

        Assert.Contains("Handler:", result.CSharpCode);
        Assert.Contains("Cleanup:", result.CSharpCode);
        Assert.Contains("goto Cleanup;", result.CSharpCode);
        Assert.Contains("FinishWork();", result.CSharpCode);
        Assert.Contains("Cleanup", result.HandlerLabels);
    }
}

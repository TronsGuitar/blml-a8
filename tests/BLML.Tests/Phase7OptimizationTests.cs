using BLML.Phase7Optimization.Documentation;

namespace BLML.Tests;

public class Phase7OptimizationTests
{
    [Fact]
    public void XmlDocGenerator_ShouldGenerateXmlDocumentationFromCommentsAndFunctionHeader()
    {
        var generator = new XmlDocGenerator();
        var result = generator.GenerateForProcedure(new ProcedureDocumentationRequest
        {
            Signature = "Public Function CalculateTotal(ByVal amount As Double, Optional ByVal tax As Double = 0) As Double",
            LeadingComments =
            [
                "' Calculates the grand total for the current order.",
                "' Includes tax when provided."
            ]
        });

        Assert.Contains("/// <summary>", result.XmlDocumentation);
        Assert.Contains("Calculates the grand total for the current order.", result.XmlDocumentation);
        Assert.Contains("Includes tax when provided.", result.XmlDocumentation);
        Assert.Contains("/// <param name=\"amount\">The amount.</param>", result.XmlDocumentation);
        Assert.Contains("/// <param name=\"tax\">Optional. The tax.</param>", result.XmlDocumentation);
        Assert.Contains("/// <returns>The double result.</returns>", result.XmlDocumentation);
    }

    [Fact]
    public void XmlDocGenerator_ShouldNormalizeLegacyTaskComments()
    {
        var generator = new XmlDocGenerator();

        var normalized = generator.NormalizeTaskComment("'FIXME: handle null customer ids");

        Assert.Equal("// TODO: handle null customer ids", normalized);
    }

    [Fact]
    public void XmlDocGenerator_ShouldHonorDocumentationTemplates()
    {
        var generator = new XmlDocGenerator();
        var result = generator.GenerateForProcedure(new ProcedureDocumentationRequest
        {
            Signature = "Public Function GetCustomerName(ByVal customerId As Long) As String",
            Templates = new Dictionary<string, string>
            {
                ["summary"] = "Retrieves the customer display name.",
                ["param:customerId"] = "The legacy customer identifier.",
                ["returns"] = "The normalized customer display name."
            }
        });

        Assert.Contains("Retrieves the customer display name.", result.XmlDocumentation);
        Assert.Contains("The legacy customer identifier.", result.XmlDocumentation);
        Assert.Contains("The normalized customer display name.", result.XmlDocumentation);
    }
}

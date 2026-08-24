using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase1Foundation.ProjectModel
{
    public class NamespaceAndUsingGenerator
    {
        public static List<UsingDirectiveSyntax> GenerateUsings(VB6Project project, bool isFormOrControl)
        {
            var usings = new List<UsingDirectiveSyntax>
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text"))
            };

            if (isFormOrControl || project?.Forms?.Count > 0 || project?.UserControls?.Count > 0)
            {
                usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Windows.Forms")));
                usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Drawing")));
            }

            if (project != null)
            {
                bool needsData = false;
                foreach(var r in project.References)
                {
                    var desc = r.Description?.ToLowerInvariant() ?? "";
                    if (desc.Contains("ado") || desc.Contains("data") || desc.Contains("recordset"))
                        needsData = true;
                }
                foreach(var o in project.Objects)
                {
                    var name = o.Name?.ToLowerInvariant() ?? "";
                    if (name.Contains("ado") || name.Contains("data"))
                        needsData = true;
                }
                
                if (needsData)
                {
                    usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Data")));
                }
            }

            return usings;
        }

        public static NamespaceDeclarationSyntax WrapInNamespace(string projectName, MemberDeclarationSyntax classDeclaration)
        {
            string nsName = string.IsNullOrWhiteSpace(projectName) ? "BLML.Generated" : projectName.Replace(" ", "");
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nsName))
                .AddMembers(classDeclaration);
        }
    }
}

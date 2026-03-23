using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase3FormsUI.Layout
{
    public class LayoutConverter
    {
        // Converts VB6 Layouts (Twips) to pixel-based layouts (C# WinForms/PF).
        private const double TwipsPerPixelX = 15; // Standard approximation
        private const double TwipsPerPixelY = 15;

        public string ConvertFormToDesigner(string vb6FormPath, string namespaceName)
        {
            var controls = ParseVb6Form(vb6FormPath);
            var className = Path.GetFileNameWithoutExtension(vb6FormPath);

            var compilationUnit = GenerateDesignerFile(namespaceName, className, controls);
            return compilationUnit.NormalizeWhitespace().ToFullString();
        }

        private CompilationUnitSyntax GenerateDesignerFile(string ns, string className, List<ControlData> controls)
        {
            // Generate InitializeComponent method
            var initComp = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "InitializeComponent")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .WithBody(SyntaxFactory.Block(
                    GenerateControlInstantiations(controls)
                    .Concat(GenerateControlProperties(controls))
                    .Concat(GenerateFormProperties(controls)) // Add form setup at the end
                ));

            // Generate field declarations
            var fields = controls.Select(c => 
                SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(c.Type))
                    .AddVariables(SyntaxFactory.VariableDeclarator(c.Name)))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            );

            var classDecl = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword), SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(fields.ToArray())
                .AddMembers(initComp);

            return SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Windows.Forms")))
                .AddMembers(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(ns))
                    .AddMembers(classDecl));
        }

        private IEnumerable<StatementSyntax> GenerateControlInstantiations(List<ControlData> controls)
        {
            foreach (var c in controls)
            {
                yield return SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(c.Name),
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(c.Type))
                            .WithArgumentList(SyntaxFactory.ArgumentList())));
            }
        }

        private IEnumerable<StatementSyntax> GenerateControlProperties(List<ControlData> controls)
        {
            foreach (var c in controls)
            {
                // SuspendLayout logic could go here
                
                // Set Name
                yield return SetProp(c.Name, "Name", SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(c.Name)));

                // Set standard props (Left, Top, Width, Height) -> Location, Size
                if (c.Properties.ContainsKey("Left") && c.Properties.ContainsKey("Top"))
                {
                    int x = (int)(int.Parse(c.Properties["Left"]) / TwipsPerPixelX);
                    int y = (int)(int.Parse(c.Properties["Top"]) / TwipsPerPixelY);
                    yield return SetProp(c.Name, "Location", 
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Drawing.Point"))
                        .AddArgumentListArguments(
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(x))),
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(y)))));
                }

                if (c.Properties.ContainsKey("Width") && c.Properties.ContainsKey("Height"))
                {
                    int w = (int)(int.Parse(c.Properties["Width"]) / TwipsPerPixelX);
                    int h = (int)(int.Parse(c.Properties["Height"]) / TwipsPerPixelY);
                    yield return SetProp(c.Name, "Size", 
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Drawing.Size"))
                        .AddArgumentListArguments(
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(w))),
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(h)))));
                }

                // Caption -> Text
                if (c.Properties.ContainsKey("Caption"))
                {
                    yield return SetProp(c.Name, "Text", SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(c.Properties["Caption"].Trim('"'))));
                }
                
                // Add to Controls collection (assuming flat list for now, ideally strictly hierarchical)
                 yield return SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName("Controls")),
                            SyntaxFactory.IdentifierName("Add")))
                        .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(c.Name))));
            }
        }

        private IEnumerable<StatementSyntax> GenerateFormProperties(List<ControlData> controls)
        {
            // This is just a placeholder to ensure the form itself is set up
             yield return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName("AutoScaleDimensions")),
                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Drawing.SizeF"))
                        .AddArgumentListArguments(
                             SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(6F))),
                             SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(13F)))
                        )));
        }

        private StatementSyntax SetProp(string obj, string prop, ExpressionSyntax value)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(obj),
                        SyntaxFactory.IdentifierName(prop)),
                    value));
        }

        private List<ControlData> ParseVb6Form(string path)
        {
            var controls = new List<ControlData>();
            if (!File.Exists(path)) return controls;
            
            var lines = File.ReadAllLines(path);
            var controlStack = new Stack<ControlData>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Begin "))
                {
                    var match = Regex.Match(trimmed, @"Begin\s+(\w+\.\w+)\s+(\w+)");
                    if (match.Success)
                    {
                        var type = MapType(match.Groups[1].Value);
                        var name = match.Groups[2].Value;
                        var cd = new ControlData { Type = type, Name = name };
                        controls.Add(cd);
                        controlStack.Push(cd);
                    }
                }
                else if (trimmed == "End")
                {
                    if (controlStack.Count > 0) controlStack.Pop();
                }
                else if (controlStack.Count > 0 && trimmed.Contains("="))
                {
                    var parts = trimmed.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        controlStack.Peek().Properties[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
            return controls;
        }

        private string MapType(string vb6Type)
        {
            return vb6Type switch
            {
                "VB.CommandButton" => "System.Windows.Forms.Button",
                "VB.TextBox" => "System.Windows.Forms.TextBox",
                "VB.Label" => "System.Windows.Forms.Label",
                "VB.CheckBox" => "System.Windows.Forms.CheckBox",
                "VB.Form" => "System.Windows.Forms.Form",
                _ => "System.Windows.Forms.Control"
            };
        }

        private class ControlData
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
        }
    }
}

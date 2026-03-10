using System;
using System.IO;
using System.Linq;
using Vb6FormParser.Parser;
using Xunit;

namespace BLML.Tests;

public class Phase3FormsUiTests
{
    [Fact]
    public void ParseForms_ShouldParseControlsFromSingleForm()
    {
        var rootFolder = CreateTempFolder();

        try
        {
            var formPath = Path.Combine(rootFolder, "MainForm.frm");
            File.WriteAllText(formPath, """
                VERSION 5.00
                Object = "{6B7E6392-850A-101B-AFC0-4210102A8DA7}#1.0#0"; "MSCOMCTL.OCX"
                Begin VB.Form MainForm
                   Begin VB.CommandButton Command1
                      Caption = "Run"
                   End
                   Begin MSComctlLib.TreeView TreeView1
                   End
                End
                """);

            var controls = FormParser.ParseForms(rootFolder);

            Assert.Contains(controls, c =>
                c.FormFileName == "MainForm.frm" &&
                c.ControlType == "VB.CommandButton" &&
                c.ControlName == "Command1" &&
                c.Guid == "{6B7E6392-850A-101B-AFC0-4210102A8DA7}");

            Assert.Contains(controls, c =>
                c.FormFileName == "MainForm.frm" &&
                c.ControlType == "MSComctlLib.TreeView" &&
                c.ControlName == "TreeView1" &&
                c.Guid == "{6B7E6392-850A-101B-AFC0-4210102A8DA7}");
        }
        finally
        {
            Directory.Delete(rootFolder, recursive: true);
        }
    }

    [Fact]
    public void ParseForms_ShouldParseFrmFilesRecursively()
    {
        var rootFolder = CreateTempFolder();

        try
        {
            File.WriteAllText(Path.Combine(rootFolder, "RootForm.frm"), """
                VERSION 5.00
                Begin VB.Form RootForm
                   Begin VB.Label Label1
                   End
                End
                """);

            var nestedFolder = Path.Combine(rootFolder, "Nested");
            Directory.CreateDirectory(nestedFolder);
            File.WriteAllText(Path.Combine(nestedFolder, "ChildForm.frm"), """
                VERSION 5.00
                Begin VB.Form ChildForm
                   Begin VB.TextBox Text1
                   End
                End
                """);

            var controls = FormParser.ParseForms(rootFolder);

            Assert.Contains(controls, c => c.FormFileName == "RootForm.frm" && c.ControlName == "Label1");
            Assert.Contains(controls, c => c.FormFileName == "ChildForm.frm" && c.ControlName == "Text1");
        }
        finally
        {
            Directory.Delete(rootFolder, recursive: true);
        }
    }

    [Fact]
    public void ParseForms_ShouldLeaveGuidNullWhenFormHasNoObjectReference()
    {
        var rootFolder = CreateTempFolder();

        try
        {
            File.WriteAllText(Path.Combine(rootFolder, "SimpleForm.frm"), """
                VERSION 5.00
                Begin VB.Form SimpleForm
                   Begin VB.TextBox Text1
                   End
                End
                """);

            var controls = FormParser.ParseForms(rootFolder);
            var textBox = Assert.Single(controls.Where(c => c.ControlName == "Text1"));

            Assert.Null(textBox.Guid);
            Assert.Equal("VB.TextBox", textBox.ControlType);
            Assert.Equal("SimpleForm.frm", textBox.FormFileName);
        }
        finally
        {
            Directory.Delete(rootFolder, recursive: true);
        }
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

using System;
using System.IO;
using BLML.Phase3FormsUI.ControlMapping;
using BLML.Phase3FormsUI.FormParsing;

if (args.Length == 0)
{
    Console.WriteLine("Usage: BLML.Phase3.Tools <command> [args]");
    Console.WriteLine("Commands:");
    Console.WriteLine("  frm-parse <input.frm> <output.frmx>");
    Console.WriteLine("  form-codegen <input.frm> <output.cs>");
    Console.WriteLine("  activex-codegen <input.frm> <output.cs>");
    Console.WriteLine("  tablelayout <inputDesigner> <outputDesigner> <inputResx> <outputResx>");
    return;
}

switch (args[0].ToLowerInvariant())
{
    case "frm-parse" when args.Length >= 3:
        FrmParser.ParseAndConvertToCSharp(args[1], args[2]);
        break;

    case "form-codegen" when args.Length >= 3:
        File.WriteAllText(args[2], Vb6FormCodeGenerator.ConvertToCSharp(File.ReadAllText(args[1])));
        break;

    case "activex-codegen" when args.Length >= 3:
        var activeXGenerator = new ActiveXFormCodeGenerator();
        File.WriteAllText(args[2], activeXGenerator.ConvertToCSharp(File.ReadAllText(args[1])));
        break;

    case "tablelayout" when args.Length >= 5:
        var converter = new WinFormsTableLayoutConverter();
        converter.ConvertFiles(args[1], args[2], args[3], args[4]);
        break;

    default:
        Console.WriteLine("Invalid command or arguments.");
        break;
}

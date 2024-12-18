using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class VB6ToCSharpPreparer
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: VB6ToCSharpPreparer <folder_path>");
            return;
        }

        string folderPath = args[0];
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("Error: Specified folder does not exist.");
            return;
        }

        string outputFilePath = Path.Combine(folderPath, @"CombinedVB6CodeForCSharp.cs");
        var vb6Files = Directory.GetFiles(folderPath, "*.bas")
            .Concat(Directory.GetFiles(folderPath, "*.cls"))
            .Concat(Directory.GetFiles(folderPath, "*.frm"))
            .ToArray();

        List<string> processedLines = new List<string>();

        foreach (string file in vb6Files)
        {
            Console.WriteLine($"Processing: {file}");
            var lines = File.ReadAllLines(file);
            processedLines.Add($"// File: {Path.GetFileName(file)}");
            processedLines.AddRange(ProcessVB6Lines(lines));
            processedLines.Add(""); // Add a blank line between files.
        }

        File.WriteAllLines(outputFilePath, processedLines);
        Console.WriteLine($"Processing complete. Output written to {outputFilePath}");
    }

    static IEnumerable<string> ProcessVB6Lines(string[] lines)
    {
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Remove comments (VB6 comments start with ' or Rem).
            if (trimmedLine.StartsWith("'") || trimmedLine.StartsWith("Rem ", StringComparison.OrdinalIgnoreCase))
                continue;

            // Replace "GoSub" with a method call (add method definitions manually).
            trimmedLine = Regex.Replace(trimmedLine, "\bGoSub\b (\w+)", "Call$1()", RegexOptions.IgnoreCase);

            // Replace "Dim variable As Type" with "Dim variable As Object" to handle type ambiguities.
            trimmedLine = Regex.Replace(trimmedLine, "Dim (\w+) As (\w+)", "Dim $1 As Object", RegexOptions.IgnoreCase);

            // Mark lines for manual review if they contain VB6-specific constructs.
            if (Regex.IsMatch(trimmedLine, "\bOn Error\b", RegexOptions.IgnoreCase))
                trimmedLine = "// TODO: Review error handling: " + trimmedLine;

            if (Regex.IsMatch(trimmedLine, "\bCall\b", RegexOptions.IgnoreCase))
                trimmedLine = "// TODO: Ensure Call statement compatibility: " + trimmedLine;

            // Convert "End Sub" or "End Function" to braces as placeholders for C# syntax.
            trimmedLine = trimmedLine.Replace("End Sub", "}")
                                     .Replace("End Function", "}");

            // Replace "Sub" with "void" (placeholder transformation).
            trimmedLine = Regex.Replace(trimmedLine, "\bSub\b (\w+)", "void $1()", RegexOptions.IgnoreCase);

            // Replace "Function" with a placeholder "object" return type.
            trimmedLine = Regex.Replace(trimmedLine, "\bFunction\b (\w+)", "object $1()", RegexOptions.IgnoreCase);

            yield return trimmedLine;
        }
    }
}

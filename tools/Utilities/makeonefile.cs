using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        string rootDirectory = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        string outputFilePath = Path.Combine(rootDirectory, "CombinedClasses.cs");

        var usingStatements = new HashSet<string>();
        var classDefinitions = new List<string>();

        foreach (var file in Directory.EnumerateFiles(rootDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var lines = File.ReadAllLines(file);
            var classContent = new List<string>();
            bool inClass = false;

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("using "))
                {
                    usingStatements.Add(line.Trim());
                }
                else if (line.Trim().StartsWith("class ") || line.Trim().StartsWith("public class ") || line.Trim().StartsWith("private class ") || line.Trim().StartsWith("internal class "))
                {
                    inClass = true;
                }

                if (inClass)
                {
                    classContent.Add(line);
                }
            }

            if (classContent.Count > 0)
            {
                classDefinitions.AddRange(classContent);
            }
        }

        using (var writer = new StreamWriter(outputFilePath))
        {
            foreach (var usingStatement in usingStatements)
            {
                writer.WriteLine(usingStatement);
            }

            writer.WriteLine();

            foreach (var classDefinition in classDefinitions)
            {
                writer.WriteLine(classDefinition);
            }
        }

        Console.WriteLine($"Combined classes have been written to {outputFilePath}");
    }
}

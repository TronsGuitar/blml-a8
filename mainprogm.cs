using System;
using System.Linq;
using Vb6FormParser.Parser;

namespace Vb6FormParser
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Define paths
            var rootFolder = @"C:\path\to\vb6\forms";
            var outputCsvAll = @"C:\temp\AllControls.csv";
            var outputCsvSingle = @"C:\temp\SingleControl.csv";
            var outputCsProj = @"C:\temp\GeneratedProject.csproj";

            // 2. Parse .frm files
            var allControls = FormParser.ParseForms(rootFolder);

            // 3. Write out to CSV
            CsvWriter.WriteAllControlsCsv(allControls, outputCsvAll);

            // 4. Write a single control to CSV (for example, the first one)
            var singleControl = allControls.FirstOrDefault();
            CsvWriter.WriteSingleControlCsv(singleControl, outputCsvSingle);

            // 5. Generate a sample .csproj referencing discovered GUIDs
            CsProjGenerator.GenerateCsProj(allControls, outputCsProj);

            Console.WriteLine("Parsing complete! CSVs and .csproj generated.");
        }
    }
}

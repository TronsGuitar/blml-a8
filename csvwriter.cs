using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vb6FormParser.Models;

namespace Vb6FormParser.Parser
{
    public static class CsvWriter
    {
        /// <summary>
        /// Writes a list of Vb6ControlInfo to a CSV file.
        /// </summary>
        /// <param name="controls">List of Vb6ControlInfo</param>
        /// <param name="outputCsvPath">Path for CSV output</param>
        public static void WriteAllControlsCsv(List<Vb6ControlInfo> controls, string outputCsvPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("FormFileName,ControlType,ControlName,Guid");

            foreach (var c in controls)
            {
                sb.AppendLine($"{c.FormFileName},{c.ControlType},{c.ControlName},{c.Guid}");
            }

            File.WriteAllText(outputCsvPath, sb.ToString());
        }

        /// <summary>
        /// Writes a single control (for example, the first or last) to another CSV.
        /// </summary>
        /// <param name="control">A single Vb6ControlInfo</param>
        /// <param name="outputCsvPath">Path for the single-control CSV</param>
        public static void WriteSingleControlCsv(Vb6ControlInfo control, string outputCsvPath)
        {
            if (control == null)
            {
                // Just create an empty CSV or handle as needed
                File.WriteAllText(outputCsvPath, "FormFileName,ControlType,ControlName,Guid\n");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("FormFileName,ControlType,ControlName,Guid");
            sb.AppendLine($"{control.FormFileName},{control.ControlType},{control.ControlName},{control.Guid}");

            File.WriteAllText(outputCsvPath, sb.ToString());
        }
    }
}

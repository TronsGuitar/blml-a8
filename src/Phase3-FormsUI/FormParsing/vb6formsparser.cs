using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Vb6FormParser.Models;

namespace Vb6FormParser.Parser
{
    public static class FormParser
    {
        // Regex patterns to detect lines of interest in .frm
        private static readonly Regex BeginControlRegex = new Regex(
            @"^Begin\s+(?<ControlType>[\w\.]+)\s+(?<ControlName>\w+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // A typical line might look like:
        // Object = "{6B7E6392-850A-101B-AFC0-4210102A8DA7}#1.0#0"; "MSCOMCTL.OCX"
        // Or sometimes just the GUID in curly braces
        private static readonly Regex GuidRegex = new Regex(
            @"\{(?<Guid>[0-9A-F-]{36})\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Parses all .frm files in the specified root folder (recursively).
        /// Returns a list of Vb6ControlInfo objects with discovered control info.
        /// </summary>
        /// <param name="rootFolder">Root folder to search for .frm files</param>
        /// <returns>List of Vb6ControlInfo</returns>
        public static List<Vb6ControlInfo> ParseForms(string rootFolder)
        {
            var allControls = new List<Vb6ControlInfo>();
            
            // Get all .frm files in the root folder + subfolders
            var frmFiles = Directory.GetFiles(rootFolder, "*.frm", SearchOption.AllDirectories);

            foreach (var frmFilePath in frmFiles)
            {
                var fileName = Path.GetFileName(frmFilePath);
                var lines = File.ReadAllLines(frmFilePath);

                // We might store a "currentGUID" as we parse
                // if the "Object = {GUID}" line typically applies to all 
                // subsequent controls. In some VB6 forms, the references 
                // appear at the top.
                string currentGuid = null;

                foreach (var line in lines)
                {
                    // Check for an Object GUID line
                    var guidMatch = GuidRegex.Match(line);
                    if (guidMatch.Success)
                    {
                        currentGuid = "{" + guidMatch.Groups["Guid"].Value + "}";
                    }

                    // Check for control definition lines
                    var beginMatch = BeginControlRegex.Match(line.Trim());
                    if (beginMatch.Success)
                    {
                        allControls.Add(new Vb6ControlInfo
                        {
                            FormFileName = fileName,
                            ControlType = beginMatch.Groups["ControlType"].Value,
                            ControlName = beginMatch.Groups["ControlName"].Value,
                            Guid = currentGuid
                        });
                    }
                }
            }

            return allControls;
        }
    }
}

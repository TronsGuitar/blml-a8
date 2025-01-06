using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Vb6FormParser.Models;

namespace Vb6FormParser.Parser
{
    public static class CsProjGenerator
    {
        /// <summary>
        /// Generates a minimal .csproj file for .NET 6 or .NET Framework (depending on needs).
        /// This example assumes .NET 6 or .NET 7 for VS 2022. Adjust as needed.
        /// </summary>
        /// <param name="controls">List of all discovered controls</param>
        /// <param name="outputCsProjPath">Where to write the .csproj file</param>
        public static void GenerateCsProj(List<Vb6ControlInfo> controls, string outputCsProjPath)
        {
            // Mapping from known GUID to a reference name (fake data for demo).
            // In real use, you’d map each known GUID to the relevant COM library name
            var knownGuidMap = new Dictionary<string, string>
            {
                { "{6B7E6392-850A-101B-AFC0-4210102A8DA7}", "MSCOMCTL" },
                { "{0002E55F-0000-0000-C000-000000000046}", "VBIDE" },
                // Add more as needed...
            };

            // Distill the unique set of GUIDs from the discovered controls
            var uniqueGuids = new HashSet<string>(
                controls
                .Where(c => !string.IsNullOrWhiteSpace(c.Guid))
                .Select(c => c.Guid));

            // Create a new XDocument representing a .csproj
            var project = new XElement("Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"));

            // Add property group (TargetFramework etc.)
            var propertyGroup = new XElement("PropertyGroup",
                new XElement("TargetFramework", "net7.0"),       // for VS 2022
                new XElement("OutputType", "Library"),
                new XElement("RootNamespace", "Vb6FormParser")
            );

            project.Add(propertyGroup);

            // Build the <ItemGroup> with COM references if found
            var itemGroup = new XElement("ItemGroup");

            foreach (var guid in uniqueGuids)
            {
                if (knownGuidMap.TryGetValue(guid, out string comRefName))
                {
                    var comReference = new XElement("COMReference",
                        new XAttribute("Include", comRefName),
                        new XElement("Guid", guid),
                        new XElement("VersionMajor", "1"),
                        new XElement("VersionMinor", "0"),
                        new XElement("WrapperTool", "tlbimp")
                    );
                    itemGroup.Add(comReference);
                }
                else
                {
                    // If we don’t know the GUID -> we might skip or add a placeholder
                    // so you can manually fill in details later.
                    var unknownRef = new XElement("COMReference",
                        new XAttribute("Include", "UnknownControl_" + guid),
                        new XElement("Guid", guid),
                        new XElement("VersionMajor", "1"),
                        new XElement("VersionMinor", "0"),
                        new XElement("WrapperTool", "tlbimp")
                    );
                    itemGroup.Add(unknownRef);
                }
            }

            project.Add(itemGroup);

            // Wrap it in the Project
            var doc = new XDocument(project);

            // Save to file
            doc.Save(outputCsProjPath);
        }
    }
}

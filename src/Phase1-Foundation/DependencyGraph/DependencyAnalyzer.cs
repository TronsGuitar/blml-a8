using System;
using System.Collections.Generic;
using System.Linq;
using BLML.Phase1Foundation.ProjectModel; // Ensure this namespace is available

namespace BLML.Phase1Foundation.DependencyGraph
{
    public class DependencyAnalyzer
    {
        public class DependencyNode
        {
            public string Name { get; set; }
            public string FilePath { get; set; }
            public ComponentType Type { get; set; }
            // Identify dependencies
            public HashSet<string> References { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public List<string> DependsOn { get; } = new List<string>();
        }

        public enum ComponentType
        {
            Form,
            Module,
            Class,
            UserControl,
            Reference
        }

        /* TODO: Implementation Logic
         * 1. Build a Directed Acyclic Graph (DAG) of project components.
         * 2. Track cross-module calls and global variable usage.
         * 3. Identify circular dependencies that may need refactoring for C#.
         * 4. Determine the optimal order for conversion and compilation.
         * 5. Visualize the dependency graph (optional).
         */

        public Dictionary<string, DependencyNode> BuildDependencyGraph(VB6Project project)
        {
            var nodes = new Dictionary<string, DependencyNode>(StringComparer.OrdinalIgnoreCase);

            // Add Forms, Modules, Classes to the graph
            foreach (var form in project.Forms)
            {
                var node = CreateNode(form, ComponentType.Form);
                nodes[node.Name] = node;
            }
            foreach (var module in project.Modules)
            {
                 var node = CreateNode(module, ComponentType.Module);
                 nodes[node.Name] = node;
            }
            foreach (var cls in project.Classes)
            {
                 var node = CreateNode(cls, ComponentType.Class);
                 nodes[node.Name] = node;
            }
             foreach (var ctl in project.UserControls)
            {
                 var node = CreateNode(ctl, ComponentType.UserControl);
                 nodes[node.Name] = node;
            }
            
            // Add References
            foreach(var refLib in project.References)
            {
                 var node = new DependencyNode 
                 { 
                     Name = refLib.Description ?? refLib.Guid, 
                     Type = ComponentType.Reference,
                     FilePath = refLib.Path
                 };
                 if(!nodes.ContainsKey(node.Name))
                    nodes[node.Name] = node;
            }

            // TODO: In a real implementation, we would parse each file content here to find references 
            // to other components and populate the DependsOn list.
            
            return nodes;
        }

        private DependencyNode CreateNode(string entry, ComponentType type)
        {
            // entry is typically the file path relative to the project, e.g., "Module1.bas"
            string name = System.IO.Path.GetFileNameWithoutExtension(entry);
            return new DependencyNode
            {
                Name = name,
                FilePath = entry,
                Type = type
            };
        }

        public List<string> Metadata_GetTopologicalSort(Dictionary<string, DependencyNode> graph)
        {
            // Placeholder for topological sort to determine compilation order
            // This naively returns the list as is for now
            return graph.Keys.ToList();
        }

        public List<string> DetectCircularDependencies(Dictionary<string, DependencyNode> graph)
        {
            // Placeholder for circular dependency detection
            return new List<string>();
        }
    }
}

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
            // Topological sort using Kahn's algorithm
            var sorted = new List<string>();
            var inDegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in graph.Values)
            {
                inDegree[node.Name] = 0;
            }
            foreach (var node in graph.Values)
            {
                foreach (var dep in node.DependsOn)
                {
                    if (inDegree.ContainsKey(dep))
                        inDegree[dep]++;
                }
            }
            var queue = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            while (queue.Count > 0)
            {
                var name = queue.Dequeue();
                sorted.Add(name);
                foreach (var dep in graph[name].DependsOn)
                {
                    if (inDegree.ContainsKey(dep))
                    {
                        inDegree[dep]--;
                        if (inDegree[dep] == 0)
                            queue.Enqueue(dep);
                    }
                }
            }
            // If not all nodes are sorted, there is a cycle
            if (sorted.Count != graph.Count)
                return new List<string>(); // Cycle detected
            return sorted;
        }

        public List<string> DetectCircularDependencies(Dictionary<string, DependencyNode> graph)
        {
            // Detect cycles using DFS
            var cycles = new List<string>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Visit(string node)
            {
                if (stack.Contains(node))
                {
                    cycles.Add(node);
                    return;
                }
                if (visited.Contains(node)) return;
                visited.Add(node);
                stack.Add(node);
                foreach (var dep in graph[node].DependsOn)
                {
                    if (graph.ContainsKey(dep))
                        Visit(dep);
                }
                stack.Remove(node);
            }
            foreach (var node in graph.Keys)
            {
                Visit(node);
            }
            return cycles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}

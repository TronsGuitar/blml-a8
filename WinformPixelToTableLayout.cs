using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WinFormPixelToTableLayout
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example usage: 
            // WinFormPixelToTableLayout.exe "MyForm.Designer.cs" "MyForm_TLP.Designer.cs" "MyForm.resx" "MyForm_TLP.resx"
            
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: WinFormPixelToTableLayout <inputDesigner> <outputDesigner> <inputResx> <outputResx>");
                return;
            }

            string inputDesigner = args[0];
            string outputDesigner = args[1];
            string inputResx = args[2];
            string outputResx = args[3];

            if (!File.Exists(inputDesigner))
            {
                Console.WriteLine("Designer file not found: " + inputDesigner);
                return;
            }

            if (!File.Exists(inputResx))
            {
                Console.WriteLine("Resx file not found: " + inputResx);
                return;
            }

            try
            {
                // Read original designer lines
                var lines = File.ReadAllLines(inputDesigner).ToList();

                // Extract control data
                var controls = ExtractControls(lines);

                // Build new designer lines
                var newDesignerLines = RebuildWithTableLayout(lines, controls);

                // Write out new file
                File.WriteAllLines(outputDesigner, newDesignerLines);

                // Copy resx
                File.Copy(inputResx, outputResx, true);

                // Optional: run a basic runtime check
                // This code compiles the new form at different sizes and checks if all controls are visible
                // Realistically, you might integrate this test differently
                RuntimeCheckTestForm(outputDesigner);

                Console.WriteLine("Conversion complete. New files saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        // Holds minimal control data
        class ControlData
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        static List<ControlData> ExtractControls(List<string> lines)
        {
            var controls = new List<ControlData>();
            // Regex for control instantiation: this.myControl = new System.Windows.Forms.Button();
            var createRegex = new Regex(@"this\.(?<name>\w+)\s*=\s*new\s*System\.Windows\.Forms\.(?<type>\w+)\s*\(\)");
            // Regex for location: this.myControl.Location = new System.Drawing.Point(x, y);
            var locRegex = new Regex(@"this\.(?<name>\w+)\.Location\s*=\s*new\s*System\.Drawing\.Point\((?<x>\d+),\s*(?<y>\d+)\)");
            // Regex for size: this.myControl.Size = new System.Drawing.Size(width, height);
            var sizeRegex = new Regex(@"this\.(?<name>\w+)\.Size\s*=\s*new\s*System\.Drawing\.Size\((?<width>\d+),\s*(?<height>\d+)\)");

            var controlTemp = new Dictionary<string, ControlData>();

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                var matchCreate = createRegex.Match(line);
                if (matchCreate.Success)
                {
                    var cData = new ControlData
                    {
                        Name = matchCreate.Groups["name"].Value,
                        Type = matchCreate.Groups["type"].Value,
                    };
                    controlTemp[cData.Name] = cData;
                }

                var matchLoc = locRegex.Match(line);
                if (matchLoc.Success)
                {
                    var name = matchLoc.Groups["name"].Value;
                    if (controlTemp.ContainsKey(name))
                    {
                        controlTemp[name].X = int.Parse(matchLoc.Groups["x"].Value);
                        controlTemp[name].Y = int.Parse(matchLoc.Groups["y"].Value);
                    }
                }

                var matchSize = sizeRegex.Match(line);
                if (matchSize.Success)
                {
                    var name = matchSize.Groups["name"].Value;
                    if (controlTemp.ContainsKey(name))
                    {
                        controlTemp[name].Width = int.Parse(matchSize.Groups["width"].Value);
                        controlTemp[name].Height = int.Parse(matchSize.Groups["height"].Value);
                    }
                }
            }

            // Finalize list
            controls = controlTemp.Values.ToList();
            return controls;
        }

        static List<string> RebuildWithTableLayout(List<string> lines, List<ControlData> controls)
        {
            // Locate InitializeComponent method boundaries
            int startIndex = -1, endIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("void InitializeComponent()"))
                    startIndex = i;
                
                if (startIndex != -1 && lines[i].Contains("}"))
                {
                    endIndex = i;
                    break;
                }
            }

            if (startIndex == -1 || endIndex == -1)
                return lines;

            var initMethod = lines.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();

            // Insert code for new TableLayoutPanel
            // We'll call it tableLayoutPanelMain
            var newTableLayoutCode = new List<string>();
            newTableLayoutCode.Add("this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();");

            // We'll remove all original location/size lines for each control
            var locationRegex = new Regex(@"\.Location\s*=");
            var sizeRegex = new Regex(@"\.Size\s*=");
            var newInitMethod = new List<string>();

            foreach (var line in initMethod)
            {
                if (locationRegex.IsMatch(line) || sizeRegex.IsMatch(line))
                {
                    // Skip them
                    continue;
                }
                else
                {
                    newInitMethod.Add(line);
                }
            }

            // Add the tableLayoutPanel to the form's controls
            // We can do it near the end of InitializeComponent
            int insertIndex = newInitMethod.FindIndex(x => x.Contains("// 
this.Controls"));
            if (insertIndex < 0)
                insertIndex = newInitMethod.Count;

            // We'll figure out row counts. One approach: each control in its own row.
            // Or group by Y coordinate. This example puts each control in its own row.
            int rowCount = controls.Count;
            newTableLayoutCode.Add($"this.tableLayoutPanelMain.ColumnCount = 1;");
            newTableLayoutCode.Add($"this.tableLayoutPanelMain.RowCount = {rowCount};");
            newTableLayoutCode.Add("this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;");

            // For each row, set its size as an equal percentage
            // (100 / rowCount) for each row
            for (int i = 0; i < rowCount; i++)
            {
                double pct = 100.0 / rowCount;
                newTableLayoutCode.Add($"this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, {pct}F));");
            }
            // Single column is 100%
            newTableLayoutCode.Add("this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));");

            // Place controls in the rows
            // Adjust size for listbox if needed
            for (int i = 0; i < controls.Count; i++)
            {
                var ctrl = controls[i];
                // Example: this.tableLayoutPanelMain.Controls.Add(this.myControl, 0, i);
                newTableLayoutCode.Add($"this.tableLayoutPanelMain.Controls.Add(this.{ctrl.Name}, 0, {i});");
                // If it's a ListBox, shrink it a bit
                if (ctrl.Type.Equals("ListBox", StringComparison.OrdinalIgnoreCase))
                {
                    newTableLayoutCode.Add($"this.{ctrl.Name}.Dock = System.Windows.Forms.DockStyle.Fill;");
                }
                else
                {
                    newTableLayoutCode.Add($"this.{ctrl.Name}.Dock = System.Windows.Forms.DockStyle.Fill;");
                }
            }

            // Add code that adds the tableLayoutPanel to the form
            newTableLayoutCode.Add("this.Controls.Add(this.tableLayoutPanelMain);");

            // Insert into the newInitMethod
            newInitMethod.InsertRange(insertIndex, newTableLayoutCode);

            // Insert the tableLayoutPanelMain declaration at the top
            // Right after "private System.Windows.Forms..."
            int declIndex = lines.FindIndex(x => x.Contains("private System.ComponentModel.IContainer components"));
            if (declIndex == -1)
                declIndex = lines.FindIndex(x => x.Contains("private"));

            if (declIndex != -1)
            {
                lines.Insert(declIndex + 1, "private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;");
            }

            // Now replace the old method with the new one
            var result = new List<string>();
            // Copy everything up to startIndex
            for (int i = 0; i < startIndex; i++)
            {
                result.Add(lines[i]);
            }
            // Insert new initMethod lines
            result.AddRange(newInitMethod);
            // Copy everything after endIndex
            for (int i = endIndex + 1; i < lines.Count; i++)
            {
                result.Add(lines[i]);
            }

            return result;
        }

        // Dummy function to illustrate a runtime test approach
        static void RuntimeCheckTestForm(string newDesignerPath)
        {
            // A real approach might compile the new form dynamically,
            // then set multiple sizes: 800x600, 1024x768, etc.
            // Checking if all controls are visible would involve reflection.
            Console.WriteLine("Runtime check is a placeholder here.");
        }
    }
}

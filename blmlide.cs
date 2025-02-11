using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace BLMLDebugger
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private ToolStripButton btnStart, btnStop, btnPause, btnStep;
        private SplitContainer splitContainer;
        private RichTextBox codeEditor;
        private TabControl bottomTabs;
        private TabPage tabCommand;
        private TabPage tabWatch;
        private TextBox txtCommandWindow;
        private ListView listWatch;
        private Timer executionTimer;
        private List<int> breakpoints = new List<int>();
        private List<string> codeLines = new List<string>();
        private int currentLine = 0;
        private bool isPaused = false;

        public MainForm()
        {
            this.Text = "BLML Debugger";
            this.Size = new Size(1000, 700);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Set up the menu at the top
            menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += OpenItem_Click;
            fileMenu.DropDownItems.Add(openItem);
            menuStrip.Items.Add(fileMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Set up the toolbar below the menu
            toolStrip = new ToolStrip();
            btnStart = new ToolStripButton("Start");
            btnStart.Click += BtnStart_Click;
            btnStop = new ToolStripButton("Stop");
            btnStop.Click += BtnStop_Click;
            btnPause = new ToolStripButton("Pause");
            btnPause.Click += BtnPause_Click;
            btnStep = new ToolStripButton("Step");
            btnStep.Click += BtnStep_Click;
            toolStrip.Items.Add(btnStart);
            toolStrip.Items.Add(btnStop);
            toolStrip.Items.Add(btnPause);
            toolStrip.Items.Add(btnStep);
            toolStrip.Location = new Point(0, menuStrip.Height);
            this.Controls.Add(toolStrip);

            // Set up the split container to divide the main area and the bottom area
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            // Use horizontal split: top for the code editor, bottom for command window and watch list
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterDistance = this.ClientSize.Height - 200;
            splitContainer.Panel1MinSize = 300;
            this.Controls.Add(splitContainer);

            // Code editor on Panel1
            codeEditor = new RichTextBox();
            codeEditor.Dock = DockStyle.Fill;
            codeEditor.Font = new Font("Consolas", 10);
            // Add a context menu for toggling breakpoints
            var contextMenu = new ContextMenuStrip();
            var toggleBreakpointItem = new ToolStripMenuItem("Toggle Breakpoint");
            toggleBreakpointItem.Click += ToggleBreakpointItem_Click;
            contextMenu.Items.Add(toggleBreakpointItem);
            codeEditor.ContextMenuStrip = contextMenu;
            splitContainer.Panel1.Controls.Add(codeEditor);

            // Set up the tab control on Panel2 for command window and watch list
            bottomTabs = new TabControl();
            bottomTabs.Dock = DockStyle.Fill;

            tabCommand = new TabPage("Command Window");
            txtCommandWindow = new TextBox();
            txtCommandWindow.Multiline = true;
            txtCommandWindow.Dock = DockStyle.Fill;
            txtCommandWindow.ScrollBars = ScrollBars.Both;
            txtCommandWindow.Font = new Font("Consolas", 10);
            txtCommandWindow.ReadOnly = true;
            tabCommand.Controls.Add(txtCommandWindow);

            tabWatch = new TabPage("Watch List");
            listWatch = new ListView();
            listWatch.Dock = DockStyle.Fill;
            listWatch.View = View.Details;
            listWatch.Columns.Add("Variable", 150);
            listWatch.Columns.Add("Value", 150);
            tabWatch.Controls.Add(listWatch);

            bottomTabs.TabPages.Add(tabCommand);
            bottomTabs.TabPages.Add(tabWatch);
            splitContainer.Panel2.Controls.Add(bottomTabs);

            // Timer to simulate code execution
            executionTimer = new Timer();
            executionTimer.Interval = 1000; // one second per line
            executionTimer.Tick += ExecutionTimer_Tick;
        }

        private void OpenItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BLML Files|*.blml|All Files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string code = System.IO.File.ReadAllText(ofd.FileName);
                codeEditor.Text = code;
                // Reset breakpoints and current line
                breakpoints.Clear();
                codeLines = new List<string>(code.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
                currentLine = 0;
                AppendCommandText("Loaded file: " + ofd.FileName);
            }
        }

        private void AppendCommandText(string text)
        {
            txtCommandWindow.AppendText(text + Environment.NewLine);
        }

        private void ToggleBreakpointItem_Click(object sender, EventArgs e)
        {
            int line = codeEditor.GetLineFromCharIndex(codeEditor.SelectionStart);
            if (breakpoints.Contains(line))
            {
                breakpoints.Remove(line);
                AppendCommandText("Removed breakpoint at line " + (line + 1));
            }
            else
            {
                breakpoints.Add(line);
                AppendCommandText("Set breakpoint at line " + (line + 1));
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            // Load code lines if not already loaded
            if (codeLines.Count == 0)
            {
                codeLines = new List<string>(codeEditor.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
            }
            currentLine = 0;
            isPaused = false;
            executionTimer.Start();
            AppendCommandText("Execution started");
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            executionTimer.Stop();
            currentLine = 0;
            isPaused = false;
            ClearHighlights();
            AppendCommandText("Execution stopped");
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            isPaused = true;
            executionTimer.Stop();
            AppendCommandText("Execution paused");
        }

        private void BtnStep_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                executionTimer.Stop();
                isPaused = true;
            }
            ExecuteNextLine();
        }

        private void ExecutionTimer_Tick(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                ExecuteNextLine();
            }
        }

        private void ExecuteNextLine()
        {
            if (currentLine >= codeLines.Count)
            {
                executionTimer.Stop();
                AppendCommandText("Execution finished");
                return;
            }

            // Highlight the current line in the editor
            HighlightLine(currentLine);

            // Simulate executing the current line
            string lineText = codeLines[currentLine].Trim();
            AppendCommandText("Executing line " + (currentLine + 1) + ": " + lineText);
            // Update the watch list with some dummy variables
            UpdateWatchList(currentLine);

            // If a breakpoint is set on this line, pause execution
            if (breakpoints.Contains(currentLine))
            {
                AppendCommandText("Hit breakpoint at line " + (currentLine + 1));
                isPaused = true;
                executionTimer.Stop();
                currentLine++;
                return;
            }

            currentLine++;
        }

        private void UpdateWatchList(int lineNumber)
        {
            // Clear previous values
            listWatch.Items.Clear();

            // Dummy local variables for simulation; in a real app, these would reflect actual values
            var variables = new List<(string name, string value)>
            {
                ("line", lineNumber.ToString()),
                ("dummyVar", (lineNumber * 2).ToString()),
                ("status", (lineNumber % 2 == 0) ? "even" : "odd")
            };

            // Preset watches
            variables.Add(("presetWatch1", "value1"));
            variables.Add(("presetWatch2", "value2"));

            foreach (var variable in variables)
            {
                var item = new ListViewItem(variable.name);
                item.SubItems.Add(variable.value);
                listWatch.Items.Add(item);
            }
        }

        private void HighlightLine(int lineIndex)
        {
            ClearHighlights();
            int start = codeEditor.GetFirstCharIndexFromLine(lineIndex);
            if (start < 0) return;
            int length = codeEditor.Lines[lineIndex].Length;
            codeEditor.Select(start, length);
            codeEditor.SelectionBackColor = Color.Yellow;
            codeEditor.ScrollToCaret();
        }

        private void ClearHighlights()
        {
            int selStart = codeEditor.SelectionStart;
            int selLength = codeEditor.SelectionLength;
            codeEditor.SelectAll();
            codeEditor.SelectionBackColor = Color.White;
            codeEditor.Select(selStart, selLength);
        }
    }
}

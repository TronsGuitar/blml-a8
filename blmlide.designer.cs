// MainForm.Designer.cs
namespace BLMLDebugger
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnStart;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.ToolStripButton btnPause;
        private System.Windows.Forms.ToolStripButton btnStep;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.RichTextBox codeEditor;
        private System.Windows.Forms.ContextMenuStrip codeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem toggleBreakpointItem;
        private System.Windows.Forms.TabControl bottomTabs;
        private System.Windows.Forms.TabPage tabCommand;
        private System.Windows.Forms.TabPage tabWatch;
        private System.Windows.Forms.TextBox txtCommandWindow;
        private System.Windows.Forms.ListView listWatch;
        private System.Windows.Forms.ColumnHeader columnVariable;
        private System.Windows.Forms.ColumnHeader columnValue;

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Build the UI components.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnStart = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.btnPause = new System.Windows.Forms.ToolStripButton();
            this.btnStep = new System.Windows.Forms.ToolStripButton();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.codeEditor = new System.Windows.Forms.RichTextBox();
            this.codeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toggleBreakpointItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bottomTabs = new System.Windows.Forms.TabControl();
            this.tabCommand = new System.Windows.Forms.TabPage();
            this.txtCommandWindow = new System.Windows.Forms.TextBox();
            this.tabWatch = new System.Windows.Forms.TabPage();
            this.listWatch = new System.Windows.Forms.ListView();
            this.columnVariable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));

            // 
            // menuStrip
            // 
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(800, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // toolStrip
            // 
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(800, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip";
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnStart,
            this.btnStop,
            this.btnPause,
            this.btnStep});
            // 
            // btnStart
            // 
            this.btnStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(35, 22);
            this.btnStart.Text = "Start";
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(35, 22);
            this.btnStop.Text = "Stop";
            // 
            // btnPause
            // 
            this.btnPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(40, 22);
            this.btnPause.Text = "Pause";
            // 
            // btnStep
            // 
            this.btnStep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnStep.Name = "btnStep";
            this.btnStep.Size = new System.Drawing.Size(35, 22);
            this.btnStep.Text = "Step";
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 49);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.codeEditor);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.bottomTabs);
            this.splitContainer.Size = new System.Drawing.Size(800, 401);
            this.splitContainer.SplitterDistance = 300;
            this.splitContainer.TabIndex = 2;
            // 
            // codeEditor
            // 
            this.codeEditor.ContextMenuStrip = this.codeContextMenu;
            this.codeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeEditor.Font = new System.Drawing.Font("Consolas", 10F);
            this.codeEditor.Location = new System.Drawing.Point(0, 0);
            this.codeEditor.Name = "codeEditor";
            this.codeEditor.Size = new System.Drawing.Size(800, 300);
            this.codeEditor.TabIndex = 0;
            this.codeEditor.Text = "";
            // 
            // codeContextMenu
            // 
            this.codeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleBreakpointItem});
            this.codeContextMenu.Name = "codeContextMenu";
            this.codeContextMenu.Size = new System.Drawing.Size(165, 26);
            // 
            // toggleBreakpointItem
            // 
            this.toggleBreakpointItem.Name = "toggleBreakpointItem";
            this.toggleBreakpointItem.Size = new System.Drawing.Size(164, 22);
            this.toggleBreakpointItem.Text = "Toggle Breakpoint";
            // 
            // bottomTabs
            // 
            this.bottomTabs.Controls.Add(this.tabCommand);
            this.bottomTabs.Controls.Add(this.tabWatch);
            this.bottomTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomTabs.Location = new System.Drawing.Point(0, 0);
            this.bottomTabs.Name = "bottomTabs";
            this.bottomTabs.SelectedIndex = 0;
            this.bottomTabs.Size = new System.Drawing.Size(800, 97);
            this.bottomTabs.TabIndex = 0;
            // 
            // tabCommand
            // 
            this.tabCommand.Controls.Add(this.txtCommandWindow);
            this.tabCommand.Location = new System.Drawing.Point(4, 22);
            this.tabCommand.Name = "tabCommand";
            this.tabCommand.Padding = new System.Windows.Forms.Padding(3);
            this.tabCommand.Size = new System.Drawing.Size(792, 71);
            this.tabCommand.TabIndex = 0;
            this.tabCommand.Text = "Command Window";
            this.tabCommand.UseVisualStyleBackColor = true;
            // 
            // txtCommandWindow
            // 
            this.txtCommandWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCommandWindow.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtCommandWindow.Location = new System.Drawing.Point(3, 3);
            this.txtCommandWindow.Multiline = true;
            this.txtCommandWindow.Name = "txtCommandWindow";
            this.txtCommandWindow.ReadOnly = true;
            this.txtCommandWindow.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCommandWindow.Size = new System.Drawing.Size(786, 65);
            this.txtCommandWindow.TabIndex = 0;
            // 
            // tabWatch
            // 
            this.tabWatch.Controls.Add(this.listWatch);
            this.tabWatch.Location = new System.Drawing.Point(4, 22);
            this.tabWatch.Name = "tabWatch";
            this.tabWatch.Padding = new System.Windows.Forms.Padding(3);
            this.tabWatch.Size = new System.Drawing.Size(792, 71);
            this.tabWatch.TabIndex = 1;
            this.tabWatch.Text = "Watch List";
            this.tabWatch.UseVisualStyleBackColor = true;
            // 
            // listWatch
            // 
            this.listWatch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnVariable,
            this.columnValue});
            this.listWatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listWatch.FullRowSelect = true;
            this.listWatch.GridLines = true;
            this.listWatch.HideSelection = false;
            this.listWatch.Location = new System.Drawing.Point(3, 3);
            this.listWatch.Name = "listWatch";
            this.listWatch.Size = new System.Drawing.Size(786, 65);
            this.listWatch.TabIndex = 0;
            this.listWatch.UseCompatibleStateImageBehavior = false;
            this.listWatch.View = System.Windows.Forms.View.Details;
            // 
            // columnVariable
            // 
            this.columnVariable.Text = "Variable";
            this.columnVariable.Width = 150;
            // 
            // columnValue
            // 
            this.columnValue.Text = "Value";
            this.columnValue.Width = 150;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "BLML Debugger";
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.bottomTabs.ResumeLayout(false);
            this.tabCommand.ResumeLayout(false);
            this.tabCommand.PerformLayout();
            this.tabWatch.ResumeLayout(false);
            this.codeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}

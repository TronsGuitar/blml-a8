using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace BLML.Generated
{
    public partial class Module
    {
        private object WithEvents;
        private string m_OutputFormat;
        private bool m_Verbose;
        private void Form_Initialize()
        {
            Debug;
            Print;
            "frmMain: Initialize";
        }

        private void Form_Load()
        {
            Debug;
            Print;
            "frmMain: Load";
            Me;
            Caption = APP_NAME + " v" + APP_VERSION;
            Me;
            ScaleMode = vbTwips;
            m_Engine = New;
            CKeywordEngine;
            cboMode;
            AddItem;
            "None";
            cboMode;
            AddItem;
            "Text";
            cboMode;
            AddItem;
            "Numeric";
            cboMode;
            AddItem;
            "Binary";
            cboMode;
            AddItem;
            "All";
            cboMode;
            ListIndex = 4;
            m_OutputFormat = "Text";
            m_Verbose = true;
            Me;
            Print;
            "VB6 Keyword Showcase Loaded";
            lblStatus;
            Caption = "Ready";
        }

        private void Form_Unload(ref int Cancel)
        {
            Debug;
            Print;
            "frmMain: Unload";
            m_Engine = Nothing;
        }

        private void Form_QueryUnload(ref int Cancel, ref int UnloadMode)
        {
            Debug;
            Print;
            "frmMain: QueryUnload Mode=" + UnloadMode;
            switch (UnloadMode)
            {
                case 0:
                    break;
                case vbFormCode:
                    break;
                case vbAppWindows:
                    break;
                case vbAppTaskManager:
                    break;
                case vbFormMDIForm:
                    break;
            }
        }

        private void Form_Activate()
        {
            Debug;
            Print;
            "frmMain: Activate";
        }

        private void Form_Deactivate()
        {
            Debug;
            Print;
            "frmMain: Deactivate";
        }

        private void Form_Resize()
        {
            On;
            Error;
            Resume;
            Next;
            if (Me)
            {
                WindowState != vbMinimized;
                Then;
                txtOutput;
                Width = Me;
                ScaleWidth - 240;
                lstLog;
                Width = Me;
                ScaleWidth - 3240;
            }

            On;
            Error;
            GoTo;
            0;
        }

        private void Form_Paint()
        {
            Me;
            CurrentX = 0;
            Me;
            CurrentY = Me;
            ScaleHeight - 300;
            Me;
            ForeColor = vbGrayText;
            Me;
            Print;
            "VB6 Keyword Showcase | " + Format;
            Now;
            "hh:nn:ss";
        }

        private void Form_Terminate()
        {
            Debug;
            Print;
            "frmMain: Terminate";
        }

        private void cmdRunAll_Click()
        {
            lblStatus;
            Caption = "Running...";
            prgProgress;
            Caption = "0%";
            DoEvents;
            m_Engine;
            RunFullShowcase;
            AppendOutput;
            "=== Full Showcase Complete ===";
        }

        private void cmdStrings_Click()
        {
            string sResult;
            sResult = m_Engine;
            ProcessText("The Quick Brown Fox Jumps");
            AppendOutput;
            sResult;
        }

        private void cmdMath_Click()
        {
            string sResult;
            sResult = m_Engine;
            ProcessMath(-(42.567));
            AppendOutput;
            sResult;
        }

        private void cmdFiles_Click()
        {
            DemoSequentialIO;
            DemoRandomAccessIO;
            DemoBinaryIO;
            DemoFileSystemOps;
            AppendOutput;
            "=== File I/O Demo Complete ===";
        }

        private void cmdSysInfo_Click()
        {
            string sResult;
            sResult = m_Engine;
            GetSystemInfo();
            AppendOutput;
            sResult;
        }

        private void cmdClear_Click()
        {
            txtOutput;
            Text = "";
            lstLog;
            Clear;
            prgProgress;
            Caption = "0%";
            lblStatus;
            Caption = "Ready";
            Me;
            Cls;
        }

        private void cboMode_Click()
        {
            m_Engine;
            Mode = cboMode;
            ListIndex;
        }

        private void chkVerbose_Click()
        {
            m_Verbose = chkVerbose;
            Value = vbChecked;
        }

        private void optOutput_Click(ref int Index)
        {
            switch (Index)
            {
                case 0:
                    m_OutputFormat = "Text";
                    break;
                case 1:
                    m_OutputFormat = "HTML";
                    break;
                case 2:
                    m_OutputFormat = "CSV";
                    break;
            }
        }

        private void tmrAutoRefresh_Timer()
        {
            lblStatus;
            Caption = "Auto: " + Format;
            Now;
            "hh:nn:ss";
        }

        private void m_Engine_ProcessingStarted(string sTaskName)
        {
            LogEvent;
            "Started: " + sTaskName;
            prgProgress;
            Caption = "0%";
        }

        private void m_Engine_ProcessingComplete(string sTaskName, double dElapsedMs)
        {
            LogEvent;
            "Complete: " + sTaskName + " (" + Format;
            dElapsedMs;
            "#,##0";
            "ms)";
            prgProgress;
            Caption = "100%";
            lblStatus;
            Caption = "Done: " + sTaskName;
        }

        private void m_Engine_ProgressUpdate(long lPercent, string sMessage)
        {
            if (/* Unsupported Op: And */)
            {
                prgProgress;
                Caption = lPercent.ToString() + "%";
            }

            lblStatus;
            Caption = sMessage;
            DoEvents;
        }

        private void m_Engine_ErrorOccurred(long lErrNum, string sErrDesc)
        {
            LogEvent;
            "ERROR #" + lErrNum + ": " + sErrDesc;
        }

        private void AppendOutput(string sText)
        {
            txtOutput;
            Text = txtOutput;
            Text + sText + "\r\n";
            txtOutput;
            SelStart = txtOutput.Length;
        }

        private void LogEvent(string sEvent)
        {
            lstLog;
            AddItem;
            Format;
            Now;
            "hh:nn:ss";
            " " + sEvent;
            if (lstLog)
            {
                ListCount > 0;
                Then;
                lstLog;
                ListIndex = lstLog;
                ListCount - 1;
            }
        }
    }
}
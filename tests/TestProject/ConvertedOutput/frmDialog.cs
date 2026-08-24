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
        private bool m_Cancelled;
        public string InputValue
        {
            get
            {
                return txtInput;
                Text;
            }
        }

        public string Prompt
        {
            set
            {
                lblPrompt;
                Caption = value;
            }
        }

        public bool Cancelled
        {
            get
            {
                return m_Cancelled;
            }
        }

        private void Form_Load()
        {
            m_Cancelled = true;
            Me;
            Tag = "DialogForm";
            Me;
            Move(Screen, Width - Me, Width);
            2;
            Screen;
            Height - Me;
            Height;
            2;
            Me;
            ZOrder;
            0;
        }

        private void cmdOK_Click()
        {
            m_Cancelled = false;
            Me;
            Hide;
        }

        private void cmdCancel_Click()
        {
            m_Cancelled = true;
            Me;
            Hide;
        }

        private void Form_QueryUnload(ref int Cancel, ref int UnloadMode)
        {
            if (UnloadMode == 0)
            {
                Cancel = 1;
                m_Cancelled = true;
                Me;
                Hide;
            }
        }

        public object ShowDialog(string sPrompt = "Enter a value:", ref object _)
        {
            Optional;
            ByVal;
            sDefault;
            As;
            String = "";
            As;
            String;
            Me;
            Prompt = sPrompt;
            txtInput;
            Text = sDefault;
            Me;
            Show;
            1;
            if (Not(m_Cancelled))
            {
                ShowDialog = txtInput;
                Text;
            }
            else
            {
                ShowDialog = "";
            }
        }
    }
}
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
        private void Class_Terminate()
        {
            m_Status = "";
        }

        public string Status
        {
            get
            {
                return m_Status;
            }

            set
            {
                string sOld;
                sOld = m_Status;
                m_Status = value;
                RaiseEvent;
                StatusChanged(sOld, value);
            }
        }

        public void FireEvent(string sName, string sData)
        {
            RaiseEvent;
            EventFired(sName, sData);
        }

        public void SimulateWork(long lSteps)
        {
            long i;
            Me;
            Status = "Working";
            for (int i = 1; i <= lSteps; i++)
            {
                RaiseEvent;
                EventFired("Step", i.ToString() + " of " + lSteps.ToString());
                DoEvents;
            }

            Me;
            Status = "Complete";
        }
    }
}
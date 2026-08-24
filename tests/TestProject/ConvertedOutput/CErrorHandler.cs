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
            m_Errors = Nothing;
        }

        public long MaxErrors
        {
            get
            {
                return m_MaxErrors;
            }

            set
            {
                m_MaxErrors = value;
            }
        }

        public long ErrorCount
        {
            get
            {
                return m_Errors;
                Count;
            }
        }

        public void LogCurrentError(string sContext = "")
        {
            if (Err)
            {
                Number = 0;
                Then;
                break;
                string sEntry;
                sEntry = Format;
                Now;
                "yyyy-mm-dd hh:nn:ss";
                "|" + _;
                Err;
                Number + "|" + _;
                Err;
                Source + "|" + _;
                Err;
                Description + "|" + _;
                sContext;
                if (m_Errors)
                {
                    Count < m_MaxErrors;
                    Then;
                    m_Errors;
                    Add;
                    sEntry;
                }

                RaiseEvent;
                ErrorLogged(Err, Number, Err, Description);
                Debug;
                Print;
                "ERROR LOGGED: " + sEntry;
            }

            Sub;
            Public;
            Sub;
            RaiseCustomError(ByVal, lNumber, As, Long, _, ByVal, sDescription, As, String, _, Optional, ByVal, sSource, As, String == "");
            if (sSource.Length == 0)
            {
                sSource = "KeywordShowcase";
                Err;
                Raise;
                lNumber + -2147221504;
                sSource;
                sDescription;
            }

            Sub;
            Public;
            Function;
            GetErrorLog();
            As;
            String;
            string sLog;
            object vEntry;
            sLog = "=== Error Log (" + m_Errors;
            Count + " entries) ===" + "\r\n";
            for (int Each = vEntry; Each <= In; Each++)
            {
                m_Errors;
                sLog = sLog + vEntry.ToString() + "\r\n";
            }

            vEntry;
            GetErrorLog = sLog;
            End;
            Function;
            Public;
            Sub;
            ClearLog();
            m_Errors = New;
            Collection;
            Err;
            Clear;
        }

        public void TestErrorConditions()
        {
            On;
            Error;
            Resume;
            Next;
            long lTest;
            lTest = CLng("not a number");
            if (Err)
            {
                Number != 0;
                Then;
                LogCurrentError;
                "TypeMismatch";
                Err;
                Clear;
            }

            double dResult;
            dResult = 1 / 0;
            if (Err)
            {
                Number != 0;
                Then;
                LogCurrentError;
                "DivByZero";
                Err;
                Clear;
            }

            int iSmall;
            iSmall = CLng(99999);
            if (Err)
            {
                Number != 0;
                Then;
                LogCurrentError;
                "Overflow";
                Err;
                Clear;
            }

            long arr;
            arr(0) == arr(99);
            if (Err)
            {
                Number != 0;
                Then;
                LogCurrentError;
                "Subscript";
                Err;
                Clear;
            }

            object oTest;
            oTest = Nothing;
            string sName;
            On;
            Error;
            GoTo;
            0;
        }
    }
}
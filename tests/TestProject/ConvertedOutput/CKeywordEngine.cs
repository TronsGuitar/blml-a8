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
            m_Items = Nothing;
            m_ErrorHandler = Nothing;
            m_FileWorker = Nothing;
            m_EventRaiser = Nothing;
            m_Initialized = false;
            TerminateGlobals;
            Debug;
            Print;
            "CKeywordEngine terminated at " + Now;
        }

        public string Name
        {
            get
            {
                return m_Name;
            }

            set
            {
                if (value.Length > 0)
                {
                    m_Name = value;
                }
            }
        }

        public ProcessingMode Mode
        {
            get
            {
                return m_Mode;
            }

            set
            {
                m_Mode = value;
            }
        }

        public long ItemCount
        {
            get
            {
                return m_Items;
                Count;
            }
        }

        public bool Initialized
        {
            get
            {
                return m_Initialized;
            }
        }

        public CErrorHandler ErrorHandler
        {
            get
            {
                return m_ErrorHandler;
            }

            set
            {
                m_ErrorHandler = value;
            }
        }

        internal string GetInternalState()
        {
            GetInternalState = "Name=" + m_Name + " Mode=" + m_Mode + _;
            " Items=" + m_Items;
            Count + " Init=" + m_Initialized;
        }

        internal void ResetInternal()
        {
            m_Items = New;
            Collection;
            m_Mode = pmNone;
        }

        public void RunFullShowcase()
        {
            long lStartTick;
            lStartTick = GetTickCount();
            RaiseEvent;
            ProcessingStarted("FullShowcase");
            Debug;
            Print;
            "Running showcase on: " + Me;
            Name;
            RaiseEvent;
            ProgressUpdate(10, "Demonstrating data types...");
            Call;
            DemonstrateAllDataTypes;
            RaiseEvent;
            ProgressUpdate(20, "String operations...");
            string sStrResult;
            sStrResult = DemoStringFunctions("Hello VB6 World");
            RaiseEvent;
            ProgressUpdate(30, "Math operations...");
            string sMathResult;
            sMathResult = DemoMathFunctions(-(42.567));
            RaiseEvent;
            ProgressUpdate(40, "Date/Time functions...");
            string sDateResult;
            sDateResult = DemoDateTimeFunctions();
            RaiseEvent;
            ProgressUpdate(50, "Flow control...");
            Call;
            DemonstrateLoops;
            Call;
            DemoGoSubReturn;
            Call;
            DemoWithBlock;
            RaiseEvent;
            ProgressUpdate(60, "File I/O...");
            Call;
            DemoSequentialIO;
            Call;
            DemoRandomAccessIO;
            Call;
            DemoBinaryIO;
            RaiseEvent;
            ProgressUpdate(70, "Error handling...");
            Call;
            DemoErrorHandling;
            RaiseEvent;
            ProgressUpdate(80, "Logical operations...");
            string sLogical;
            sLogical = DemoLogicalOps(true, false);
            RaiseEvent;
            ProgressUpdate(90, "Environment info...");
            string sEnv;
            sEnv = DemoEnvironment();
            RaiseEvent;
            ProgressUpdate(95, "WinAPI calls...");
            string sSysInfo;
            sSysInfo = GetSystemInfo();
            double dElapsed;
            dElapsed = GetElapsedMs(lStartTick);
            RaiseEvent;
            ProgressUpdate(100, "Complete!");
            RaiseEvent;
            ProcessingComplete("FullShowcase", dElapsed);
        }

        public string GetSystemInfo()
        {
            string sResult;
            sResult = "=== System Information ===" + "\r\n" + _;
            "Computer: " + MWinAPI;
            GetComputerName() + "\r\n" + _;
            "User: " + MWinAPI;
            GetUserName() + "\r\n" + _;
            "Screen: " + GetScreenWidth() + "x" + GetScreenHeight() + "\r\n" + _;
            "OS Version: " + GetOSVersion() + "\r\n" + _;
            "Temp Path: " + MWinAPI;
            GetTempPath() + "\r\n" + _;
            "App: " + App;
            Title + " v" + App;
            Major + "." + App;
            Minor + "." + App;
            Revision;
            GetSystemInfo = sResult;
        }

        public void AddItem(object vItem, string sKey = "")
        {
            On;
            Error;
            Resume;
            Next;
            if (sKey.Length > 0)
            {
                m_Items;
                Add;
                vItem;
                sKey;
            }
            else
            {
                m_Items;
                Add;
                vItem;
            }

            On;
            Error;
            GoTo;
            0;
        }

        public object GetItem(object vIndex)
        {
            On;
            Error;
            Resume;
            Next;
            if (IsObject(m_Items(vIndex)))
            {
                GetItem = m_Items(vIndex);
            }
            else
            {
                GetItem = m_Items(vIndex);
            }

            On;
            Error;
            GoTo;
            0;
        }

        public void oveItem(ByVal vIndex As Variant)()
        {
            On;
            Error;
            Resume;
            Next;
            m_Items;
            On;
            Error;
            GoTo;
            0;
        }

        public string ProcessText(string sInput)
        {
            RaiseEvent;
            ProcessingStarted("ProcessText");
            long lStart;
            lStart = GetTickCount();
            string sResult;
            sResult = DemoStringFunctions(sInput) + "\r\n" + _;
            "Split/Join: " + DemoSplitJoin("apple, banana, cherry, date");
            ProcessText = sResult;
            RaiseEvent;
            ProcessingComplete("ProcessText", GetElapsedMs(lStart));
        }

        public string ProcessMath(double dValue)
        {
            RaiseEvent;
            ProcessingStarted("ProcessMath");
            long lStart;
            lStart = GetTickCount();
            string sResult;
            sResult = DemoMathFunctions(dValue) + "\r\n" + _;
            DemoArithmetic(dValue, 7) + "\r\n" + _;
            DemoDateTimeFunctions();
            ProcessMath = sResult;
            RaiseEvent;
            ProcessingComplete("ProcessMath", GetElapsedMs(lStart));
        }

        private string CDataProcessor_ProcessData(string sInput)
        {
            CDataProcessor_ProcessData = Me;
            ProcessText(sInput);
        }

        private void CDataProcessor_Initialize(string sConfig)
        {
            Me;
            Name = sConfig;
        }

        private string CDataProcessor_Status
        {
            get
            {
                return IIf(m_Initialized, "Ready", "Not Initialized");
            }
        }

        private void m_EventRaiser_EventFired(string sEventName, string sData)
        {
            Debug;
            Print;
            "Engine received event: " + sEventName + " Data: " + sData;
        }

        public string SelfDescribe()
        {
            if (TypeOf)
            {
                Me;
                Is;
                CKeywordEngine;
                Then;
                SelfDescribe = "I am a CKeywordEngine: " + Me;
                Name;
            }

            object oTest;
            oTest = Nothing;
            if (oTest)
            {
                Is;
                Nothing;
                Then;
                SelfDescribe = SelfDescribe + " (test object is Nothing)";
            }
        }
    }
}
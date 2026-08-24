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
        public object Enum;
        public object Enum;
        public object Enum;
        public string g_AppName;
        public string g_Version;
        public bool g_Initialized;
        public ProcessingMode g_Mode;
        private bool m_InternalFlag;
        private long m_ErrorCount;
        private object m_ModuleVariant;
        public object Const;
        public object Const;
        public object Const;
        public object Const;
        public object Const;
        private object Const;
        public long GetNextID()
        {
            long lNextID;
            lNextID = lNextID + 1;
            GetNextID = lNextID;
        }

        public void InitializeGlobals()
        {
            g_AppName = APP_NAME;
            g_Version = APP_VERSION;
            g_Initialized = true;
            g_Mode = pmAll;
            g_LegacyCounter = 0;
            m_InternalFlag = false;
            m_ErrorCount = 0;
            m_ModuleVariant = Empty;
            Randomize;
            Timer;
        }

        public void TerminateGlobals()
        {
            g_Initialized = false;
            g_Mode = pmNone;
            m_ModuleVariant = Nothing;
        }

        public void DemonstrateAllDataTypes()
        {
            bool bFlag;
            Byte yByte;
            int iSmall;
            long lBig;
            float fFloat;
            double dPrecise;
            Currency cMoney;
            DateTime dtNow;
            string sText;
            string sFixed;
            50;
            object vAnything;
            object oGeneric;
            object iSuffix;
            object lSuffix;
            object fSuffix;
            object dSuffix;
            object cSuffix;
            object sSuffix;
            bFlag = true;
            yByte = 255;
            iSmall = 32767;
            lBig = 2147483647;
            fFloat = 3.14;
            dPrecise = 3.14159265358979;
            cMoney = 1234.5678;
            dtNow = Now;
            sText = "Hello World";
            sFixed = "Fixed Length";
            vAnything = "I am a Variant";
            oGeneric = Nothing;
            long lFromBool;
            lFromBool = CLng(bFlag);
            bool bFromInt;
            bFromInt = CBool(1);
            Byte yFromInt;
            yFromInt = CByte(/* Unsupported Op: Mod */);
            int iFromLong;
            iFromLong = Convert.ToInt32( /* Unsupported Op: Mod */);
            float fFromDbl;
            fFromDbl = Convert.ToSingle(dPrecise);
            double dFromStr;
            dFromStr = Convert.ToDouble("123.456");
            Currency cFromDbl;
            cFromDbl = CCur(dPrecise);
            DateTime dtFromStr;
            dtFromStr = CDate("2024-01-15");
            string sFromNum;
            sFromNum = lBig.ToString();
            object vFromStr;
            vFromStr = CVar(sText);
            object vDec;
            vDec = CDec("12345678901234567890.1234");
            long arrFixed;
            double arrMulti;
            string arrDynamic;
            ;
            arrDynamic(0) == "First";
            arrDynamic(1) == "Second";
            ;
            arrDynamic(5) == "Sixth";
            long lLow;
            long lHigh;
            lLow = LBound(arrDynamic);
            lHigh = UBound(arrDynamic);
            Erase;
            arrFixed;
            Erase;
            arrMulti;
            vAnything = Empty;
            vAnything = Null;
            oGeneric = Nothing;
            sText = null;
            if (oGeneric)
            {
                Is;
                Nothing;
                Then;
            }

            if (VarType(vAnything) == 1)
            {
            }

            string sTypeName;
            sTypeName = TypeName(vAnything);
            bool bTests;
            vAnything = Empty;
            bTests(0) == IsEmpty(vAnything);
            vAnything = Null;
            bTests(1) == IsNull(vAnything);
            bTests(2) == double.TryParse("123", out _);
            bTests(3) == IsDate("1/1/2024");
            bTests(4) == IsObject(oGeneric);
            bTests(5) == IsArray(arrDynamic);
            bTests(6) == IsError(CVErr(1));
            Erase;
            arrDynamic;
        }

        public ProcessingMode AppMode
        {
            get
            {
                return g_Mode;
            }

            set
            {
                g_Mode = value;
            }
        }
    }
}
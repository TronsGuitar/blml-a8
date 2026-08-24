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
        public void DemoErrorHandling()
        {
            long lOrigErrNum;
            string sOrigErrDesc;
            On;
            Error;
            GoTo;
            ErrHandler;
            Err;
            Raise;
            9;
            "MFlowControl.DemoErrorHandling";
            "Subscript out of range demo";
            break;
            ErrHandler;
            lOrigErrNum = Err;
            Number;
            sOrigErrDesc = Err;
            Description;
            Debug;
            Print;
            "Error #" + lOrigErrNum + ": " + sOrigErrDesc;
            Debug;
            Print;
            "Source: " + Err;
            Source;
            Debug;
            Print;
            "Error$(11) = " + Error;
            11;
            Err;
            Clear;
            On;
            Error;
            Resume;
            Next;
            double dResult;
            dResult = 1 / 0;
            if (Err)
            {
                Number != 0;
                Then;
                Debug;
                Print;
                "Caught via Resume Next: " + Err;
                Description;
                Err;
                Clear;
            }

            On;
            Error;
            GoTo;
            0;
            object vErr;
            vErr = CVErr(2015);
            if (IsError(vErr))
            {
                Debug;
                Print;
                "Variant error value: " + vErr.ToString();
            }
        }

        public void DemoDoEvents(long lIterations)
        {
            long i;
            for (int i = 1; i <= lIterations; i++)
            {
                DoEvents;
            }
        }

        public string DemoEnvironment()
        {
            string sResult;
            sResult = "PATH=" + Left;
            Environ;
            "PATH";
            50;
            "..." + "\r\n";
            sResult = sResult + "TEMP=" + Environ;
            "TEMP" + "\r\n";
            sResult = sResult + "USERNAME=" + Environ;
            "USERNAME" + "\r\n";
            string sEnv;
            sEnv = Environ;
            1;
            sResult = sResult + "Environ(1)=" + sEnv + "\r\n";
            sResult = sResult + "Command$=" + Command;
            "\r\n";
            DemoEnvironment = sResult;
        }

        public void DemoShell()
        {
            long lTaskID;
            On;
            Error;
            Resume;
            Next;
            lTaskID = Shell("notepad.exe", vbNormalFocus);
            if (Err)
            {
                Number = /* Unsupported Op: And */;
                Then;
                AppActivate;
                lTaskID;
                Sleep;
                500;
                SendKeys;
                "Hello from VB6!{ENTER}";
                true;
                SendKeys;
                "%{F4}";
                true;
            }

            On;
            Error;
            GoTo;
            0;
        }

        public string DemoDialogs()
        {
            string sResult;
            long lResponse;
            lResponse = MsgBox("Continue processing?", _, vbYesNoCancel + vbQuestion + vbDefaultButton1, _, "Confirm");
            switch (lResponse)
            {
                case vbYes:
                    sResult = "User chose Yes";
                    break;
                case vbNo:
                    sResult = "User chose No";
                    break;
                case vbCancel:
                    sResult = "User chose Cancel";
                    break;
            }

            MsgBox;
            "Processing complete.";
            vbInformation;
            "Done";
            string sInput;
            sInput = InputBox("Enter a value:", "Input Required", "Default Value");
            sResult = sResult + " Input='" + sInput + "'";
            DemoDialogs = sResult;
        }

        public void DemoBeep()
        {
            Beep;
        }

        public void DemoLetKeyword()
        {
            string sValue;
            sValue = "Assigned with Let";
            long lValue;
            lValue = 42;
            Debug;
            Print;
            sValue + " " + lValue;
        }

        public void DemoPrintWrite()
        {
            Debug;
            Print;
            "Column1";
            Tab(20);
            "Column2";
            Tab(40);
            "Column3";
            Debug;
            Print;
            "Data1";
            Spc(5);
            "Data2";
            Spc(5);
            "Data3";
            Debug;
            Print;
            1;
            2;
            3;
            Debug;
            Print;
            1;
            2;
            3;
        }

        public void DemoFormOperations()
        {
            On;
            Error;
            Resume;
            Next;
            On;
            Error;
            GoTo;
            0;
        }

        public Collection DemoNewKeyword()
        {
            New col;
            Collection;
            Collection col2;
            col2 = New;
            Collection;
            col;
            Add;
            "Item1";
            "Key1";
            col;
            Add;
            "Item2";
            "Key2";
            DemoNewKeyword = col;
            col2 = Nothing;
        }

        public void DemoLateBinding()
        {
            On;
            Error;
            Resume;
            Next;
            object oDict;
            oDict = CreateObject("Scripting.Dictionary");
            if (Not(oDict))
            {
                Is;
                Nothing;
                Then;
                oDict;
                Add;
                "Key1";
                "Value1";
                oDict;
                Add;
                "Key2";
                "Value2";
                object vKey;
                for (int Each = vKey; Each <= In; Each++)
                {
                    oDict;
                    Keys;
                    Debug;
                    Print;
                    vKey + "=" + oDict(vKey);
                }

                vKey;
                oDict = Nothing;
            }

            On;
            Error;
            GoTo;
            0;
        }

        public string GetObjectTypeName(object obj)
        {
            if (obj)
            {
                Is;
                Nothing;
                Then;
                GetObjectTypeName = "Nothing";
                ElseIf;
                TypeOf;
                obj;
                Is;
                Collection;
                Then;
                GetObjectTypeName = "Collection";
                ElseIf;
                TypeOf;
                obj;
                Is;
                CKeywordEngine;
                Then;
                GetObjectTypeName = "CKeywordEngine";
            }
            else
            {
                GetObjectTypeName = TypeName(obj);
            }
        }
    }
}
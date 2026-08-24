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
        public void LogMessage(string sMessage, ref object _)
        {
            Optional;
            ByVal;
            eSeverity;
            As;
            Severity = sevInfo;
            _;
            Optional;
            ByVal;
            sSource;
            As;
            String = "System";
            string sFormatted;
            sFormatted = "[" + Format;
            Now;
            "yyyy-mm-dd hh:nn:ss";
            "] " + _;
            "[" + sSource + "] " + _;
            Choose(eSeverity + 1, "INFO", "WARN", "ERROR", "CRIT") + _;
            ": " + sMessage;
            Debug;
            Print;
            sFormatted;
            Debug;
            Assert;
            sFormatted.Length > 0;
        }

        public void LogMultiple(ref object ParamArray)
        {
            Messages();
            As;
            Variant;
            object vMsg;
            long idx;
            for (int Each = vMsg; Each <= In; Each++)
            {
                Messages;
                if (Not(IsMissing(vMsg)))
                {
                    Debug;
                    Print;
                    "  ParamArray(" + idx + "): " + vMsg.ToString();
                }

                idx = idx + 1;
            }

            vMsg;
        }

        public object AddAndReturn(ref long lAccumulator, ref object _)
        {
            ByVal;
            lAmount;
            As;
            Long;
            As;
            Long;
            lAccumulator = lAccumulator + lAmount;
            AddAndReturn = lAccumulator;
        }

        public object SafeDivide(double dNumerator, ref object _)
        {
            ByVal;
            dDenominator;
            As;
            Double;
            As;
            Variant;
            On;
            Error;
            GoTo;
            ErrorHandler;
            if (dDenominator == 0)
            {
                Then;
                GoTo;
                ZeroDivision;
            }

            SafeDivide = dNumerator / dDenominator;
            GoTo;
            CleanExit;
            ZeroDivision;
            SafeDivide = CVErr(11);
            GoTo;
            CleanExit;
            ErrorHandler;
            SafeDivide = CVErr(Err, Number);
            Resume;
            CleanExit;
            CleanExit;
            On;
            Error;
            Resume;
            Next;
            break;
        }

        public void DemoGoSubReturn()
        {
            long lValue;
            lValue = 10;
            GoSub;
            DoubleIt;
            Debug;
            Print;
            "After GoSub: " + lValue;
            break;
            DoubleIt;
            lValue = lValue * 2;
            Return;
        }

        public string ClassifyValue(object vValue)
        {
            switch (true)
            {
                case IsEmpty(vValue):
                    ClassifyValue = "Empty";
                    break;
                case IsNull(vValue):
                    ClassifyValue = "Null";
                    break;
                case double.TryParse(vValue, out _):
                    if (Convert.ToDouble(vValue) < 0)
                    {
                        ClassifyValue = "Negative";
                    }
                    else if (Convert.ToDouble(vValue) == 0)
                    {
                        ClassifyValue = "Zero";
                    }
                    else if (Convert.ToDouble(vValue) >= 1 && Convert.ToDouble(vValue) <= 10)
                    {
                        ClassifyValue = "Small";
                    }
                    else if (Convert.ToDouble(vValue) >= 11 && Convert.ToDouble(vValue) <= 100)
                    {
                        ClassifyValue = "Medium";
                    }
                    else if (Convert.ToDouble(vValue) >= 101 && Convert.ToDouble(vValue) <= 3000)
                    {
                        ClassifyValue = "Large or Special";
                    }
                    else if (Convert.ToDouble(vValue) > 1000)
                    {
                        ClassifyValue = "Huge";
                    }
                    else
                    {
                        ClassifyValue = "Other Numeric";
                    }

                    break;
                case IsDate(vValue):
                    ClassifyValue = "Date";
                    break;
                default:
                    ClassifyValue = "String or Other";
                    break;
            }
        }

        public void DemonstrateLoops()
        {
            long i;
            long j;
            long lSum;
            bool bDone;
            lSum = 0;
            for (int i = 1; i <= 10; i += 1)
            {
                lSum = lSum + i;
            }

            for (int i = 10; i <= 1; i += -(1))
            {
                Debug;
                Print;
                i;
            }

            New col;
            Collection;
            col;
            Add;
            "Alpha";
            col;
            Add;
            "Beta";
            col;
            Add;
            "Gamma";
            object vItem;
            for (int Each = vItem; Each <= In; Each++)
            {
                col;
                Debug;
                Print;
                vItem;
            }

            vItem;
            i = 0;
            while (i < 5)
            {
                i = i + 1;
            }

            i = 0;
            do
            {
                i = i + 1;
            }
            while (i < 5);
            i = 0;
            while (!(i >= 5))
            {
                i = i + 1;
            }

            i = 0;
            do
            {
                i = i + 1;
            }
            while (!(i >= 5));
            i = 0;
            while (i < 5)
            {
                i = i + 1;
            }

            for (int i = 1; i <= 100; i++)
            {
                if (i > 5)
                {
                    break;
                    Next;
                    i;
                    while (true)
                    {
                        if (bDone)
                        {
                            break;
                            bDone = true;
                            Loop;
                            for (int i = 1; i <= 3; i++)
                            {
                                for (int j = 1; j <= 3; j++)
                                {
                                    if (/* Unsupported Op: And */)
                                    {
                                        break;
                                        Next;
                                        j;
                                        Next;
                                        i;
                                        col = Nothing;
                                    }

                                    Sub;
                                    Public;
                                    Function;
                                    EvaluateGrade(ByVal, dScore, As, Double);
                                    As;
                                    String;
                                    if (dScore >= 90)
                                    {
                                        EvaluateGrade = "A";
                                        ElseIf;
                                        dScore >= 80;
                                        Then;
                                        EvaluateGrade = "B";
                                        ElseIf;
                                        dScore >= 70;
                                        Then;
                                        EvaluateGrade = "C";
                                        ElseIf;
                                        dScore >= 60;
                                        Then;
                                        EvaluateGrade = "D";
                                    }
                                    else
                                    {
                                        EvaluateGrade = "F";
                                    }

                                    if (dScore == 100)
                                    {
                                        EvaluateGrade = "A+";
                                    }
                                    else
                                    {
                                        if (dScore < 0)
                                        {
                                            EvaluateGrade = "Invalid";
                                        }

                                        Function;
                                        Public;
                                        Function;
                                        DemoLogicalOps(ByVal, bA, As, Boolean, ByVal, bB, As, Boolean);
                                        As;
                                        String;
                                        string sResult;
                                        sResult = "And=" + /* Unsupported Op: And */ + _;
                                        " Or=" + /* Unsupported Op: Or */ + _;
                                        " Not=" + Not(bA) + _;
                                        " Xor=" + bA;
                                        Xor;
                                        bB;
                                        _;
                                        " Eqv=" + bA;
                                        Eqv;
                                        bB;
                                        _;
                                        " Imp=" + bA;
                                        Imp;
                                        bB;
                                        DemoLogicalOps = sResult;
                                    }

                                    Function;
                                    Public;
                                    Function;
                                    DemoArithmetic(ByVal, a, As, Double, ByVal, b, As, Double);
                                    As;
                                    String;
                                    string sResult;
                                    if (b != 0)
                                    {
                                        sResult = "Add=" + a + b + _;
                                        " Sub=" + a - b + _;
                                        " Mul=" + a * b + _;
                                        " Div=" + a / b + _;
                                        " IntDiv=" + CLng(a);
                                        CLng(b);
                                        _;
                                        " Mod=" + /* Unsupported Op: Mod */ + _;
                                        " Pow=" + a;
                                        2;
                                    }
                                    else
                                    {
                                        sResult = "Cannot divide by zero";
                                    }

                                    string sFull;
                                    sFull = "Hello" + " " + "World";
                                    sFull = "Hello" + " " + "World";
                                    DemoArithmetic = sResult;
                                    End;
                                    Function;
                                    Public;
                                    Function;
                                    MatchesPattern(ByVal, sText, As, String, _, ByVal, sPattern, As, String);
                                    As;
                                    Boolean;
                                    MatchesPattern = sText;
                                    Like;
                                    sPattern;
                                    End;
                                    Function;
                                    Public;
                                    Sub;
                                    DemoWithBlock();
                                    RECT rct;
                                    With;
                                    rct;
                                    Left = 0;
                                    Top = 0;
                                    Right = 100;
                                    Bottom = 100;
                                    End;
                                    With;
                                    Debug;
                                    Print;
                                    "Rect: " + rct;
                                    Left + "," + rct;
                                    Top + "," + rct;
                                    Right + "," + rct;
                                    Bottom;
                                    End;
                                    Sub;
                                    Public;
                                    Function;
                                    GetCallbackPointer();
                                    As;
                                    Long;
                                    GetCallbackPointer = 0;
                                    End;
                                    Function;
                                    Public;
                                    Sub;
                                    TimerCallback(ByVal, hWnd, As, Long, ByVal, uMsg, As, Long, _, ByVal, idEvent, As, Long, ByVal, dwTime, As, Long);
                                    Debug;
                                    Print;
                                    "Timer callback fired at " + dwTime;
                                    End;
                                    Sub;
                                    Public;
                                    Sub;
                                    DemoDebugFeatures();
                                    long i;
                                    i = 42;
                                    Debug;
                                    Print;
                                    "Value is: " + i;
                                    Debug;
                                    Assert;
                                    i = 42;
                                    End;
                                    Sub;
                                    Public;
                                    Function;
                                    DemoInlineFunctions(ByVal, lValue, As, Long);
                                    As;
                                    String;
                                    string sResult;
                                    sResult = IIf(lValue > 0, "Positive", "Non-positive");
                                    if (/* Unsupported Op: And */)
                                    {
                                        sResult = sResult + " " + Choose(lValue, "One", "Two", "Three", "Four");
                                    }

                                    sResult = sResult + " " + Switch(_, lValue < 0, "Negative", _, lValue == 0, "Zero", _, lValue > 0, "Positive");
                                    DemoInlineFunctions = sResult;
                                    End;
                                    Function;
                                    Public;
                                    Sub;
                                    DemoComputedGoTo(ByVal, lChoice, As, Long);
                                    On;
                                    lChoice;
                                    GoTo;
                                    Label1;
                                    Label2;
                                    Label3;
                                    Debug;
                                    Print;
                                    "No match";
                                    break;
                                    Label1;
                                    Debug;
                                    Print;
                                    "Choice 1";
                                    break;
                                    Label2;
                                    Debug;
                                    Print;
                                    "Choice 2";
                                    break;
                                    Label3;
                                    Debug;
                                    Print;
                                    "Choice 3";
                                    break;
                                    End;
                                    Sub;
                                    Public;
                                    Sub;
                                    DemoComputedGoSub(ByVal, lChoice, As, Long);
                                    On;
                                    lChoice;
                                    GoSub;
                                    Sub1;
                                    Sub2;
                                    Sub3;
                                    break;
                                    Sub1;
                                    Debug;
                                    Print;
                                    "Subroutine 1";
                                    Return;
                                    Sub2;
                                    Debug;
                                    Print;
                                    "Subroutine 2";
                                    Return;
                                    Sub3;
                                    Debug;
                                    Print;
                                    "Subroutine 3";
                                    Return;
                                    End;
                                    Sub;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
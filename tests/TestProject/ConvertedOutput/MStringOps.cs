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
        public string DemoStringFunctions(string sInput)
        {
            string sResult;
            long lPos;
            Debug;
            Print;
            "Length: " + sInput.Length;
            if (sInput.Length >= 5)
            {
                sResult = Left;
                sInput;
                3;
                "|" + Right;
                sInput;
                3;
                "|" + Mid;
                sInput;
                2;
                3;
            }

            string sMutable;
            sMutable = "ABCDEFGH";
            Mid;
            sMutable;
            3;
            2;
            "XX";
            string sPadded;
            sPadded = "  Hello  ";
            Debug;
            Print;
            "Trim:  [" + Trim;
            sPadded + "]";
            Debug;
            Print;
            "LTrim: [" + LTrim;
            sPadded + "]";
            Debug;
            Print;
            "RTrim: [" + RTrim;
            sPadded + "]";
            Debug;
            Print;
            "Upper: " + UCase;
            sInput;
            Debug;
            Print;
            "Lower: " + LCase;
            sInput;
            string sSpaces;
            sSpaces = Space;
            10;
            string sRepeated;
            sRepeated = String;
            5;
            "*";
            sRepeated = String;
            5;
            65;
            lPos = 1.IndexOf(sInput) + 1;
            long lPosRev;
            lPosRev = InStrRev(sInput, "a", -(1), 1);
            string sReplaced;
            sReplaced = sInput.Replace(" ", "_");
            int iCmp;
            iCmp = StrComp("abc", "ABC", 1);
            iCmp = StrComp("abc", "ABC", 0);
            string sConverted;
            sConverted = StrConv(sInput, vbProperCase);
            sConverted = StrConv(sInput, vbUpperCase);
            sConverted = StrConv(sInput, vbLowerCase);
            sConverted = StrConv(sInput, vbUnicode);
            sConverted = StrConv(sConverted, vbFromUnicode);
            string sReversed;
            sReversed = StrReverse(sInput);
            if (sInput.Length > 0)
            {
                int iAscVal;
                iAscVal = Asc(sInput);
                Debug;
                Print;
                "Asc: " + iAscVal + " Chr: " + Chr;
                iAscVal;
                long lAscW;
                lAscW = AscW(sInput);
                Debug;
                Print;
                "AscW: " + lAscW + " ChrW: " + ChrW;
                lAscW;
            }

            double dVal;
            dVal = Val("  123.45abc");
            string sStr;
            sStr = Str;
            dVal;
            Debug;
            Print;
            "Hex: " + Hex;
            255;
            Debug;
            Print;
            "Oct: " + Oct;
            255;
            Debug;
            Print;
            "Formatted Number: " + Format;
            12345.6789;
            "#,##0.00";
            Debug;
            Print;
            "Formatted Date:   " + Format;
            Now;
            "yyyy-mm-dd hh:nn:ss";
            Debug;
            Print;
            "Formatted Pct:    " + Format;
            0.85;
            "0.00%";
            Debug;
            Print;
            "Scientific:       " + Format;
            12345.6789;
            "0.00E+00";
            DemoStringFunctions = "Input='" + sInput + "'" + "\r\n" + _;
            "Reversed='" + sReversed + "'" + "\r\n" + _;
            "Replaced='" + sReplaced + "'" + "\r\n" + _;
            "Mutable='" + sMutable + "'";
        }

        public string DemoSplitJoin(string sCsv)
        {
            string arr;
            arr = Split(sCsv, ",");
            long i;
            for (int i = LBound(arr); i <= UBound(arr); i++)
            {
                arr(i) == Trim;
                arr(i);
            }

            DemoSplitJoin = Join(arr, " | ");
        }

        public void DemoByteStringFunctions()
        {
            string sTest;
            sTest = "Hello";
            Debug;
            Print;
            "LenB: " + LenB(sTest);
            Debug;
            Print;
            "LeftB: " + LeftB;
            sTest;
            4;
            Debug;
            Print;
            "RightB: " + RightB;
            sTest;
            4;
            Debug;
            Print;
            "MidB: " + MidB;
            sTest;
            3;
            4;
        }
    }
}
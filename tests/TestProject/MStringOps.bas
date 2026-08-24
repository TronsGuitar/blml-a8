Attribute VB_Name = "MStringOps"
'==============================================================================
' MODULE: MStringOps
' PURPOSE: Demonstrates VB6 string functions/keywords:
'          Len, Left, Right, Mid, Mid$ (statement), Trim, LTrim, RTrim,
'          UCase, LCase, Space, String$, InStr, InStrRev, Replace,
'          StrComp, StrConv, StrReverse, Asc, Chr, AscW, ChrW,
'          Val, Str, Hex, Oct, Format, Format$
'==============================================================================
Option Explicit

Public Function DemoStringFunctions(ByVal sInput As String) As String
    Dim sResult As String
    Dim lPos As Long
    
    ' --- Len ---
    Debug.Print "Length: " & Len(sInput)
    
    ' --- Left$ / Right$ / Mid$ (functions) ---
    If Len(sInput) >= 5 Then
        sResult = Left$(sInput, 3) & "|" & Right$(sInput, 3) & "|" & Mid$(sInput, 2, 3)
    End If
    
    ' --- Mid$ as statement (replace in place) ---
    Dim sMutable As String
    sMutable = "ABCDEFGH"
    Mid$(sMutable, 3, 2) = "XX"         ' Result: "ABXXEFGH"
    
    ' --- Trim$ / LTrim$ / RTrim$ ---
    Dim sPadded As String
    sPadded = "  Hello  "
    Debug.Print "Trim:  [" & Trim$(sPadded) & "]"
    Debug.Print "LTrim: [" & LTrim$(sPadded) & "]"
    Debug.Print "RTrim: [" & RTrim$(sPadded) & "]"
    
    ' --- UCase$ / LCase$ ---
    Debug.Print "Upper: " & UCase$(sInput)
    Debug.Print "Lower: " & LCase$(sInput)
    
    ' --- Space$ / String$ ---
    Dim sSpaces As String
    sSpaces = Space$(10)
    Dim sRepeated As String
    sRepeated = String$(5, "*")          ' "*****"
    sRepeated = String$(5, 65)           ' "AAAAA" (Asc of "A" = 65)
    
    ' --- InStr / InStrRev ---
    lPos = InStr(1, sInput, "a", vbTextCompare)
    Dim lPosRev As Long
    lPosRev = InStrRev(sInput, "a", -1, vbTextCompare)
    
    ' --- Replace ---
    Dim sReplaced As String
    sReplaced = Replace(sInput, " ", "_")
    
    ' --- StrComp ---
    Dim iCmp As Integer
    iCmp = StrComp("abc", "ABC", vbTextCompare)     ' 0 (equal)
    iCmp = StrComp("abc", "ABC", vbBinaryCompare)   ' non-zero
    
    ' --- StrConv ---
    Dim sConverted As String
    sConverted = StrConv(sInput, vbProperCase)       ' Title Case
    sConverted = StrConv(sInput, vbUpperCase)
    sConverted = StrConv(sInput, vbLowerCase)
    sConverted = StrConv(sInput, vbUnicode)
    sConverted = StrConv(sConverted, vbFromUnicode)
    
    ' --- StrReverse ---
    Dim sReversed As String
    sReversed = StrReverse(sInput)
    
    ' --- Asc / Chr / AscW / ChrW ---
    If Len(sInput) > 0 Then
        Dim iAscVal As Integer
        iAscVal = Asc(sInput)
        Debug.Print "Asc: " & iAscVal & " Chr: " & Chr$(iAscVal)
        
        Dim lAscW As Long
        lAscW = AscW(sInput)
        Debug.Print "AscW: " & lAscW & " ChrW: " & ChrW$(lAscW)
    End If
    
    ' --- Val / Str$ ---
    Dim dVal As Double
    dVal = Val("  123.45abc")            ' Returns 123.45
    Dim sStr As String
    sStr = Str$(dVal)                    ' Returns " 123.45"
    
    ' --- Hex$ / Oct$ ---
    Debug.Print "Hex: " & Hex$(255)      ' "FF"
    Debug.Print "Oct: " & Oct$(255)      ' "377"
    
    ' --- Format$ ---
    Debug.Print "Formatted Number: " & Format$(12345.6789, "#,##0.00")
    Debug.Print "Formatted Date:   " & Format$(Now, "yyyy-mm-dd hh:nn:ss")
    Debug.Print "Formatted Pct:    " & Format$(0.85, "0.00%")
    Debug.Print "Scientific:       " & Format$(12345.6789, "0.00E+00")
    
    ' --- Build result ---
    DemoStringFunctions = "Input='" & sInput & "'" & vbCrLf & _
                          "Reversed='" & sReversed & "'" & vbCrLf & _
                          "Replaced='" & sReplaced & "'" & vbCrLf & _
                          "Mutable='" & sMutable & "'"
End Function

' --- Split / Join ---

Public Function DemoSplitJoin(ByVal sCsv As String) As String
    Dim arr() As String
    arr = Split(sCsv, ",")
    
    Dim i As Long
    For i = LBound(arr) To UBound(arr)
        arr(i) = Trim$(arr(i))
    Next i
    
    DemoSplitJoin = Join(arr, " | ")
End Function

' --- LenB / LeftB / RightB / MidB / AscB / ChrB (byte-level) ---

Public Sub DemoByteStringFunctions()
    Dim sTest As String
    sTest = "Hello"
    
    Debug.Print "LenB: " & LenB(sTest)          ' 10 (Unicode = 2 bytes per char)
    Debug.Print "LeftB: " & LeftB$(sTest, 4)     ' First 2 chars
    Debug.Print "RightB: " & RightB$(sTest, 4)   ' Last 2 chars
    Debug.Print "MidB: " & MidB$(sTest, 3, 4)    ' 2 chars from pos 2
End Sub

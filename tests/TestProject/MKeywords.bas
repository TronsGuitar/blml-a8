Attribute VB_Name = "MKeywords"
'==============================================================================
' MODULE: MKeywords
' PURPOSE: Demonstrates Sub, Function, Property, Optional, ParamArray,
'          ByVal, ByRef, GoTo, GoSub, Return, On Error, Resume, With,
'          Select Case, For/Next, For Each/Next, Do/Loop, While/Wend,
'          If/Then/Else/ElseIf, Exit, End, Stop, Debug, WithEvents,
'          AddressOf, Implements (in classes), Like, Is, And/Or/Not/Xor/Eqv/Imp,
'          Mod, \, ^, &, +, -, *, /
'==============================================================================
Option Explicit

' --- Sub with Optional and ParamArray ---

Public Sub LogMessage(ByVal sMessage As String, _
                      Optional ByVal eSeverity As Severity = sevInfo, _
                      Optional ByVal sSource As String = "System")
    Dim sFormatted As String
    sFormatted = "[" & Format$(Now, "yyyy-mm-dd hh:nn:ss") & "] " & _
                 "[" & sSource & "] " & _
                 Choose(eSeverity + 1, "INFO", "WARN", "ERROR", "CRIT") & _
                 ": " & sMessage
    Debug.Print sFormatted
    Debug.Assert Len(sFormatted) > 0
End Sub

Public Sub LogMultiple(ParamArray Messages() As Variant)
    Dim vMsg As Variant
    Dim idx As Long
    
    ' --- For Each / Next on ParamArray ---
    For Each vMsg In Messages
        If Not IsMissing(vMsg) Then
            Debug.Print "  ParamArray(" & idx & "): " & CStr(vMsg)
        End If
        idx = idx + 1
    Next vMsg
End Sub

' --- Function with ByRef (default) and ByVal ---

Public Function AddAndReturn(ByRef lAccumulator As Long, _
                             ByVal lAmount As Long) As Long
    lAccumulator = lAccumulator + lAmount
    AddAndReturn = lAccumulator
End Function

' --- GoTo / On Error GoTo / Resume / Resume Next ---

Public Function SafeDivide(ByVal dNumerator As Double, _
                           ByVal dDenominator As Double) As Variant
    On Error GoTo ErrorHandler
    
    If dDenominator = 0# Then
        GoTo ZeroDivision
    End If
    
    SafeDivide = dNumerator / dDenominator
    GoTo CleanExit

ZeroDivision:
    SafeDivide = CVErr(11)              ' Division by zero error
    GoTo CleanExit

ErrorHandler:
    SafeDivide = CVErr(Err.Number)
    Resume CleanExit

CleanExit:
    On Error Resume Next
    ' Cleanup would go here
    Exit Function
End Function

' --- GoSub / Return (legacy) ---

Public Sub DemoGoSubReturn()
    Dim lValue As Long
    lValue = 10
    
    GoSub DoubleIt
    Debug.Print "After GoSub: " & lValue
    Exit Sub

DoubleIt:
    lValue = lValue * 2
    Return
End Sub

' --- Select Case with various matching ---

Public Function ClassifyValue(ByVal vValue As Variant) As String
    Select Case True
        Case IsEmpty(vValue)
            ClassifyValue = "Empty"
        Case IsNull(vValue)
            ClassifyValue = "Null"
        Case IsNumeric(vValue)
            Select Case CDbl(vValue)
                Case Is < 0
                    ClassifyValue = "Negative"
                Case 0
                    ClassifyValue = "Zero"
                Case 1 To 10
                    ClassifyValue = "Small"
                Case 11 To 100
                    ClassifyValue = "Medium"
                Case 101 To 1000, 2000, 3000
                    ClassifyValue = "Large or Special"
                Case Is > 1000
                    ClassifyValue = "Huge"
                Case Else
                    ClassifyValue = "Other Numeric"
            End Select
        Case IsDate(vValue)
            ClassifyValue = "Date"
        Case Else
            ClassifyValue = "String or Other"
    End Select
End Function

' --- All loop constructs ---

Public Sub DemonstrateLoops()
    Dim i As Long
    Dim j As Long
    Dim lSum As Long
    Dim bDone As Boolean
    
    ' --- For / Next with Step ---
    lSum = 0
    For i = 1 To 10 Step 1
        lSum = lSum + i
    Next i
    
    ' --- For / Next with negative Step ---
    For i = 10 To 1 Step -1
        Debug.Print i;
    Next i
    
    ' --- For Each / Next on Collection ---
    Dim col As New Collection
    col.Add "Alpha"
    col.Add "Beta"
    col.Add "Gamma"
    
    Dim vItem As Variant
    For Each vItem In col
        Debug.Print vItem
    Next vItem
    
    ' --- Do While / Loop ---
    i = 0
    Do While i < 5
        i = i + 1
    Loop
    
    ' --- Do / Loop While ---
    i = 0
    Do
        i = i + 1
    Loop While i < 5
    
    ' --- Do Until / Loop ---
    i = 0
    Do Until i >= 5
        i = i + 1
    Loop
    
    ' --- Do / Loop Until ---
    i = 0
    Do
        i = i + 1
    Loop Until i >= 5
    
    ' --- While / Wend (legacy) ---
    i = 0
    While i < 5
        i = i + 1
    Wend
    
    ' --- Exit For, Exit Do ---
    For i = 1 To 100
        If i > 5 Then Exit For
    Next i
    
    Do While True
        If bDone Then Exit Do
        bDone = True
    Loop
    
    ' --- Nested loops with labeled Next ---
    For i = 1 To 3
        For j = 1 To 3
            If i = 2 And j = 2 Then Exit For
        Next j
    Next i
    
    Set col = Nothing
End Sub

' --- If / Then / Else / ElseIf / End If ---

Public Function EvaluateGrade(ByVal dScore As Double) As String
    If dScore >= 90 Then
        EvaluateGrade = "A"
    ElseIf dScore >= 80 Then
        EvaluateGrade = "B"
    ElseIf dScore >= 70 Then
        EvaluateGrade = "C"
    ElseIf dScore >= 60 Then
        EvaluateGrade = "D"
    Else
        EvaluateGrade = "F"
    End If
    
    ' --- Single-line If/Then/Else ---
    If dScore = 100 Then EvaluateGrade = "A+" Else If dScore < 0 Then EvaluateGrade = "Invalid"
End Function

' --- Logical / Bitwise operators: And, Or, Not, Xor, Eqv, Imp ---

Public Function DemoLogicalOps(ByVal bA As Boolean, ByVal bB As Boolean) As String
    Dim sResult As String
    
    sResult = "And=" & (bA And bB) & _
              " Or=" & (bA Or bB) & _
              " Not=" & (Not bA) & _
              " Xor=" & (bA Xor bB) & _
              " Eqv=" & (bA Eqv bB) & _
              " Imp=" & (bA Imp bB)
    
    DemoLogicalOps = sResult
End Function

' --- Arithmetic operators: +, -, *, /, \, Mod, ^ ---

Public Function DemoArithmetic(ByVal a As Double, ByVal b As Double) As String
    Dim sResult As String
    
    If b <> 0 Then
        sResult = "Add=" & (a + b) & _
                  " Sub=" & (a - b) & _
                  " Mul=" & (a * b) & _
                  " Div=" & (a / b) & _
                  " IntDiv=" & (CLng(a) \ CLng(b)) & _
                  " Mod=" & (CLng(a) Mod CLng(b)) & _
                  " Pow=" & (a ^ 2)
    Else
        sResult = "Cannot divide by zero"
    End If
    
    ' --- String concatenation: & and + ---
    Dim sFull As String
    sFull = "Hello" & " " & "World"
    sFull = "Hello" + " " + "World"
    
    DemoArithmetic = sResult
End Function

' --- Like operator for pattern matching ---

Public Function MatchesPattern(ByVal sText As String, _
                               ByVal sPattern As String) As Boolean
    MatchesPattern = (sText Like sPattern)
End Function

' --- With / End With ---

Public Sub DemoWithBlock()
    Dim rct As RECT
    
    With rct
        .Left = 0
        .Top = 0
        .Right = 100
        .Bottom = 100
    End With
    
    Debug.Print "Rect: " & rct.Left & "," & rct.Top & "," & rct.Right & "," & rct.Bottom
End Sub

' --- AddressOf demonstration (callback pointer) ---

Public Function GetCallbackPointer() As Long
    GetCallbackPointer = 0  ' In real usage: AddressOf MyCallbackProc
    ' Note: AddressOf can only be used when passing to an API call
    ' Example: SetTimer hWnd, 1, 1000, AddressOf TimerCallback
End Function

' Cannot actually call AddressOf at module level, but demonstrating the concept:
Public Sub TimerCallback(ByVal hWnd As Long, ByVal uMsg As Long, _
                         ByVal idEvent As Long, ByVal dwTime As Long)
    ' This would be the callback procedure passed via AddressOf
    Debug.Print "Timer callback fired at " & dwTime
End Sub

' --- Stop (debugger break) ---

Public Sub DemoDebugFeatures()
    Dim i As Long
    i = 42
    
    Debug.Print "Value is: " & i
    Debug.Assert i = 42
    
    ' Stop  ' Uncomment to break into debugger
    ' End   ' Uncomment to immediately terminate (not recommended)
End Sub

' --- Demonstrates IIf, Choose, Switch ---

Public Function DemoInlineFunctions(ByVal lValue As Long) As String
    Dim sResult As String
    
    ' IIf
    sResult = IIf(lValue > 0, "Positive", "Non-positive")
    
    ' Choose
    If lValue >= 1 And lValue <= 4 Then
        sResult = sResult & " " & Choose(lValue, "One", "Two", "Three", "Four")
    End If
    
    ' Switch
    sResult = sResult & " " & Switch( _
        lValue < 0, "Negative", _
        lValue = 0, "Zero", _
        lValue > 0, "Positive")
    
    DemoInlineFunctions = sResult
End Function

' --- Demonstrates On...GoTo (computed GoTo, legacy) ---

Public Sub DemoComputedGoTo(ByVal lChoice As Long)
    On lChoice GoTo Label1, Label2, Label3
    Debug.Print "No match"
    Exit Sub
    
Label1:
    Debug.Print "Choice 1"
    Exit Sub
Label2:
    Debug.Print "Choice 2"
    Exit Sub
Label3:
    Debug.Print "Choice 3"
    Exit Sub
End Sub

' --- Demonstrates On...GoSub (computed GoSub, legacy) ---

Public Sub DemoComputedGoSub(ByVal lChoice As Long)
    On lChoice GoSub Sub1, Sub2, Sub3
    Exit Sub
    
Sub1:
    Debug.Print "Subroutine 1"
    Return
Sub2:
    Debug.Print "Subroutine 2"
    Return
Sub3:
    Debug.Print "Subroutine 3"
    Return
End Sub

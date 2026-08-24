Attribute VB_Name = "MFlowControl"
'==============================================================================
' MODULE: MFlowControl
' PURPOSE: Demonstrates On Error GoTo/Resume/Resume Next, Err object,
'          Error/Error$, DoEvents, AppActivate, Shell, SendKeys,
'          Environ, Command, InputBox, MsgBox, Beep, Load/Unload,
'          Print, Write (Debug context), Let
'==============================================================================
Option Explicit

' --- Comprehensive error handling ---

Public Sub DemoErrorHandling()
    Dim lOrigErrNum As Long
    Dim sOrigErrDesc As String
    
    ' --- On Error GoTo ---
    On Error GoTo ErrHandler
    
    ' Force an error
    Err.Raise 9, "MFlowControl.DemoErrorHandling", "Subscript out of range demo"
    
    Exit Sub

ErrHandler:
    lOrigErrNum = Err.Number
    sOrigErrDesc = Err.Description
    
    Debug.Print "Error #" & lOrigErrNum & ": " & sOrigErrDesc
    Debug.Print "Source: " & Err.Source
    
    ' --- Error$ function (returns error description for a given number) ---
    Debug.Print "Error$(11) = " & Error$(11)
    
    ' --- Err.Clear ---
    Err.Clear
    
    ' --- On Error Resume Next ---
    On Error Resume Next
    Dim dResult As Double
    dResult = 1 / 0                     ' This won't crash
    If Err.Number <> 0 Then
        Debug.Print "Caught via Resume Next: " & Err.Description
        Err.Clear
    End If
    
    ' --- On Error GoTo 0 (disable error handling) ---
    On Error GoTo 0
    
    ' --- CVErr ---
    Dim vErr As Variant
    vErr = CVErr(2015)
    If IsError(vErr) Then
        Debug.Print "Variant error value: " & CStr(vErr)
    End If
End Sub

' --- DoEvents ---

Public Sub DemoDoEvents(ByVal lIterations As Long)
    Dim i As Long
    For i = 1 To lIterations
        ' Allow Windows to process messages
        DoEvents
    Next i
End Sub

' --- Shell / Environ / Command$ ---

Public Function DemoEnvironment() As String
    Dim sResult As String
    
    ' --- Environ$ ---
    sResult = "PATH=" & Left$(Environ$("PATH"), 50) & "..." & vbCrLf
    sResult = sResult & "TEMP=" & Environ$("TEMP") & vbCrLf
    sResult = sResult & "USERNAME=" & Environ$("USERNAME") & vbCrLf
    
    ' --- Environ by index ---
    Dim sEnv As String
    sEnv = Environ$(1)                  ' First environment variable
    sResult = sResult & "Environ(1)=" & sEnv & vbCrLf
    
    ' --- Command$ (command line arguments) ---
    sResult = sResult & "Command$=" & Command$ & vbCrLf
    
    DemoEnvironment = sResult
End Function

Public Sub DemoShell()
    Dim lTaskID As Long
    
    On Error Resume Next
    ' --- Shell ---
    lTaskID = Shell("notepad.exe", vbNormalFocus)
    If Err.Number = 0 And lTaskID <> 0 Then
        ' --- AppActivate ---
        AppActivate lTaskID
        
        ' Wait a moment
        Sleep 500
        
        ' --- SendKeys ---
        SendKeys "Hello from VB6!{ENTER}", True
        SendKeys "%{F4}", True           ' Alt+F4 to close
    End If
    On Error GoTo 0
End Sub

' --- MsgBox / InputBox ---

Public Function DemoDialogs() As String
    Dim sResult As String
    Dim lResponse As Long
    
    ' --- MsgBox as function ---
    lResponse = MsgBox("Continue processing?", _
                       vbYesNoCancel + vbQuestion + vbDefaultButton1, _
                       "Confirm")
    
    Select Case lResponse
        Case vbYes
            sResult = "User chose Yes"
        Case vbNo
            sResult = "User chose No"
        Case vbCancel
            sResult = "User chose Cancel"
    End Select
    
    ' --- MsgBox as statement ---
    MsgBox "Processing complete.", vbInformation, "Done"
    
    ' --- InputBox ---
    Dim sInput As String
    sInput = InputBox("Enter a value:", "Input Required", "Default Value")
    
    sResult = sResult & " Input='" & sInput & "'"
    DemoDialogs = sResult
End Function

' --- Beep ---

Public Sub DemoBeep()
    Beep
End Sub

' --- Let keyword (explicit assignment, rarely used) ---

Public Sub DemoLetKeyword()
    Dim sValue As String
    Let sValue = "Assigned with Let"     ' Let is optional but valid
    
    Dim lValue As Long
    Let lValue = 42
    
    Debug.Print sValue & " " & lValue
End Sub

' --- Print / Write in Debug context ---

Public Sub DemoPrintWrite()
    ' --- Debug.Print with formatting ---
    Debug.Print "Column1"; Tab(20); "Column2"; Tab(40); "Column3"
    Debug.Print "Data1"; Spc(5); "Data2"; Spc(5); "Data3"
    
    ' --- Print using semicolons and commas for formatting ---
    Debug.Print 1; 2; 3                 ' Semicolon = compact
    Debug.Print 1, 2, 3                 ' Comma = tab zones
End Sub

' --- Load / Unload (for forms) ---

Public Sub DemoFormOperations()
    On Error Resume Next
    
    ' Load frmMain        ' Would load the form into memory
    ' frmMain.Show vbModal ' Show as modal
    ' Unload frmMain      ' Unload from memory
    ' Set frmMain = Nothing
    
    ' These are commented because in an ActiveX DLL context,
    ' forms need special handling. The form files are included
    ' to demonstrate the syntax.
    
    On Error GoTo 0
End Sub

' --- Demonstrates Me keyword is only valid in class/form context ---
' (See CKeywordEngine.cls for Me usage)

' --- Demonstrates New keyword ---

Public Function DemoNewKeyword() As Collection
    Dim col As New Collection           ' Auto-instantiation with New
    
    Dim col2 As Collection
    Set col2 = New Collection           ' Explicit instantiation with New
    
    col.Add "Item1", "Key1"
    col.Add "Item2", "Key2"
    
    Set DemoNewKeyword = col
    Set col2 = Nothing
End Function

' --- CreateObject / GetObject ---

Public Sub DemoLateBinding()
    On Error Resume Next
    
    ' --- CreateObject (late binding) ---
    Dim oDict As Object
    Set oDict = CreateObject("Scripting.Dictionary")
    
    If Not oDict Is Nothing Then
        oDict.Add "Key1", "Value1"
        oDict.Add "Key2", "Value2"
        
        Dim vKey As Variant
        For Each vKey In oDict.Keys
            Debug.Print vKey & "=" & oDict(vKey)
        Next vKey
        
        Set oDict = Nothing
    End If
    
    ' --- GetObject (connect to existing instance) ---
    ' Dim oExcel As Object
    ' Set oExcel = GetObject(, "Excel.Application")
    ' This would connect to a running Excel instance
    
    On Error GoTo 0
End Sub

' --- TypeOf...Is ---

Public Function GetObjectTypeName(ByVal obj As Object) As String
    If obj Is Nothing Then
        GetObjectTypeName = "Nothing"
    ElseIf TypeOf obj Is Collection Then
        GetObjectTypeName = "Collection"
    ElseIf TypeOf obj Is CKeywordEngine Then
        GetObjectTypeName = "CKeywordEngine"
    Else
        GetObjectTypeName = TypeName(obj)
    End If
End Function

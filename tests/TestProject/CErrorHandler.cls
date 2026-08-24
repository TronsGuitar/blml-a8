VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CErrorHandler"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
'==============================================================================
' CLASS: CErrorHandler
' PURPOSE: Demonstrates Err object (Number, Description, Source, Clear, Raise),
'          Error trapping modes, and structured error logging.
'==============================================================================
Option Explicit

Public Event ErrorLogged(ByVal lErrNum As Long, ByVal sDesc As String)

Private m_Errors As Collection
Private m_MaxErrors As Long

Private Sub Class_Initialize()
    Set m_Errors = New Collection
    m_MaxErrors = 100
End Sub

Private Sub Class_Terminate()
    Set m_Errors = Nothing
End Sub

Public Property Get MaxErrors() As Long
    MaxErrors = m_MaxErrors
End Property

Public Property Let MaxErrors(ByVal lValue As Long)
    m_MaxErrors = lValue
End Property

Public Property Get ErrorCount() As Long
    ErrorCount = m_Errors.Count
End Property

' --- Log current Err state ---
Public Sub LogCurrentError(Optional ByVal sContext As String = "")
    If Err.Number = 0 Then Exit Sub
    
    Dim sEntry As String
    sEntry = Format$(Now, "yyyy-mm-dd hh:nn:ss") & "|" & _
             Err.Number & "|" & _
             Err.Source & "|" & _
             Err.Description & "|" & _
             sContext
    
    If m_Errors.Count < m_MaxErrors Then
        m_Errors.Add sEntry
    End If
    
    RaiseEvent ErrorLogged(Err.Number, Err.Description)
    
    Debug.Print "ERROR LOGGED: " & sEntry
End Sub

' --- Raise a custom error ---
Public Sub RaiseCustomError(ByVal lNumber As Long, _
                            ByVal sDescription As String, _
                            Optional ByVal sSource As String = "")
    If Len(sSource) = 0 Then sSource = "KeywordShowcase"
    
    Err.Raise lNumber + vbObjectError, sSource, sDescription
End Sub

' --- Get error log as string ---
Public Function GetErrorLog() As String
    Dim sLog As String
    Dim vEntry As Variant
    
    sLog = "=== Error Log (" & m_Errors.Count & " entries) ===" & vbCrLf
    
    For Each vEntry In m_Errors
        sLog = sLog & CStr(vEntry) & vbCrLf
    Next vEntry
    
    GetErrorLog = sLog
End Function

' --- Clear the error log ---
Public Sub ClearLog()
    Set m_Errors = New Collection
    Err.Clear
End Sub

' --- Test various error conditions ---
Public Sub TestErrorConditions()
    ' --- On Error Resume Next ---
    On Error Resume Next
    
    ' Type mismatch
    Dim lTest As Long
    lTest = CLng("not a number")
    If Err.Number <> 0 Then
        LogCurrentError "TypeMismatch"
        Err.Clear
    End If
    
    ' Division by zero
    Dim dResult As Double
    dResult = 1 / 0
    If Err.Number <> 0 Then
        LogCurrentError "DivByZero"
        Err.Clear
    End If
    
    ' Overflow
    Dim iSmall As Integer
    iSmall = CLng(99999)
    If Err.Number <> 0 Then
        LogCurrentError "Overflow"
        Err.Clear
    End If
    
    ' Subscript out of range
    Dim arr(0 To 2) As Long
    arr(0) = arr(99)
    If Err.Number <> 0 Then
        LogCurrentError "Subscript"
        Err.Clear
    End If
    
    ' Object required
    Dim oTest As Object
    Set oTest = Nothing
    Dim sName As String
    ' sName = oTest.Name  ' Would cause error 91
    
    On Error GoTo 0
End Sub

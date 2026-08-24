VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CEventRaiser"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
'==============================================================================
' CLASS: CEventRaiser
' PURPOSE: Demonstrates Event declaration and RaiseEvent.
'          Consumed via WithEvents in CKeywordEngine.
'==============================================================================
Option Explicit

' --- Event declaration ---
Public Event EventFired(ByVal sEventName As String, ByVal sData As String)
Public Event StatusChanged(ByVal sOldStatus As String, ByVal sNewStatus As String)

Private m_Status As String

Private Sub Class_Initialize()
    m_Status = "Idle"
End Sub

Private Sub Class_Terminate()
    m_Status = ""
End Sub

Public Property Get Status() As String
    Status = m_Status
End Property

Public Property Let Status(ByVal sNewStatus As String)
    Dim sOld As String
    sOld = m_Status
    m_Status = sNewStatus
    ' --- RaiseEvent ---
    RaiseEvent StatusChanged(sOld, sNewStatus)
End Property

Public Sub FireEvent(ByVal sName As String, ByVal sData As String)
    ' --- RaiseEvent ---
    RaiseEvent EventFired(sName, sData)
End Sub

Public Sub SimulateWork(ByVal lSteps As Long)
    Dim i As Long
    Me.Status = "Working"
    
    For i = 1 To lSteps
        RaiseEvent EventFired("Step", CStr(i) & " of " & CStr(lSteps))
        DoEvents
    Next i
    
    Me.Status = "Complete"
End Sub

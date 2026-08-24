VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CDataProcessor"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
'==============================================================================
' CLASS: CDataProcessor
' PURPOSE: Interface class used with Implements keyword.
'          Defines the contract that CKeywordEngine implements.
'          Also demonstrates abstract-like pattern in VB6.
'==============================================================================
Option Explicit

' --- Interface methods ---

Public Function ProcessData(ByVal sInput As String) As String
    ' Default implementation (overridden by implementors)
    ProcessData = ""
End Function

Public Sub Initialize(ByVal sConfig As String)
    ' Default implementation
End Sub

Public Property Get Status() As String
    Status = "Base"
End Property

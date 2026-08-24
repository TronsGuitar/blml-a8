VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CFileWorker"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
'==============================================================================
' CLASS: CFileWorker
' PURPOSE: File operations class demonstrating Open/Close/FreeFile/EOF/LOF
'          in an OOP context. Also demonstrates Persistable-like patterns.
'==============================================================================
Option Explicit

Private m_FilePath As String
Private m_FileNum As Integer
Private m_IsOpen As Boolean
Private m_Mode As String

Private Sub Class_Initialize()
    m_IsOpen = False
    m_FileNum = 0
    m_Mode = ""
End Sub

Private Sub Class_Terminate()
    If m_IsOpen Then
        CloseFile
    End If
End Sub

Public Property Get FilePath() As String
    FilePath = m_FilePath
End Property

Public Property Get IsOpen() As Boolean
    IsOpen = m_IsOpen
End Property

Public Property Get FileSize() As Long
    If m_IsOpen Then
        FileSize = LOF(m_FileNum)
    Else
        On Error Resume Next
        FileSize = FileLen(m_FilePath)
        On Error GoTo 0
    End If
End Property

Public Function OpenForOutput(ByVal sPath As String) As Boolean
    On Error GoTo ErrOut
    If m_IsOpen Then CloseFile
    
    m_FilePath = sPath
    m_FileNum = FreeFile
    Open m_FilePath For Output As #m_FileNum
    m_IsOpen = True
    m_Mode = "Output"
    OpenForOutput = True
    Exit Function
ErrOut:
    OpenForOutput = False
End Function

Public Function OpenForInput(ByVal sPath As String) As Boolean
    On Error GoTo ErrIn
    If m_IsOpen Then CloseFile
    
    m_FilePath = sPath
    m_FileNum = FreeFile
    Open m_FilePath For Input As #m_FileNum
    m_IsOpen = True
    m_Mode = "Input"
    OpenForInput = True
    Exit Function
ErrIn:
    OpenForInput = False
End Function

Public Function OpenForAppend(ByVal sPath As String) As Boolean
    On Error GoTo ErrApp
    If m_IsOpen Then CloseFile
    
    m_FilePath = sPath
    m_FileNum = FreeFile
    Open m_FilePath For Append As #m_FileNum
    m_IsOpen = True
    m_Mode = "Append"
    OpenForAppend = True
    Exit Function
ErrApp:
    OpenForAppend = False
End Function

Public Sub WriteLine(ByVal sLine As String)
    If m_IsOpen And (m_Mode = "Output" Or m_Mode = "Append") Then
        Print #m_FileNum, sLine
    End If
End Sub

Public Sub WriteDelimited(ParamArray Fields() As Variant)
    If m_IsOpen And (m_Mode = "Output" Or m_Mode = "Append") Then
        Dim i As Long
        Dim sLine As String
        For i = LBound(Fields) To UBound(Fields)
            If i > LBound(Fields) Then sLine = sLine & ","
            sLine = sLine & CStr(Fields(i))
        Next i
        Write #m_FileNum, sLine
    End If
End Sub

Public Function ReadLine() As String
    If m_IsOpen And m_Mode = "Input" And Not EOF(m_FileNum) Then
        Dim sLine As String
        Line Input #m_FileNum, sLine
        ReadLine = sLine
    End If
End Function

Public Function ReadAll() As String
    If m_IsOpen And m_Mode = "Input" Then
        Dim sAll As String
        Dim sLine As String
        Do While Not EOF(m_FileNum)
            Line Input #m_FileNum, sLine
            If Len(sAll) > 0 Then sAll = sAll & vbCrLf
            sAll = sAll & sLine
        Loop
        ReadAll = sAll
    End If
End Function

Public Function AtEndOfFile() As Boolean
    If m_IsOpen Then
        AtEndOfFile = EOF(m_FileNum)
    Else
        AtEndOfFile = True
    End If
End Function

Public Sub CloseFile()
    If m_IsOpen Then
        Close #m_FileNum
        m_IsOpen = False
        m_FileNum = 0
        m_Mode = ""
    End If
End Sub

Public Sub DeleteFile()
    If m_IsOpen Then CloseFile
    On Error Resume Next
    If Len(m_FilePath) > 0 Then Kill m_FilePath
    On Error GoTo 0
End Sub

VERSION 5.00
Begin VB.Form frmMain 
   Caption         =   "VB6 Keyword Showcase"
   ClientHeight    =   6000
   ClientLeft      =   120
   ClientTop       =   465
   ClientWidth     =   9000
   LinkTopic       =   "Form1"
   ScaleHeight     =   6000
   ScaleWidth      =   9000
   StartUpPosition =   2  'CenterScreen
   Begin VB.CommandButton cmdRunAll 
      Caption         =   "&Run Full Showcase"
      Height          =   495
      Left            =   120
      TabIndex        =   0
      Top             =   120
      Width           =   2535
   End
   Begin VB.CommandButton cmdStrings 
      Caption         =   "&Strings"
      Height          =   495
      Left            =   2760
      TabIndex        =   1
      Top             =   120
      Width           =   1215
   End
   Begin VB.CommandButton cmdMath 
      Caption         =   "&Math"
      Height          =   495
      Left            =   4080
      TabIndex        =   2
      Top             =   120
      Width           =   1215
   End
   Begin VB.CommandButton cmdFiles 
      Caption         =   "&Files"
      Height          =   495
      Left            =   5400
      TabIndex        =   3
      Top             =   120
      Width           =   1215
   End
   Begin VB.CommandButton cmdSysInfo 
      Caption         =   "S&ystem Info"
      Height          =   495
      Left            =   6720
      TabIndex        =   4
      Top             =   120
      Width           =   1215
   End
   Begin VB.CommandButton cmdClear 
      Caption         =   "&Clear"
      Height          =   495
      Left            =   8040
      TabIndex        =   5
      Top             =   120
      Width           =   855
   End
   Begin VB.TextBox txtOutput 
      Height          =   3735
      Left            =   120
      MultiLine       =   -1  'True
      ScrollBars      =   3  'Both
      TabIndex        =   6
      Top             =   720
      Width           =   8775
   End
   Begin VB.ComboBox cboMode 
      Height          =   315
      Left            =   1320
      Style           =   2  'Dropdown List
      TabIndex        =   8
      Top             =   4560
      Width           =   2055
   End
   Begin VB.CheckBox chkVerbose 
      Caption         =   "Verbose Output"
      Height          =   255
      Left            =   3600
      TabIndex        =   9
      Top             =   4600
      Value           =   1  'Checked
      Width           =   1575
   End
   Begin VB.OptionButton optOutput(0) 
      Caption         =   "Text"
      Height          =   255
      Index           =   0
      Left            =   5400
      TabIndex        =   10
      Top             =   4560
      Value           =   -1  'True
      Width           =   735
   End
   Begin VB.OptionButton optOutput(1) 
      Caption         =   "HTML"
      Height          =   255
      Index           =   1
      Left            =   6240
      TabIndex        =   11
      Top             =   4560
      Width           =   735
   End
   Begin VB.OptionButton optOutput(2) 
      Caption         =   "CSV"
      Height          =   255
      Index           =   2
      Left            =   7080
      TabIndex        =   12
      Top             =   4560
      Width           =   735
   End
   Begin VB.ListBox lstLog 
      Height          =   1035
      Left            =   120
      TabIndex        =   13
      Top             =   4920
      Width           =   5895
   End
   Begin VB.Label prgProgress 
      Alignment       =   2  'Center
      BackColor       =   &H8000000F&
      BorderStyle     =   1  'Fixed Single
      Caption         =   "0%"
      Height          =   255
      Left            =   6120
      TabIndex        =   14
      Top             =   4920
      Width           =   2775
   End
   Begin VB.Timer tmrAutoRefresh 
      Enabled         =   0   'False
      Interval        =   1000
      Left            =   8280
      Top             =   5400
   End
   Begin VB.Label lblMode 
      Caption         =   "Mode:"
      Height          =   255
      Left            =   120
      TabIndex        =   7
      Top             =   4600
      Width           =   1095
   End
   Begin VB.Label lblStatus 
      BorderStyle     =   1  'Fixed Single
      Caption         =   "Ready"
      Height          =   255
      Left            =   6120
      TabIndex        =   15
      Top             =   5280
      Width           =   2775
   End
End
Attribute VB_Name = "frmMain"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'==============================================================================
' FORM: frmMain
' PURPOSE: Demonstrates form events (Load, Unload, Resize, QueryUnload,
'          Activate, Deactivate, Paint, Initialize, Terminate),
'          Control events, Print method, ScaleMode, Me, Load/Unload,
'          Show/Hide, control array (optOutput), Timer, WithEvents
'==============================================================================
Option Explicit

' --- WithEvents for the engine ---
Private WithEvents m_Engine As CKeywordEngine
Attribute m_Engine.VB_VarHelpID = -1

Private m_OutputFormat As String
Private m_Verbose As Boolean

' ==========================================
' Form Events
' ==========================================

Private Sub Form_Initialize()
    Debug.Print "frmMain: Initialize"
End Sub

Private Sub Form_Load()
    Debug.Print "frmMain: Load"
    
    ' --- Me keyword ---
    Me.Caption = APP_NAME & " v" & APP_VERSION
    
    ' --- ScaleMode ---
    Me.ScaleMode = vbTwips
    
    ' --- Create engine with events ---
    Set m_Engine = New CKeywordEngine
    
    ' --- Populate combo ---
    cboMode.AddItem "None"
    cboMode.AddItem "Text"
    cboMode.AddItem "Numeric"
    cboMode.AddItem "Binary"
    cboMode.AddItem "All"
    cboMode.ListIndex = 4
    
    m_OutputFormat = "Text"
    m_Verbose = True
    
    ' --- Print method on form ---
    Me.Print "VB6 Keyword Showcase Loaded"
    
    lblStatus.Caption = "Ready"
End Sub

Private Sub Form_Unload(Cancel As Integer)
    Debug.Print "frmMain: Unload"
    
    ' Could set Cancel = 1 to prevent close
    
    Set m_Engine = Nothing
End Sub

Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)
    Debug.Print "frmMain: QueryUnload Mode=" & UnloadMode
    
    Select Case UnloadMode
        Case vbFormControlMenu   ' User clicked X
            ' Confirm close
        Case vbFormCode          ' Unload statement
            ' Always allow
        Case vbAppWindows        ' Windows shutting down
            ' Always allow
        Case vbAppTaskManager    ' Task Manager
            ' Always allow
        Case vbFormMDIForm       ' MDI parent closing
            ' Always allow
    End Select
End Sub

Private Sub Form_Activate()
    Debug.Print "frmMain: Activate"
End Sub

Private Sub Form_Deactivate()
    Debug.Print "frmMain: Deactivate"
End Sub

Private Sub Form_Resize()
    On Error Resume Next
    If Me.WindowState <> vbMinimized Then
        txtOutput.Width = Me.ScaleWidth - 240
        lstLog.Width = Me.ScaleWidth - 3240
    End If
    On Error GoTo 0
End Sub

Private Sub Form_Paint()
    ' --- Print on form surface ---
    Me.CurrentX = 0
    Me.CurrentY = Me.ScaleHeight - 300
    Me.ForeColor = vbGrayText
    Me.Print "VB6 Keyword Showcase | " & Format$(Now, "hh:nn:ss")
End Sub

Private Sub Form_Terminate()
    Debug.Print "frmMain: Terminate"
End Sub

' ==========================================
' Control Events
' ==========================================

Private Sub cmdRunAll_Click()
    lblStatus.Caption = "Running..."
    prgProgress.Caption = "0%"
    DoEvents
    
    m_Engine.RunFullShowcase
    
    AppendOutput "=== Full Showcase Complete ==="
End Sub

Private Sub cmdStrings_Click()
    Dim sResult As String
    sResult = m_Engine.ProcessText("The Quick Brown Fox Jumps")
    AppendOutput sResult
End Sub

Private Sub cmdMath_Click()
    Dim sResult As String
    sResult = m_Engine.ProcessMath(-42.567)
    AppendOutput sResult
End Sub

Private Sub cmdFiles_Click()
    DemoSequentialIO
    DemoRandomAccessIO
    DemoBinaryIO
    DemoFileSystemOps
    AppendOutput "=== File I/O Demo Complete ==="
End Sub

Private Sub cmdSysInfo_Click()
    Dim sResult As String
    sResult = m_Engine.GetSystemInfo()
    AppendOutput sResult
End Sub

Private Sub cmdClear_Click()
    txtOutput.Text = ""
    lstLog.Clear
    prgProgress.Caption = "0%"
    lblStatus.Caption = "Ready"
    Me.Cls                               ' Cls - clear form graphics
End Sub

Private Sub cboMode_Click()
    m_Engine.Mode = cboMode.ListIndex
End Sub

Private Sub chkVerbose_Click()
    m_Verbose = (chkVerbose.Value = vbChecked)
End Sub

' --- Control array event handler ---
Private Sub optOutput_Click(Index As Integer)
    Select Case Index
        Case 0: m_OutputFormat = "Text"
        Case 1: m_OutputFormat = "HTML"
        Case 2: m_OutputFormat = "CSV"
    End Select
End Sub

' --- Timer event ---
Private Sub tmrAutoRefresh_Timer()
    lblStatus.Caption = "Auto: " & Format$(Now, "hh:nn:ss")
End Sub

' ==========================================
' WithEvents handlers for CKeywordEngine
' ==========================================

Private Sub m_Engine_ProcessingStarted(ByVal sTaskName As String)
    LogEvent "Started: " & sTaskName
    prgProgress.Caption = "0%"
End Sub

Private Sub m_Engine_ProcessingComplete(ByVal sTaskName As String, ByVal dElapsedMs As Double)
    LogEvent "Complete: " & sTaskName & " (" & Format$(dElapsedMs, "#,##0") & "ms)"
    prgProgress.Caption = "100%"
    lblStatus.Caption = "Done: " & sTaskName
End Sub

Private Sub m_Engine_ProgressUpdate(ByVal lPercent As Long, ByVal sMessage As String)
    If lPercent >= 0 And lPercent <= 100 Then
        prgProgress.Caption = CStr(lPercent) & "%"
    End If
    lblStatus.Caption = sMessage
    DoEvents
End Sub

Private Sub m_Engine_ErrorOccurred(ByVal lErrNum As Long, ByVal sErrDesc As String)
    LogEvent "ERROR #" & lErrNum & ": " & sErrDesc
End Sub

' ==========================================
' Helper methods
' ==========================================

Private Sub AppendOutput(ByVal sText As String)
    txtOutput.Text = txtOutput.Text & sText & vbCrLf
    txtOutput.SelStart = Len(txtOutput.Text)
End Sub

Private Sub LogEvent(ByVal sEvent As String)
    lstLog.AddItem Format$(Now, "hh:nn:ss") & " " & sEvent
    If lstLog.ListCount > 0 Then
        lstLog.ListIndex = lstLog.ListCount - 1
    End If
End Sub

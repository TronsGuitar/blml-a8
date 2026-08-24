VERSION 5.00
Begin VB.Form frmDialog 
   BorderStyle     =   3  'Fixed Dialog
   Caption         =   "Input Dialog"
   ClientHeight    =   2400
   ClientLeft      =   45
   ClientTop       =   390
   ClientWidth     =   5400
   ControlBox      =   0   'False
   LinkTopic       =   "Form2"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   2400
   ScaleWidth      =   5400
   ShowInTaskbar   =   0   'False
   StartUpPosition =   1  'CenterOwner
   Begin VB.TextBox txtInput 
      Height          =   375
      Left            =   120
      TabIndex        =   1
      Top             =   480
      Width           =   5175
   End
   Begin VB.CommandButton cmdOK 
      Caption         =   "&OK"
      Default         =   -1  'True
      Height          =   495
      Left            =   2640
      TabIndex        =   2
      Top             =   1680
      Width           =   1215
   End
   Begin VB.CommandButton cmdCancel 
      Cancel          =   -1  'True
      Caption         =   "&Cancel"
      Height          =   495
      Left            =   3960
      TabIndex        =   3
      Top             =   1680
      Width           =   1215
   End
   Begin VB.Frame fraOptions 
      Caption         =   "Options"
      Height          =   735
      Left            =   120
      TabIndex        =   4
      Top             =   960
      Width           =   2415
      Begin VB.CheckBox chkOption1 
         Caption         =   "Option &1"
         Height          =   255
         Left            =   120
         TabIndex        =   5
         Top             =   360
         Width           =   1095
      End
      Begin VB.CheckBox chkOption2 
         Caption         =   "Option &2"
         Height          =   255
         Left            =   1320
         TabIndex        =   6
         Top             =   360
         Width           =   1095
      End
   End
   Begin VB.Label lblPrompt 
      Caption         =   "Enter a value:"
      Height          =   255
      Left            =   120
      TabIndex        =   0
      Top             =   120
      Width           =   5175
   End
End
Attribute VB_Name = "frmDialog"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'==============================================================================
' FORM: frmDialog
' PURPOSE: Demonstrates modal dialog pattern, Tag property,
'          Move, ZOrder, Hide/Show, Default/Cancel buttons
'==============================================================================
Option Explicit

Private m_Cancelled As Boolean

Public Property Get InputValue() As String
    InputValue = txtInput.Text
End Property

Public Property Let Prompt(ByVal sPrompt As String)
    lblPrompt.Caption = sPrompt
End Property

Public Property Get Cancelled() As Boolean
    Cancelled = m_Cancelled
End Property

Private Sub Form_Load()
    m_Cancelled = True
    
    ' --- Tag property ---
    Me.Tag = "DialogForm"
    
    ' --- Move ---
    Me.Move (Screen.Width - Me.Width) \ 2, (Screen.Height - Me.Height) \ 2
    
    ' --- ZOrder ---
    Me.ZOrder 0                          ' Bring to front
End Sub

Private Sub cmdOK_Click()
    m_Cancelled = False
    ' --- Hide (don't Unload, so caller can read properties) ---
    Me.Hide
End Sub

Private Sub cmdCancel_Click()
    m_Cancelled = True
    Me.Hide
End Sub

Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)
    If UnloadMode = vbFormControlMenu Then
        Cancel = 1
        m_Cancelled = True
        Me.Hide
    End If
End Sub

' --- Public method to show as modal dialog ---
Public Function ShowDialog(Optional ByVal sPrompt As String = "Enter a value:", _
                           Optional ByVal sDefault As String = "") As String
    Me.Prompt = sPrompt
    txtInput.Text = sDefault
    
    ' --- Show vbModal ---
    Me.Show vbModal
    
    If Not m_Cancelled Then
        ShowDialog = txtInput.Text
    Else
        ShowDialog = ""
    End If
End Function

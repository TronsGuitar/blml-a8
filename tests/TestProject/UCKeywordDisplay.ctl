VERSION 5.00
Begin VB.UserControl UCKeywordDisplay 
   ClientHeight    =   1800
   ClientLeft      =   0
   ClientTop       =   0
   ClientWidth     =   4800
   ScaleHeight     =   1800
   ScaleWidth      =   4800
   Begin VB.TextBox txtDisplay 
      Height          =   1215
      Left            =   60
      Locked          =   -1  'True
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   0
      Top             =   0
      Width           =   4695
   End
   Begin VB.CommandButton cmdRefresh 
      Caption         =   "Refresh"
      Height          =   375
      Left            =   60
      TabIndex        =   1
      Top             =   1320
      Width           =   1215
   End
   Begin VB.Label lblTitle 
      Caption         =   "Keyword Display"
      Height          =   255
      Left            =   1440
      TabIndex        =   2
      Top             =   1380
      Width           =   3255
   End
End
Attribute VB_Name = "UCKeywordDisplay"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
'==============================================================================
' USERCONTROL: UCKeywordDisplay
' PURPOSE: Demonstrates UserControl lifecycle:
'          InitProperties, ReadProperties, WriteProperties,
'          PropertyBag, PropertyChanged, Ambient, Extender,
'          UserControl_Initialize, UserControl_Terminate,
'          UserControl_Resize, UserControl_Paint,
'          UserControl_EnterFocus, UserControl_ExitFocus,
'          UserControl_Show, UserControl_Hide
'==============================================================================
Option Explicit

' --- Public Events ---
Public Event RefreshClicked()
Public Event DisplayUpdated(ByVal sContent As String)

' --- Default property values ---
Private Const DEF_TITLE As String = "Keyword Display"
Private Const DEF_AUTOREFRESH As Boolean = False
Private Const DEF_BACKCOLOR As Long = &H80000005  ' Window background

' --- Private state ---
Private m_Title As String
Private m_AutoRefresh As Boolean
Private m_Content As String

' ==========================================
' UserControl Lifecycle Events
' ==========================================

Private Sub UserControl_Initialize()
    Debug.Print "UCKeywordDisplay: Initialize"
    m_Title = DEF_TITLE
    m_AutoRefresh = DEF_AUTOREFRESH
End Sub

Private Sub UserControl_Terminate()
    Debug.Print "UCKeywordDisplay: Terminate"
End Sub

' --- InitProperties (first time control is placed on form) ---
Private Sub UserControl_InitProperties()
    Debug.Print "UCKeywordDisplay: InitProperties"
    
    m_Title = DEF_TITLE
    m_AutoRefresh = DEF_AUTOREFRESH
    
    ' --- Ambient properties ---
    UserControl.BackColor = Ambient.BackColor
    txtDisplay.Font = Ambient.Font
End Sub

' --- ReadProperties (loading from saved state) ---
Private Sub UserControl_ReadProperties(PropBag As PropertyBag)
    Debug.Print "UCKeywordDisplay: ReadProperties"
    
    ' --- PropertyBag.ReadProperty ---
    m_Title = PropBag.ReadProperty("Title", DEF_TITLE)
    m_AutoRefresh = PropBag.ReadProperty("AutoRefresh", DEF_AUTOREFRESH)
    UserControl.BackColor = PropBag.ReadProperty("BackColor", DEF_BACKCOLOR)
    
    lblTitle.Caption = m_Title
End Sub

' --- WriteProperties (saving state) ---
Private Sub UserControl_WriteProperties(PropBag As PropertyBag)
    Debug.Print "UCKeywordDisplay: WriteProperties"
    
    ' --- PropertyBag.WriteProperty ---
    PropBag.WriteProperty "Title", m_Title, DEF_TITLE
    PropBag.WriteProperty "AutoRefresh", m_AutoRefresh, DEF_AUTOREFRESH
    PropBag.WriteProperty "BackColor", UserControl.BackColor, DEF_BACKCOLOR
End Sub

Private Sub UserControl_Resize()
    On Error Resume Next
    txtDisplay.Width = UserControl.ScaleWidth - 120
    txtDisplay.Height = UserControl.ScaleHeight - 600
    cmdRefresh.Top = UserControl.ScaleHeight - 450
    lblTitle.Top = cmdRefresh.Top + 60
    On Error GoTo 0
End Sub

Private Sub UserControl_Paint()
    ' Custom painting on the UserControl surface
    UserControl.Line (0, UserControl.ScaleHeight - 500)- _
                     (UserControl.ScaleWidth, UserControl.ScaleHeight - 500), vbGrayText
End Sub

Private Sub UserControl_EnterFocus()
    Debug.Print "UCKeywordDisplay: EnterFocus"
End Sub

Private Sub UserControl_ExitFocus()
    Debug.Print "UCKeywordDisplay: ExitFocus"
End Sub

Private Sub UserControl_Show()
    Debug.Print "UCKeywordDisplay: Show"
End Sub

Private Sub UserControl_Hide()
    Debug.Print "UCKeywordDisplay: Hide"
End Sub

' ==========================================
' Public Properties
' ==========================================

Public Property Get Title() As String
    Title = m_Title
End Property

Public Property Let Title(ByVal sNewTitle As String)
    m_Title = sNewTitle
    lblTitle.Caption = m_Title
    ' --- PropertyChanged ---
    PropertyChanged "Title"
End Property

Public Property Get AutoRefresh() As Boolean
    AutoRefresh = m_AutoRefresh
End Property

Public Property Let AutoRefresh(ByVal bValue As Boolean)
    m_AutoRefresh = bValue
    PropertyChanged "AutoRefresh"
End Property

Public Property Get BackColor() As OLE_COLOR
    BackColor = UserControl.BackColor
End Property

Public Property Let BackColor(ByVal clr As OLE_COLOR)
    UserControl.BackColor = clr
    PropertyChanged "BackColor"
End Property

Public Property Get Content() As String
    Content = txtDisplay.Text
End Property

' --- Extender properties (provided by container) ---
Public Property Get ControlName() As String
    On Error Resume Next
    ControlName = Extender.Name
    On Error GoTo 0
End Property

Public Property Get ControlVisible() As Boolean
    On Error Resume Next
    ControlVisible = Extender.Visible
    On Error GoTo 0
End Property

' ==========================================
' Public Methods
' ==========================================

Public Sub DisplayText(ByVal sText As String)
    m_Content = sText
    txtDisplay.Text = sText
    RaiseEvent DisplayUpdated(sText)
End Sub

Public Sub AppendText(ByVal sText As String)
    m_Content = m_Content & vbCrLf & sText
    txtDisplay.Text = m_Content
End Sub

Public Sub ClearDisplay()
    m_Content = ""
    txtDisplay.Text = ""
End Sub

' ==========================================
' Control Events
' ==========================================

Private Sub cmdRefresh_Click()
    RaiseEvent RefreshClicked
End Sub

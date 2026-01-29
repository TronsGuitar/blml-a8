VERSION 5.00
Begin VB.Form AllControls 
   Caption         =   "All Controls"
   ClientHeight    =   7500
   ClientLeft      =   60
   ClientTop       =   345
   ClientWidth     =   9000
   BeginProperty Font 
      Name            =   "Arial"
      Size            =   8.25
      Charset         =   0
      Weight          =   400
   EndProperty
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdButton 
      Caption         =   "CommandButton"
      Height          =   495
      Left            =   600
      TabIndex        =   0
      Top             =   600
      Width           =   1500
   End
   Begin VB.TextBox txtBox 
      Height          =   495
      Left            =   600
      TabIndex        =   1
      Text            =   "TextBox"
      Top             =   1200
      Width           =   1500
   End
   Begin VB.Label lblLabel 
      Caption         =   "Label"
      Height          =   315
      Left            =   600
      TabIndex        =   2
      Top             =   1800
      Width           =   1500
   End
   Begin VB.CheckBox chkBox 
      Caption         =   "CheckBox"
      Height          =   315
      Left            =   600
      TabIndex        =   3
      Top             =   2400
      Width           =   1500
   End
   Begin VB.OptionButton optButton 
      Caption         =   "OptionButton"
      Height          =   315
      Left            =   600
      TabIndex        =   4
      Top             =   3000
      Width           =   1500
   End
   Begin VB.ListBox lstBox 
      Height          =   1215
      Left            =   600
      TabIndex        =   5
      Top             =   3600
      Width           =   1500
   End
   Begin VB.ComboBox cboBox 
      Height          =   315
      Left            =   600
      TabIndex        =   6
      Top             =   4800
      Width           =   1500
   End
   Begin VB.Frame fraFrame 
      Caption         =   "Frame"
      Height          =   2000
      Left            =   2400
      TabIndex        =   7
      Top             =   600
      Width           =   3000
   End
   Begin VB.PictureBox picBox 
      Height          =   1215
      Left            =   2400
      TabIndex        =   8
      Top             =   3000
      Width           =   1500
   End
   Begin VB.HScrollBar hScrollBar 
      Height          =   255
      Left            =   600
      TabIndex        =   9
      Top             =   6000
      Width           =   1500
   End
   Begin VB.VScrollBar vScrollBar 
      Height          =   1215
      Left            =   3000
      TabIndex        =   10
      Top             =   600
      Width           =   255
   End
   Begin VB.Timer tmrTimer 
      Interval        =   1000
      Left            =   0
      Top             =   0
   End
End
Attribute VB_Name = "AllControls"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
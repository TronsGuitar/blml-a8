VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CCollection"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
'==============================================================================
' CLASS: CCollection
' PURPOSE: Custom collection class demonstrating:
'          - Default property (Attribute VB_UserMemId = 0)
'          - NewEnum for For Each support (Attribute VB_UserMemId = -4)
'          - Collection wrapping pattern
'==============================================================================
Option Explicit

Private m_Col As Collection

Private Sub Class_Initialize()
    Set m_Col = New Collection
End Sub

Private Sub Class_Terminate()
    Set m_Col = Nothing
End Sub

' --- Default property (Item) ---
Public Property Get Item(ByVal vIndex As Variant) As Variant
Attribute Item.VB_UserMemId = 0
    If IsObject(m_Col(vIndex)) Then
        Set Item = m_Col(vIndex)
    Else
        Item = m_Col(vIndex)
    End If
End Property

' --- NewEnum for For Each support ---
Public Property Get NewEnum() As IUnknown
Attribute NewEnum.VB_UserMemId = -4
Attribute NewEnum.VB_MemberFlags = "40"
    Set NewEnum = m_Col.[_NewEnum]
End Property

Public Sub Add(ByVal vItem As Variant, Optional ByVal sKey As String = "", _
               Optional ByVal vBefore As Variant, Optional ByVal vAfter As Variant)
    If Len(sKey) > 0 Then
        If Not IsMissing(vBefore) Then
            m_Col.Add vItem, sKey, Before:=vBefore
        ElseIf Not IsMissing(vAfter) Then
            m_Col.Add vItem, sKey, After:=vAfter
        Else
            m_Col.Add vItem, sKey
        End If
    Else
        m_Col.Add vItem
    End If
End Sub

Public Sub Remove(ByVal vIndex As Variant)
    m_Col.Remove vIndex
End Sub

Public Property Get Count() As Long
    Count = m_Col.Count
End Property

Public Sub Clear()
    Set m_Col = New Collection
End Sub

' --- Demonstrates IsMissing with Optional Variant ---
Public Function Find(ByVal sSearchTerm As String, _
                     Optional ByVal vStartIndex As Variant) As Long
    Dim lStart As Long
    
    If IsMissing(vStartIndex) Then
        lStart = 1
    Else
        lStart = CLng(vStartIndex)
    End If
    
    Dim i As Long
    For i = lStart To m_Col.Count
        If CStr(m_Col(i)) Like "*" & sSearchTerm & "*" Then
            Find = i
            Exit Function
        End If
    Next i
    
    Find = 0
End Function

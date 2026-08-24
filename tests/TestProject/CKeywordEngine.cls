VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CKeywordEngine"
Attribute VB_GlobalNameSpace = True
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
'==============================================================================
' CLASS: CKeywordEngine
' PURPOSE: Primary public class exposed by the ActiveX DLL.
'          Demonstrates Class_Initialize, Class_Terminate, Property Get/Let/Set,
'          Public/Private/Friend, Event, RaiseEvent, WithEvents, Me,
'          Implements, Collection, Dictionary usage
'==============================================================================
Option Explicit

' --- Implements keyword (interface implementation) ---
Implements CDataProcessor

' --- Event declarations ---
Public Event ProcessingStarted(ByVal sTaskName As String)
Public Event ProcessingComplete(ByVal sTaskName As String, ByVal dElapsedMs As Double)
Public Event ProgressUpdate(ByVal lPercent As Long, ByVal sMessage As String)
Public Event ErrorOccurred(ByVal lErrNum As Long, ByVal sErrDesc As String)

' --- WithEvents (event sink for contained object) ---
Private WithEvents m_EventRaiser As CEventRaiser
Attribute m_EventRaiser.VB_VarHelpID = -1

' --- Private member variables ---
Private m_Name As String
Private m_Mode As ProcessingMode
Private m_Items As Collection
Private m_ErrorHandler As CErrorHandler
Private m_FileWorker As CFileWorker
Private m_Initialized As Boolean
Private m_StartTick As Long

' --- Class_Initialize ---
Private Sub Class_Initialize()
    m_Name = "KeywordEngine"
    m_Mode = pmAll
    Set m_Items = New Collection
    Set m_ErrorHandler = New CErrorHandler
    Set m_FileWorker = New CFileWorker
    Set m_EventRaiser = New CEventRaiser
    m_Initialized = True
    
    InitializeGlobals
    
    Debug.Print "CKeywordEngine initialized at " & Now
End Sub

' --- Class_Terminate ---
Private Sub Class_Terminate()
    Set m_Items = Nothing
    Set m_ErrorHandler = Nothing
    Set m_FileWorker = Nothing
    Set m_EventRaiser = Nothing
    m_Initialized = False
    
    TerminateGlobals
    
    Debug.Print "CKeywordEngine terminated at " & Now
End Sub

' ==========================================
' Property Get / Let / Set
' ==========================================

Public Property Get Name() As String
    Name = m_Name
End Property

Public Property Let Name(ByVal sNewName As String)
    If Len(sNewName) > 0 Then
        m_Name = sNewName
    End If
End Property

Public Property Get Mode() As ProcessingMode
    Mode = m_Mode
End Property

Public Property Let Mode(ByVal eNewMode As ProcessingMode)
    m_Mode = eNewMode
End Property

Public Property Get ItemCount() As Long
    ItemCount = m_Items.Count
End Property

Public Property Get Initialized() As Boolean
    Initialized = m_Initialized
End Property

' --- Property Set (for object references) ---
Public Property Set ErrorHandler(ByVal oHandler As CErrorHandler)
    Set m_ErrorHandler = oHandler
End Property

Public Property Get ErrorHandler() As CErrorHandler
    Set ErrorHandler = m_ErrorHandler
End Property

' ==========================================
' Friend methods (visible within project only)
' ==========================================

Friend Function GetInternalState() As String
    GetInternalState = "Name=" & m_Name & " Mode=" & m_Mode & _
                       " Items=" & m_Items.Count & " Init=" & m_Initialized
End Function

Friend Sub ResetInternal()
    Set m_Items = New Collection
    m_Mode = pmNone
End Sub

' ==========================================
' Public methods - the main API
' ==========================================

Public Sub RunFullShowcase()
    Dim lStartTick As Long
    lStartTick = GetTickCount()
    
    RaiseEvent ProcessingStarted("FullShowcase")
    
    ' --- Me keyword ---
    Debug.Print "Running showcase on: " & Me.Name
    
    ' 1. Data types
    RaiseEvent ProgressUpdate(10, "Demonstrating data types...")
    Call DemonstrateAllDataTypes
    
    ' 2. String operations
    RaiseEvent ProgressUpdate(20, "String operations...")
    Dim sStrResult As String
    sStrResult = DemoStringFunctions("Hello VB6 World")
    
    ' 3. Math operations
    RaiseEvent ProgressUpdate(30, "Math operations...")
    Dim sMathResult As String
    sMathResult = DemoMathFunctions(-42.567)
    
    ' 4. Date/Time
    RaiseEvent ProgressUpdate(40, "Date/Time functions...")
    Dim sDateResult As String
    sDateResult = DemoDateTimeFunctions()
    
    ' 5. Flow control
    RaiseEvent ProgressUpdate(50, "Flow control...")
    Call DemonstrateLoops
    Call DemoGoSubReturn
    Call DemoWithBlock
    
    ' 6. File I/O
    RaiseEvent ProgressUpdate(60, "File I/O...")
    Call DemoSequentialIO
    Call DemoRandomAccessIO
    Call DemoBinaryIO
    
    ' 7. Error handling
    RaiseEvent ProgressUpdate(70, "Error handling...")
    Call DemoErrorHandling
    
    ' 8. Logical operators
    RaiseEvent ProgressUpdate(80, "Logical operations...")
    Dim sLogical As String
    sLogical = DemoLogicalOps(True, False)
    
    ' 9. Environment
    RaiseEvent ProgressUpdate(90, "Environment info...")
    Dim sEnv As String
    sEnv = DemoEnvironment()
    
    ' 10. WinAPI
    RaiseEvent ProgressUpdate(95, "WinAPI calls...")
    Dim sSysInfo As String
    sSysInfo = GetSystemInfo()
    
    ' Complete
    Dim dElapsed As Double
    dElapsed = GetElapsedMs(lStartTick)
    RaiseEvent ProgressUpdate(100, "Complete!")
    RaiseEvent ProcessingComplete("FullShowcase", dElapsed)
End Sub

Public Function GetSystemInfo() As String
    Dim sResult As String
    
    sResult = "=== System Information ===" & vbCrLf & _
              "Computer: " & MWinAPI.GetComputerName() & vbCrLf & _
              "User: " & MWinAPI.GetUserName() & vbCrLf & _
              "Screen: " & GetScreenWidth() & "x" & GetScreenHeight() & vbCrLf & _
              "OS Version: " & GetOSVersion() & vbCrLf & _
              "Temp Path: " & MWinAPI.GetTempPath() & vbCrLf & _
              "App: " & App.Title & " v" & App.Major & "." & App.Minor & "." & App.Revision
    
    GetSystemInfo = sResult
End Function

Public Sub AddItem(ByVal vItem As Variant, Optional ByVal sKey As String = "")
    On Error Resume Next
    If Len(sKey) > 0 Then
        m_Items.Add vItem, sKey
    Else
        m_Items.Add vItem
    End If
    On Error GoTo 0
End Sub

Public Function GetItem(ByVal vIndex As Variant) As Variant
    On Error Resume Next
    If IsObject(m_Items(vIndex)) Then
        Set GetItem = m_Items(vIndex)
    Else
        GetItem = m_Items(vIndex)
    End If
    On Error GoTo 0
End Function

Public Sub RemoveItem(ByVal vIndex As Variant)
    On Error Resume Next
    m_Items.Remove vIndex
    On Error GoTo 0
End Sub

Public Function ProcessText(ByVal sInput As String) As String
    RaiseEvent ProcessingStarted("ProcessText")
    Dim lStart As Long
    lStart = GetTickCount()
    
    Dim sResult As String
    sResult = DemoStringFunctions(sInput) & vbCrLf & _
              "Split/Join: " & DemoSplitJoin("apple, banana, cherry, date")
    
    ProcessText = sResult
    RaiseEvent ProcessingComplete("ProcessText", GetElapsedMs(lStart))
End Function

Public Function ProcessMath(ByVal dValue As Double) As String
    RaiseEvent ProcessingStarted("ProcessMath")
    Dim lStart As Long
    lStart = GetTickCount()
    
    Dim sResult As String
    sResult = DemoMathFunctions(dValue) & vbCrLf & _
              DemoArithmetic(dValue, 7#) & vbCrLf & _
              DemoDateTimeFunctions()
    
    ProcessMath = sResult
    RaiseEvent ProcessingComplete("ProcessMath", GetElapsedMs(lStart))
End Function

' ==========================================
' Implements CDataProcessor interface
' ==========================================

Private Function CDataProcessor_ProcessData(ByVal sInput As String) As String
    CDataProcessor_ProcessData = Me.ProcessText(sInput)
End Function

Private Sub CDataProcessor_Initialize(ByVal sConfig As String)
    Me.Name = sConfig
End Sub

Private Property Get CDataProcessor_Status() As String
    CDataProcessor_Status = IIf(m_Initialized, "Ready", "Not Initialized")
End Property

' ==========================================
' WithEvents handler for CEventRaiser
' ==========================================

Private Sub m_EventRaiser_EventFired(ByVal sEventName As String, ByVal sData As String)
    Debug.Print "Engine received event: " & sEventName & " Data: " & sData
End Sub

' ==========================================
' Demonstrate TypeOf...Is with Me
' ==========================================

Public Function SelfDescribe() As String
    If TypeOf Me Is CKeywordEngine Then
        SelfDescribe = "I am a CKeywordEngine: " & Me.Name
    End If
    
    ' Also demonstrate Is with Nothing
    Dim oTest As Object
    Set oTest = Nothing
    If oTest Is Nothing Then
        SelfDescribe = SelfDescribe & " (test object is Nothing)"
    End If
End Function

Attribute VB_Name = "MGlobals"
'==============================================================================
' MODULE: MGlobals
' PURPOSE: Global/Public/Private variables, Enum, Const, DefType, Static,
'          Dim, ReDim, type conversions, Option statements
'==============================================================================
Option Explicit
Option Compare Text
Option Base 0

' --- DefType statements (must appear before any declarations) ---
' DefBool, DefByte, DefCur, DefDate, DefDbl, DefDec, DefInt, DefLng,
' DefObj, DefSng, DefStr, DefVar
DefBool B
DefByte Y
DefInt I-J
DefLng K-L
DefStr S
DefSng F
DefDbl D
DefCur C
DefDate T
DefVar V
DefObj O

' --- Enum declarations ---

Public Enum ProcessingMode
    pmNone = 0
    pmText = 1
    pmNumeric = 2
    pmBinary = 3
    pmAll = 4
End Enum

Public Enum Severity
    sevInfo = 0
    sevWarning = 1
    sevError = 2
    sevCritical = 3
End Enum

Public Enum DataTypeDemo
    dtBoolean = 1
    dtByte = 2
    dtInteger = 3
    dtLong = 4
    dtSingle = 5
    dtDouble = 6
    dtCurrency = 7
    dtDate = 8
    dtString = 9
    dtObject = 10
    dtVariant = 11
    dtDecimal = 12
End Enum

' --- Public / Global / Private / Dim module-level variables ---
' Demonstrates every VB6 data type

Public g_AppName As String
Public g_Version As String
Public g_Initialized As Boolean
Public g_Mode As ProcessingMode
Global g_LegacyCounter As Long          ' Global keyword (legacy, same as Public)

Private m_InternalFlag As Boolean
Private m_ErrorCount As Long

Dim m_ModuleVariant As Variant          ' Module-level Dim

' --- Const declarations ---

Public Const APP_NAME As String = "VB6 Keyword Showcase"
Public Const APP_VERSION As String = "1.0.0"
Public Const MAX_ITEMS As Long = 1000
Public Const PI_VALUE As Double = 3.14159265358979
Public Const CRLF As String = vbCrLf
Private Const INTERNAL_SEED As Long = 42

' --- Static variable demonstration ---

Public Function GetNextID() As Long
    Static lNextID As Long
    lNextID = lNextID + 1
    GetNextID = lNextID
End Function

' --- Initialization / Termination ---

Public Sub InitializeGlobals()
    g_AppName = APP_NAME
    g_Version = APP_VERSION
    g_Initialized = True
    g_Mode = pmAll
    g_LegacyCounter = 0
    m_InternalFlag = False
    m_ErrorCount = 0
    m_ModuleVariant = Empty
    
    ' Seed the Randomize statement
    Randomize Timer
End Sub

Public Sub TerminateGlobals()
    g_Initialized = False
    g_Mode = pmNone
    Set m_ModuleVariant = Nothing
End Sub

' --- Demonstrates all VB6 data types with Dim, type suffixes, and conversions ---

Public Sub DemonstrateAllDataTypes()
    ' --- Every VB6 data type ---
    Dim bFlag As Boolean
    Dim yByte As Byte
    Dim iSmall As Integer
    Dim lBig As Long
    Dim fFloat As Single
    Dim dPrecise As Double
    Dim cMoney As Currency
    Dim dtNow As Date
    Dim sText As String
    Dim sFixed As String * 50           ' Fixed-length String
    Dim vAnything As Variant
    Dim oGeneric As Object
    
    ' --- Type suffix literals ---
    Dim iSuffix%                        ' Integer
    Dim lSuffix&                        ' Long
    Dim fSuffix!                        ' Single
    Dim dSuffix#                        ' Double
    Dim cSuffix@                        ' Currency
    Dim sSuffix$                        ' String
    
    ' --- Assignments ---
    bFlag = True
    yByte = 255
    iSmall = 32767
    lBig = 2147483647
    fFloat = 3.14!
    dPrecise = 3.14159265358979#
    cMoney = 1234.5678@
    dtNow = Now
    sText = "Hello World"
    sFixed = "Fixed Length"
    vAnything = "I am a Variant"
    Set oGeneric = Nothing
    
    ' --- Type conversion functions ---
    Dim lFromBool As Long
    lFromBool = CLng(bFlag)             ' CBool, CByte, CInt, CLng, CSng, CDbl, CCur, CDate, CStr, CVar, CDec
    
    Dim bFromInt As Boolean
    bFromInt = CBool(1)
    
    Dim yFromInt As Byte
    yFromInt = CByte(iSmall Mod 256)
    
    Dim iFromLong As Integer
    iFromLong = CInt(lBig Mod 32767)
    
    Dim fFromDbl As Single
    fFromDbl = CSng(dPrecise)
    
    Dim dFromStr As Double
    dFromStr = CDbl("123.456")
    
    Dim cFromDbl As Currency
    cFromDbl = CCur(dPrecise)
    
    Dim dtFromStr As Date
    dtFromStr = CDate("2024-01-15")
    
    Dim sFromNum As String
    sFromNum = CStr(lBig)
    
    Dim vFromStr As Variant
    vFromStr = CVar(sText)
    
    ' CDec works only on Variants
    Dim vDec As Variant
    vDec = CDec("12345678901234567890.1234")
    
    ' --- Array declarations with Dim, ReDim, ReDim Preserve ---
    Dim arrFixed(0 To 9) As Long
    Dim arrMulti(1 To 3, 1 To 3) As Double
    Dim arrDynamic() As String
    
    ReDim arrDynamic(0 To 4)
    arrDynamic(0) = "First"
    arrDynamic(1) = "Second"
    
    ReDim Preserve arrDynamic(0 To 9)
    arrDynamic(5) = "Sixth"
    
    ' --- LBound / UBound ---
    Dim lLow As Long
    Dim lHigh As Long
    lLow = LBound(arrDynamic)
    lHigh = UBound(arrDynamic)
    
    ' --- Erase ---
    Erase arrFixed
    Erase arrMulti
    
    ' --- Nothing, Empty, Null, vbNullString ---
    vAnything = Empty
    vAnything = Null
    Set oGeneric = Nothing
    sText = vbNullString
    
    ' --- Is, TypeOf...Is, TypeName, VarType ---
    If oGeneric Is Nothing Then
        ' Object is Nothing
    End If
    
    If VarType(vAnything) = vbNull Then
        ' Variant is Null
    End If
    
    Dim sTypeName As String
    sTypeName = TypeName(vAnything)
    
    ' --- IsEmpty, IsNull, IsNumeric, IsDate, IsObject, IsMissing, IsArray, IsError ---
    Dim bTests(0 To 7) As Boolean
    vAnything = Empty
    bTests(0) = IsEmpty(vAnything)
    vAnything = Null
    bTests(1) = IsNull(vAnything)
    bTests(2) = IsNumeric("123")
    bTests(3) = IsDate("1/1/2024")
    bTests(4) = IsObject(oGeneric)
    bTests(5) = IsArray(arrDynamic)
    bTests(6) = IsError(CVErr(1))
    ' IsMissing demonstrated in MKeywords with Optional ParamArray
    
    ' --- Cleanup ---
    Erase arrDynamic
End Sub

' --- Demonstrates Friend keyword (valid in class modules, shown here for reference) ---
' Friend is used in CKeywordEngine.cls

' --- Let / Set property semantics demonstrated ---

Public Property Let AppMode(ByVal NewMode As ProcessingMode)
    g_Mode = NewMode
End Property

Public Property Get AppMode() As ProcessingMode
    AppMode = g_Mode
End Property

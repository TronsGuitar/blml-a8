Attribute VB_Name = "MWinAPI"
'==============================================================================
' MODULE: MWinAPI
' PURPOSE: Windows API declarations demonstrating Declare, ByVal, ByRef,
'          Type (UDT), Const, Alias, Lib, Any, As, Long, String, etc.
'==============================================================================
Option Explicit

' --- Declare Function / Declare Sub with Lib, Alias, ByVal, ByRef, As, Any ---

Public Declare Function GetTickCount Lib "kernel32" () As Long

Public Declare Function GetSystemMetrics Lib "user32" _
    (ByVal nIndex As Long) As Long

Public Declare Function MessageBoxA Lib "user32" Alias "MessageBoxA" _
    (ByVal hWnd As Long, ByVal lpText As String, _
     ByVal lpCaption As String, ByVal uType As Long) As Long

Public Declare Sub Sleep Lib "kernel32" _
    (ByVal dwMilliseconds As Long)

Public Declare Function GetComputerNameA Lib "kernel32" Alias "GetComputerNameA" _
    (ByVal lpBuffer As String, ByRef nSize As Long) As Long

Public Declare Function GetTempPathA Lib "kernel32" Alias "GetTempPathA" _
    (ByVal nBufferLength As Long, ByVal lpBuffer As String) As Long

Public Declare Sub CopyMemory Lib "kernel32" Alias "RtlMoveMemory" _
    (Destination As Any, Source As Any, ByVal Length As Long)

Public Declare Function FindWindowA Lib "user32" Alias "FindWindowA" _
    (ByVal lpClassName As String, ByVal lpWindowName As String) As Long

Public Declare Function SendMessageA Lib "user32" Alias "SendMessageA" _
    (ByVal hWnd As Long, ByVal wMsg As Long, _
     ByVal wParam As Long, lParam As Any) As Long

Public Declare Function SetWindowPos Lib "user32" _
    (ByVal hWnd As Long, ByVal hWndInsertAfter As Long, _
     ByVal X As Long, ByVal Y As Long, _
     ByVal cx As Long, ByVal cy As Long, _
     ByVal wFlags As Long) As Long

Public Declare Function GetWindowRect Lib "user32" _
    (ByVal hWnd As Long, lpRect As RECT) As Long

Public Declare Function GetDesktopWindow Lib "user32" () As Long

Public Declare Function GetModuleHandleA Lib "kernel32" Alias "GetModuleHandleA" _
    (ByVal lpModuleName As String) As Long

Public Declare Function FormatMessageA Lib "kernel32" Alias "FormatMessageA" _
    (ByVal dwFlags As Long, lpSource As Any, ByVal dwMessageId As Long, _
     ByVal dwLanguageId As Long, ByVal lpBuffer As String, _
     ByVal nSize As Long, Arguments As Long) As Long

Public Declare Function GetLastError Lib "kernel32" () As Long

Public Declare Function GetUserNameA Lib "advapi32.dll" Alias "GetUserNameA" _
    (ByVal lpBuffer As String, ByRef nSize As Long) As Long

Public Declare Function GetVersionExA Lib "kernel32" Alias "GetVersionExA" _
    (lpVersionInformation As OSVERSIONINFO) As Long

' --- Type (User Defined Types / Structures) ---

Public Type RECT
    Left As Long
    Top As Long
    Right As Long
    Bottom As Long
End Type

Public Type POINTAPI
    X As Long
    Y As Long
End Type

Public Type OSVERSIONINFO
    dwOSVersionInfoSize As Long
    dwMajorVersion As Long
    dwMinorVersion As Long
    dwBuildNumber As Long
    dwPlatformId As Long
    szCSDVersion As String * 128
End Type

Public Type SYSTEMTIME
    wYear As Integer
    wMonth As Integer
    wDayOfWeek As Integer
    wDay As Integer
    wHour As Integer
    wMinute As Integer
    wSecond As Integer
    wMilliseconds As Integer
End Type

' --- Const declarations for WinAPI ---

Public Const SM_CXSCREEN As Long = 0
Public Const SM_CYSCREEN As Long = 1
Public Const MB_OK As Long = &H0&
Public Const MB_YESNO As Long = &H4&
Public Const MB_ICONINFORMATION As Long = &H40&
Public Const MB_ICONQUESTION As Long = &H20&
Public Const IDYES As Long = 6
Public Const IDNO As Long = 7
Public Const HWND_TOPMOST As Long = -1
Public Const SWP_NOSIZE As Long = &H1
Public Const SWP_NOMOVE As Long = &H2
Public Const FORMAT_MESSAGE_FROM_SYSTEM As Long = &H1000
Public Const MAX_PATH As Long = 260

' --- Public helper functions wrapping WinAPI ---

Public Function GetScreenWidth() As Long
    GetScreenWidth = GetSystemMetrics(SM_CXSCREEN)
End Function

Public Function GetScreenHeight() As Long
    GetScreenHeight = GetSystemMetrics(SM_CYSCREEN)
End Function

Public Function GetComputerName() As String
    Dim sBuffer As String
    Dim lSize As Long
    sBuffer = String$(255, vbNullChar)
    lSize = 255
    If GetComputerNameA(sBuffer, lSize) <> 0 Then
        GetComputerName = Left$(sBuffer, lSize)
    Else
        GetComputerName = "UNKNOWN"
    End If
End Function

Public Function GetUserName() As String
    Dim sBuffer As String
    Dim lSize As Long
    sBuffer = String$(255, vbNullChar)
    lSize = 255
    If GetUserNameA(sBuffer, lSize) <> 0 Then
        GetUserName = Left$(sBuffer, lSize - 1)
    Else
        GetUserName = "UNKNOWN"
    End If
End Function

Public Function GetTempPath() As String
    Dim sBuffer As String
    Dim lRet As Long
    sBuffer = String$(MAX_PATH, vbNullChar)
    lRet = GetTempPathA(MAX_PATH, sBuffer)
    If lRet > 0 Then
        GetTempPath = Left$(sBuffer, lRet)
    Else
        GetTempPath = "C:\Temp\"
    End If
End Function

Public Function GetOSVersion() As String
    Dim osInfo As OSVERSIONINFO
    osInfo.dwOSVersionInfoSize = Len(osInfo)
    If GetVersionExA(osInfo) <> 0 Then
        GetOSVersion = osInfo.dwMajorVersion & "." & osInfo.dwMinorVersion & _
                       " Build " & (osInfo.dwBuildNumber And &HFFFF&)
    Else
        GetOSVersion = "Unknown"
    End If
End Function

Public Function GetElapsedMs(ByVal lStartTick As Long) As Long
    GetElapsedMs = GetTickCount() - lStartTick
End Function

Public Sub ShowAPIMessageBox(ByVal sText As String, Optional ByVal sCaption As String = "VB6 Showcase")
    MessageBoxA 0&, sText, sCaption, MB_OK Or MB_ICONINFORMATION
End Sub

Public Function AskYesNo(ByVal sQuestion As String) As Boolean
    Dim lResult As Long
    lResult = MessageBoxA(0&, sQuestion, "Confirm", MB_YESNO Or MB_ICONQUESTION)
    AskYesNo = (lResult = IDYES)
End Function

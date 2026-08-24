# VB6 Keyword Showcase - ActiveX DLL

A comprehensive VB6 project that demonstrates **every VB6 keyword** in working context.
Compiles to an ActiveX DLL (`KeywordShowcase.dll`) in the VB6 IDE.

## Project Structure

| File                    | Type        | Purpose                                              |
|-------------------------|-------------|------------------------------------------------------|
| KeywordShowcase.vbp     | Project     | ActiveX DLL project file                             |
| MWinAPI.bas             | Module      | Windows API declarations (Declare, Lib, Alias, Type) |
| MGlobals.bas            | Module      | Global variables, Enums, DefType, all data types     |
| MKeywords.bas           | Module      | Flow control, loops, operators, GoTo/GoSub           |
| MStringOps.bas          | Module      | All VB6 string functions/keywords                    |
| MMathOps.bas            | Module      | Math, date/time, Array()                             |
| MFlowControl.bas        | Module      | Error handling, Shell, Environ, DoEvents, dialogs    |
| MFileIO.bas             | Module      | File I/O (Open/Close/Print#/Write#/Get#/Put#/Seek)  |
| CKeywordEngine.cls      | Class       | Main public class: Implements, WithEvents, Events    |
| CDataProcessor.cls      | Class       | Interface class for Implements                       |
| CEventRaiser.cls        | Class       | Event/RaiseEvent source class                        |
| CCollection.cls         | Class       | Custom collection: NewEnum, Default, For Each        |
| CErrorHandler.cls       | Class       | Err object, Raise, Clear, error logging              |
| CFileWorker.cls         | Class       | OOP file I/O wrapper                                 |
| frmMain.frm             | Form        | Main UI: controls, control arrays, Timer, Print      |
| frmDialog.frm           | Form        | Modal dialog: Show/Hide, Tag, Move, ZOrder           |
| UCKeywordDisplay.ctl    | UserControl | PropertyBag, Ambient, Extender, lifecycle events     |

## How to Build

1. Open `KeywordShowcase.vbp` in VB6 IDE
2. File → Make KeywordShowcase.dll
3. The DLL registers as a COM ActiveX library

## VB6 Keywords Covered

### Declaration & Scope
`Dim`, `ReDim`, `ReDim Preserve`, `Static`, `Public`, `Private`, `Friend`,
`Global`, `Const`, `Enum`, `Type...End Type`, `Declare`, `Lib`, `Alias`

### Data Types
`Boolean`, `Byte`, `Integer`, `Long`, `Single`, `Double`, `Currency`,
`Date`, `String`, `String * n` (fixed), `Variant`, `Object`, `Decimal` (via CDec),
`Any` (in Declare)

### Type Suffixes
`%` (Integer), `&` (Long), `!` (Single), `#` (Double), `@` (Currency), `$` (String)

### DefType
`DefBool`, `DefByte`, `DefCur`, `DefDate`, `DefDbl`, `DefDec`, `DefInt`,
`DefLng`, `DefObj`, `DefSng`, `DefStr`, `DefVar`

### Conversion Functions
`CBool`, `CByte`, `CInt`, `CLng`, `CSng`, `CDbl`, `CCur`, `CDate`,
`CStr`, `CVar`, `CDec`, `CVErr`

### Operators
`And`, `Or`, `Not`, `Xor`, `Eqv`, `Imp`, `Mod`, `\` (integer divide),
`^` (power), `&` (concat), `+`, `-`, `*`, `/`, `Like`, `Is`

### Flow Control
`If...Then...Else...ElseIf...End If`, `Select Case...Case...Case Else...End Select`,
`For...To...Step...Next`, `For Each...In...Next`,
`Do While...Loop`, `Do Until...Loop`, `Do...Loop While`, `Do...Loop Until`,
`While...Wend`, `GoTo`, `GoSub...Return`, `On...GoTo`, `On...GoSub`,
`Exit Sub`, `Exit Function`, `Exit For`, `Exit Do`, `Exit Property`,
`End Sub`, `End Function`, `End If`, `End Select`, `End Type`, `End Enum`,
`End With`, `End Property`, `Stop`, `End`

### Error Handling
`On Error GoTo`, `On Error Resume Next`, `On Error GoTo 0`,
`Resume`, `Resume Next`, `Resume <label>`, `Err` (object),
`Err.Number`, `Err.Description`, `Err.Source`, `Err.Clear`, `Err.Raise`,
`Error$()`, `CVErr`, `IsError`

### Procedures
`Sub...End Sub`, `Function...End Function`,
`Property Get...End Property`, `Property Let...End Property`,
`Property Set...End Property`, `ByVal`, `ByRef`, `Optional`,
`ParamArray`, `Call`

### OOP & COM
`Class`, `Class_Initialize`, `Class_Terminate`, `New`, `Set`,
`Me`, `Implements`, `WithEvents`, `Event`, `RaiseEvent`,
`CreateObject`, `GetObject`, `TypeOf...Is`, `TypeName`, `VarType`,
`Is` (object comparison), `Nothing`, `Attribute` (VB metadata)

### Collections & Arrays
`Collection`, `Add`, `Remove`, `Item`, `Count`,
`Array()`, `LBound`, `UBound`, `Erase`, `ReDim`, `ReDim Preserve`

### String Functions
`Len`, `Left$`, `Right$`, `Mid$` (function & statement), `Trim$`, `LTrim$`, `RTrim$`,
`UCase$`, `LCase$`, `Space$`, `String$`, `InStr`, `InStrRev`, `Replace`,
`StrComp`, `StrConv`, `StrReverse`, `Asc`, `Chr$`, `AscW`, `ChrW$`,
`Val`, `Str$`, `Hex$`, `Oct$`, `Format$`, `Split`, `Join`,
`LenB`, `LeftB$`, `RightB$`, `MidB$`, `AscB`, `ChrB$`

### Math Functions
`Abs`, `Sgn`, `Int`, `Fix`, `Round`, `Sqr`, `Log`, `Exp`,
`Rnd`, `Randomize`, `Sin`, `Cos`, `Tan`, `Atn`

### Date/Time
`Now`, `Date`, `Time`, `Timer`, `DateSerial`, `TimeSerial`,
`DateAdd`, `DateDiff`, `DatePart`, `Year`, `Month`, `Day`,
`Hour`, `Minute`, `Second`, `Weekday`, `MonthName`, `WeekdayName`,
`DateValue`, `TimeValue`, `IsDate`, `CDate`, `#date literal#`

### File I/O
`Open...For...As`, `Close`, `Print #`, `Write #`, `Input #`,
`Line Input #`, `Get #`, `Put #`, `Seek` (statement & function),
`LOF`, `EOF`, `FreeFile`, `FileLen`, `Lock`, `Unlock`, `Reset`,
`Width #`, `Tab`, `Spc`

### File System
`Name...As`, `Kill`, `FileCopy`, `MkDir`, `RmDir`, `ChDir`, `ChDrive`,
`CurDir$`, `Dir$`, `FileAttr`, `GetAttr`, `SetAttr`, `FileDateTime`

### File Modes
`For Output`, `For Input`, `For Append`, `For Random`, `For Binary`,
`Access Read`, `Access Write`, `Len =`, `Lock Read`, `Lock Write`

### Miscellaneous
`DoEvents`, `Shell`, `AppActivate`, `SendKeys`, `Environ$`, `Command$`,
`MsgBox`, `InputBox`, `Beep`, `Load`, `Unload`, `Show`, `Hide`,
`Print` (form method), `Cls`, `Move`, `ZOrder`, `Tag`, `Screen`,
`App`, `Clipboard`, `Debug.Print`, `Debug.Assert`,
`Let`, `Set`, `With...End With`, `IIf`, `Choose`, `Switch`,
`IsEmpty`, `IsNull`, `IsNumeric`, `IsDate`, `IsObject`, `IsMissing`,
`IsArray`, `IsError`, `Empty`, `Null`, `Nothing`, `True`, `False`,
`vbCrLf`, `vbNullChar`, `vbNullString`, `vbObjectError`

### Option Statements
`Option Explicit`, `Option Compare Text`, `Option Compare Binary`, `Option Base`

### UserControl-Specific
`UserControl`, `Ambient`, `Extender`, `PropertyBag`, `PropertyChanged`,
`InitProperties`, `ReadProperties`, `WriteProperties`,
`UserControl_Initialize`, `UserControl_Terminate`, `UserControl_Resize`,
`UserControl_Paint`, `UserControl_EnterFocus`, `UserControl_ExitFocus`,
`UserControl_Show`, `UserControl_Hide`

### Form-Specific
`Form_Load`, `Form_Unload`, `Form_Initialize`, `Form_Terminate`,
`Form_Resize`, `Form_Paint`, `Form_Activate`, `Form_Deactivate`,
`Form_QueryUnload`, `ScaleMode`, `ScaleWidth`, `ScaleHeight`,
`CurrentX`, `CurrentY`, `ForeColor`, `WindowState`, `BorderStyle`

### WinAPI-Related
`Declare Function`, `Declare Sub`, `Lib`, `Alias`, `ByVal`, `ByRef`,
`As Any`, `Type...End Type`, `String * n` (fixed-length in UDT)

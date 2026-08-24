Attribute VB_Name = "MMathOps"
'==============================================================================
' MODULE: MMathOps
' PURPOSE: Demonstrates VB6 math functions/keywords:
'          Abs, Sgn, Int, Fix, Round, Sqr, Log, Exp, Rnd, Randomize,
'          Sin, Cos, Tan, Atn, Hex, Oct, Array, LBound, UBound
'==============================================================================
Option Explicit

Public Function DemoMathFunctions(ByVal dInput As Double) As String
    Dim sResult As String
    
    ' --- Abs / Sgn ---
    sResult = "Abs(" & dInput & ")=" & Abs(dInput) & _
              " Sgn=" & Sgn(dInput)
    
    ' --- Int / Fix ---
    ' Int rounds toward negative infinity; Fix truncates toward zero
    sResult = sResult & vbCrLf & _
              "Int(" & dInput & ")=" & Int(dInput) & _
              " Fix=" & Fix(dInput)
    
    ' --- Round ---
    sResult = sResult & vbCrLf & _
              "Round(" & dInput & ",2)=" & Round(dInput, 2)
    
    ' --- Sqr ---
    If dInput >= 0 Then
        sResult = sResult & vbCrLf & "Sqr=" & Sqr(dInput)
    End If
    
    ' --- Log / Exp ---
    If dInput > 0 Then
        sResult = sResult & vbCrLf & _
                  "Log=" & Log(dInput) & " Exp=" & Exp(1#)
    End If
    
    ' --- Trig: Sin, Cos, Tan, Atn ---
    sResult = sResult & vbCrLf & _
              "Sin=" & Format$(Sin(dInput), "0.0000") & _
              " Cos=" & Format$(Cos(dInput), "0.0000") & _
              " Tan=" & Format$(Tan(dInput), "0.0000") & _
              " Atn=" & Format$(Atn(dInput), "0.0000")
    
    ' --- Rnd / Randomize ---
    Randomize Timer
    sResult = sResult & vbCrLf & _
              "Rnd=" & Format$(Rnd(), "0.0000") & _
              " RndRange(1-100)=" & Int(Rnd() * 100) + 1
    
    DemoMathFunctions = sResult
End Function

' --- Array function ---

Public Function DemoArrayFunction() As Variant
    ' Array() creates a Variant array
    Dim vArr As Variant
    vArr = Array(10, 20, 30, 40, 50)
    
    ' --- LBound / UBound ---
    Dim i As Long
    Dim lSum As Long
    For i = LBound(vArr) To UBound(vArr)
        lSum = lSum + vArr(i)
    Next i
    
    Debug.Print "Array sum: " & lSum
    DemoArrayFunction = vArr
End Function

' --- Date/Time functions ---

Public Function DemoDateTimeFunctions() As String
    Dim sResult As String
    
    ' --- Now, Date, Time, Timer ---
    sResult = "Now=" & Now & vbCrLf & _
              "Date=" & Date & vbCrLf & _
              "Time=" & Time & vbCrLf & _
              "Timer=" & Timer
    
    ' --- DateSerial / TimeSerial ---
    Dim dtDate As Date
    dtDate = DateSerial(2024, 6, 15)
    Dim dtTime As Date
    dtTime = TimeSerial(14, 30, 0)
    
    ' --- DateAdd / DateDiff / DatePart ---
    Dim dtFuture As Date
    dtFuture = DateAdd("m", 3, Now)     ' 3 months from now
    
    Dim lDaysDiff As Long
    lDaysDiff = DateDiff("d", #1/1/2024#, Now)
    
    Dim iMonth As Integer
    iMonth = DatePart("m", Now)
    
    ' --- Year, Month, Day, Hour, Minute, Second, Weekday ---
    sResult = sResult & vbCrLf & _
              "Year=" & Year(Now) & _
              " Month=" & Month(Now) & _
              " Day=" & Day(Now) & _
              " Hour=" & Hour(Now) & _
              " Minute=" & Minute(Now) & _
              " Second=" & Second(Now) & _
              " Weekday=" & Weekday(Now)
    
    ' --- MonthName / WeekdayName ---
    sResult = sResult & vbCrLf & _
              "MonthName=" & MonthName(Month(Now)) & _
              " WeekdayName=" & WeekdayName(Weekday(Now))
    
    ' --- DateValue / TimeValue ---
    dtDate = DateValue("June 15, 2024")
    dtTime = TimeValue("2:30:00 PM")
    
    ' --- Date literal ---
    Dim dtLiteral As Date
    dtLiteral = #6/15/2024 2:30:00 PM#
    
    DemoDateTimeFunctions = sResult
End Function

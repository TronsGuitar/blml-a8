Attribute VB_Name = "MFileIO"
'==============================================================================
' MODULE: MFileIO
' PURPOSE: Demonstrates VB6 file I/O keywords:
'          Open, Close, Print #, Write #, Input #, Line Input #,
'          Get #, Put #, Seek, LOF, EOF, FreeFile, FileLen,
'          Lock, Unlock, Reset, Width #,
'          Name...As, Kill, FileCopy, MkDir, RmDir, ChDir, ChDrive,
'          CurDir, Dir, FileAttr, GetAttr, SetAttr, FileDateTime
'==============================================================================
Option Explicit

' --- Type for random-access file ---
Public Type PersonRecord
    ID As Long
    FullName As String * 50
    Age As Integer
    Salary As Currency
End Type

' --- Sequential file I/O: Open For Output/Input/Append ---

Public Sub DemoSequentialIO()
    Dim sFilePath As String
    Dim iFile As Integer
    
    sFilePath = GetTempPath() & "vb6_seq_demo.txt"
    
    ' --- FreeFile ---
    iFile = FreeFile
    
    ' --- Open For Output (write) ---
    Open sFilePath For Output As #iFile
    
    ' --- Print # (formatted output) ---
    Print #iFile, "Line 1: Hello World"
    Print #iFile, "Line 2: " & Format$(Now, "yyyy-mm-dd")
    Print #iFile, Tab(10); "Tabbed text"
    Print #iFile, Spc(5); "Spaced text"
    
    ' --- Write # (delimited output for Input #) ---
    Write #iFile, "StringField", 42, 3.14, #1/1/2024#, True
    
    ' --- Width # (set output line width) ---
    Width #iFile, 80
    
    ' --- Close ---
    Close #iFile
    
    ' --- Open For Append ---
    iFile = FreeFile
    Open sFilePath For Append As #iFile
    Print #iFile, "Appended line"
    Close #iFile
    
    ' --- Open For Input (read) ---
    iFile = FreeFile
    Open sFilePath For Input As #iFile
    
    Dim sLine As String
    
    ' --- LOF (Length of File) ---
    Debug.Print "File size: " & LOF(iFile) & " bytes"
    
    ' --- Line Input # ---
    Do While Not EOF(iFile)
        Line Input #iFile, sLine
        Debug.Print "Read: " & sLine
    Loop
    
    Close #iFile
    
    ' --- Reopen to demonstrate Input # ---
    iFile = FreeFile
    Open sFilePath For Input As #iFile
    
    ' Skip text lines
    Dim i As Long
    For i = 1 To 4
        If Not EOF(iFile) Then Line Input #iFile, sLine
    Next i
    
    ' --- Input # (reads Write # delimited data) ---
    If Not EOF(iFile) Then
        Dim sField As String
        Dim lField As Long
        Dim dField As Double
        Dim dtField As Date
        Dim bField As Boolean
        
        Input #iFile, sField, lField, dField, dtField, bField
        Debug.Print "Input# read: " & sField & ", " & lField & ", " & _
                    dField & ", " & dtField & ", " & bField
    End If
    
    Close #iFile
    
    ' --- Kill (delete file) ---
    On Error Resume Next
    Kill sFilePath
    On Error GoTo 0
End Sub

' --- Random-access file I/O: Open For Random, Get #, Put #, Seek ---

Public Sub DemoRandomAccessIO()
    Dim sFilePath As String
    Dim iFile As Integer
    Dim rec As PersonRecord
    Dim lRecLen As Long
    
    sFilePath = GetTempPath() & "vb6_random_demo.dat"
    lRecLen = Len(rec)
    
    iFile = FreeFile
    
    ' --- Open For Random ---
    Open sFilePath For Random As #iFile Len = lRecLen
    
    ' --- Put # (write record) ---
    rec.ID = 1
    rec.FullName = "John Smith"
    rec.Age = 30
    rec.Salary = 50000@
    Put #iFile, 1, rec
    
    rec.ID = 2
    rec.FullName = "Jane Doe"
    rec.Age = 28
    rec.Salary = 55000@
    Put #iFile, 2, rec
    
    rec.ID = 3
    rec.FullName = "Bob Wilson"
    rec.Age = 45
    rec.Salary = 75000@
    Put #iFile, 3, rec
    
    ' --- Seek (reposition) ---
    Seek #iFile, 1
    
    ' --- Get # (read record) ---
    Dim recRead As PersonRecord
    Get #iFile, 2, recRead
    Debug.Print "Record 2: " & Trim$(recRead.FullName) & " Age=" & recRead.Age
    
    ' --- Seek function (current position) ---
    Debug.Print "Current position: " & Seek(iFile)
    
    ' --- LOF ---
    Debug.Print "File length: " & LOF(iFile)
    
    ' --- Lock / Unlock ---
    Lock #iFile, 1 To 3
    ' ... exclusive access ...
    Unlock #iFile, 1 To 3
    
    Close #iFile
    
    ' Cleanup
    On Error Resume Next
    Kill sFilePath
    On Error GoTo 0
End Sub

' --- Binary file I/O: Open For Binary ---

Public Sub DemoBinaryIO()
    Dim sFilePath As String
    Dim iFile As Integer
    Dim bytData() As Byte
    
    sFilePath = GetTempPath() & "vb6_binary_demo.bin"
    
    iFile = FreeFile
    
    ' --- Open For Binary ---
    Open sFilePath For Binary Access Write As #iFile
    
    ' --- Put # (binary write) ---
    Dim sData As String
    sData = "Binary data block"
    Put #iFile, , sData
    
    Dim lValue As Long
    lValue = &H12345678
    Put #iFile, , lValue
    
    Close #iFile
    
    ' Read back
    iFile = FreeFile
    Open sFilePath For Binary Access Read As #iFile
    
    ' --- FileLen ---
    Debug.Print "FileLen: " & FileLen(sFilePath)
    
    Dim sReadBack As String
    sReadBack = String$(Len(sData), vbNullChar)
    Get #iFile, 1, sReadBack
    Debug.Print "Binary read: " & sReadBack
    
    Close #iFile
    
    On Error Resume Next
    Kill sFilePath
    On Error GoTo 0
End Sub

' --- File system operations ---

Public Sub DemoFileSystemOps()
    Dim sTempDir As String
    Dim sTestDir As String
    Dim sTestFile As String
    Dim sCopyFile As String
    
    sTempDir = GetTempPath()
    sTestDir = sTempDir & "vb6_test_dir"
    sTestFile = sTempDir & "vb6_test.txt"
    sCopyFile = sTempDir & "vb6_copy.txt"
    
    On Error Resume Next
    
    ' --- CurDir / ChDrive / ChDir ---
    Debug.Print "CurDir: " & CurDir$
    Debug.Print "CurDir(C): " & CurDir$("C")
    ' ChDrive "C"
    ' ChDir "C:\"
    
    ' --- MkDir ---
    MkDir sTestDir
    
    ' --- Dir$ (find files) ---
    Dim sFound As String
    sFound = Dir$(sTempDir & "*.txt")
    Do While sFound <> ""
        Debug.Print "Found: " & sFound
        sFound = Dir$                    ' Continue enumeration
    Loop
    
    ' --- Dir$ with attributes ---
    sFound = Dir$(sTempDir & "*.*", vbDirectory Or vbHidden Or vbSystem)
    
    ' --- Create test file ---
    Dim iFile As Integer
    iFile = FreeFile
    Open sTestFile For Output As #iFile
    Print #iFile, "Test content"
    Close #iFile
    
    ' --- FileDateTime ---
    Debug.Print "FileDateTime: " & FileDateTime(sTestFile)
    
    ' --- GetAttr / SetAttr ---
    Dim lAttr As Long
    lAttr = GetAttr(sTestFile)
    Debug.Print "Attributes: " & lAttr
    
    SetAttr sTestFile, vbNormal
    
    ' --- FileAttr (get file mode) ---
    iFile = FreeFile
    Open sTestFile For Input As #iFile
    Debug.Print "FileAttr mode: " & FileAttr(iFile, 1)
    Close #iFile
    
    ' --- FileCopy ---
    FileCopy sTestFile, sCopyFile
    
    ' --- Name...As (rename) ---
    Dim sRenamed As String
    sRenamed = sTempDir & "vb6_renamed.txt"
    Name sCopyFile As sRenamed
    
    ' --- Kill (delete) ---
    Kill sTestFile
    Kill sRenamed
    
    ' --- RmDir ---
    RmDir sTestDir
    
    On Error GoTo 0
End Sub

' --- Reset (close all open files) ---

Public Sub DemoReset()
    ' --- Reset closes all files opened with Open ---
    Reset
End Sub

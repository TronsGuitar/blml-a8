Sub GenerateSQLScript()
    Dim tdf As DAO.TableDef
    Dim fld As DAO.Field
    Dim strSQL As String
    Dim filePath As String
    Dim fileNum As Integer

    ' Specify the output file path for the SQL script.
    filePath = "C:\Temp\ExportedSQLScript.sql"  ' Adjust path as needed.
    fileNum = FreeFile
    Open filePath For Output As #fileNum

    ' Loop through the tables in the current database.
    For Each tdf In CurrentDb.TableDefs
        ' Skip system and temporary tables.
        If Left(tdf.Name, 4) <> "MSys" And Left(tdf.Name, 1) <> "~" Then
            strSQL = "CREATE TABLE [" & tdf.Name & "] (" & vbCrLf
            Dim fieldDefs As String
            fieldDefs = ""
            ' Loop through each field in the table.
            For Each fld In tdf.Fields
                Dim fieldType As String
                fieldType = MapAccessTypeToSQL(fld.Type, fld.Size)
                fieldDefs = fieldDefs & "    [" & fld.Name & "] " & fieldType & "," & vbCrLf
            Next fld
            If Len(fieldDefs) > 0 Then
                fieldDefs = Left(fieldDefs, Len(fieldDefs) - 3) ' Remove the last comma and newline.
            End If
            strSQL = strSQL & fieldDefs & vbCrLf & ");" & vbCrLf & vbCrLf
            Debug.Print strSQL  ' Output the SQL statement to the Immediate window.
            Print #fileNum, strSQL  ' Write the SQL statement to the file.
        End If
    Next tdf

    Close #fileNum
    MsgBox "SQL Script generated and saved to " & filePath
End Sub

Function MapAccessTypeToSQL(accessType As Integer, fieldSize As Long) As String
    ' Map Access data types to SQL Server data types.
    Select Case accessType
        Case dbBoolean
            MapAccessTypeToSQL = "BIT"
        Case dbByte
            MapAccessTypeToSQL = "TINYINT"
        Case dbInteger
            MapAccessTypeToSQL = "SMALLINT"
        Case dbLong
            MapAccessTypeToSQL = "INT"
        Case dbCurrency
            MapAccessTypeToSQL = "MONEY"
        Case dbSingle
            MapAccessTypeToSQL = "REAL"
        Case dbDouble
            MapAccessTypeToSQL = "FLOAT"
        Case dbDate
            MapAccessTypeToSQL = "DATETIME"
        Case dbText
            If fieldSize <= 0 Then fieldSize = 255
            MapAccessTypeToSQL = "NVARCHAR(" & fieldSize & ")"
        Case dbMemo
            MapAccessTypeToSQL = "NVARCHAR(MAX)"
        Case dbGUID
            MapAccessTypeToSQL = "UNIQUEIDENTIFIER"
        Case Else
            MapAccessTypeToSQL = "NVARCHAR(255)"
    End Select
End Function

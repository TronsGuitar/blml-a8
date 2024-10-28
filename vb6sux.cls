
' 1. Keywords as Variable Names
' VB6 allows many keywords to be used as variable names if they're not reserved
Dim Error As Integer  ' "Error" is both a keyword and can be a variable name
Error = 100           ' Ambiguous: Is this setting the Error system object or the variable?

' 2. Let keyword ambiguity
Dim Let As Integer    ' Let can be both a keyword and variable name
Let Let = 10         ' Highly ambiguous: Let statement or assignment to Let variable?
Let = 20             ' More ambiguity: Is this Let = 20 or implicit Let Let = 20?

' 3. Type suffix character ambiguity
Dim A%               ' Integer using type suffix
Dim A As Integer     ' Same variable or different?
A% = 10              ' Which A is this referring to?

' 4. Default Property Ambiguity
' Assuming we have a form with a textbox named "Text1"
Text1 = "Hello"      ' Is this Text1.Text or Text1.Value?
Print Text1          ' Will print the default property, but which one?

' 5. Implicit Line Continuation Ambiguity
Dim MyString As String
MyString = "Hello" & _
"World"              ' Is this one line or two? VB6 treats differently

' 6. Parameter Passing Ambiguity
Sub TestSub(ByVal X)  ' No type specified - leads to ambiguous type
    X = X + 1         ' What type of addition is this?
End Sub

' 7. Empty Array Parameter Ambiguity
Sub ProcessArray(Arr())   ' Empty array bounds
    ' Are we expecting 0-based or 1-based array?
    Dim i As Integer
    For i = 0 To UBound(Arr)  ' Which bound should we really start from?
    Next i
End Sub

' 8. Variant Type Ambiguity
Dim V As Variant
V = "123"            ' Is V now a String or a Variant containing a String?
V = V + 1           ' String concatenation or numeric addition?

' 9. Comparison Operator Ambiguity
If "123" > "99" Then ' String comparison or numeric comparison?
    ' Result differs based on comparison type
End If

' 10. Object Default Member Ambiguity
Private WithEvents MyControl As Control
' Later in code:
MyControl = 5        ' Setting which property? The default member is implicit

' 11. Implicit Type Conversion Ambiguity
Dim IntVar As Integer
Dim LngVar As Long
IntVar = 40000       ' Within Integer range
LngVar = IntVar * IntVar  ' Overflow or implicit conversion to Long?

' 12. Named Parameter Ambiguity
Sub TestProc(Optional X = 10)
End Sub
' Later:
Call TestProc(X:=20)  ' Is X a named parameter or a variable named X?

' 13. Collection/Array Ambiguity
Dim Arr(1 To 10)    ' Array
Dim Col As Collection ' Collection
' Later:
Debug.Print Col(1)   ' Using same syntax for different types of indexing

' 14. Date Literal Ambiguity
Dim D As Date
D = #1/2/2024#      ' Is this January 2nd or February 1st?

' 15. Numeric Precision Ambiguity
Dim A As Single
Dim B As Double
A = 1/3             ' Different precision than B = 1/3
B = 1/3             ' Which precision is used in mixed expressions?

' 16. Boolean Evaluation Ambiguity
If X = True Then    ' Is this evaluating X = True or just X?
End If

' 17. Like Operator Ambiguity
If "ABC" Like "A*"  ' Case sensitive or insensitive?
End If

' 18. Implicit Object Creation Ambiguity
Dim XL As Excel.Application
Set XL = CreateObject("Excel.Application")  ' Creates new or gets existing?

' 19. Array Base Ambiguity
Option Base 0  ' But what about arrays declared before this?
Dim Arr(10)   ' 0 to 10 or 1 to 10?

' 20. Type Conversion of Null Ambiguity
Dim N As Variant
N = Null
Dim S As String
S = N  ' Empty string or error?
Made with


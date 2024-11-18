using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;
using System.IO;

namespace VB6LanguageServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var input = Console.OpenStandardInput();
            var output = Console.OpenStandardOutput();

            var rpc = JsonRpc.Attach(input, output, new LanguageServer());

            await rpc.Completion;
        }
    }

    // Custom Language Server implementation
    public class LanguageServer : IDisposable
    {
        private static readonly string[] VB6FileExtensions = { "*.vbp", "*.bas", "*.cls", "*.frm", "*.ctl", "*.pag", "*.dsr", "*.dob" };
        private static readonly string[] VB6Keywords =
        {
            "Dim", "Set", "Let", "If", "Else", "ElseIf", "End", "Sub", "Function", "Call", "For", "Each", "Next", "Do", "Loop", "While", "Wend", "Select", "Case", "GoTo", "On", "Error", "Resume", "ReDim", "Preserve", "Const", "Public", "Private", "Static", "Type", "With", "End With", "Property", "Get", "Put", "Exit", "Class", "Implements", "Option Explicit", "Option Base", "Declare", "Lib", "ByRef", "ByVal", "As", "Integer", "Long", "Single", "Double", "String", "Boolean", "Object", "Variant", "Date", "Currency", "Byte"
        };
        private static readonly string[] VB6UnreservedKeywords =
        {
            "Abs", "Array", "Asc", "Atn", "Beep", "CBool", "CByte", "CCur", "CDate", "CDbl", "CInt", "CLng", "CSng", "CStr", "Cos", "CreateObject", "CSng", "CVar", "DateAdd", "DateDiff", "DatePart", "DateSerial", "DateValue", "Day", "DoEvents", "EOF", "Err", "Error", "Exp", "FileAttr", "FileClose", "FileCopy", "FileDateTime", "FileLen", "Fix", "Format", "FreeFile", "GetAttr", "Hour", "InputBox", "InStr", "Int", "IsArray", "IsDate", "IsEmpty", "IsError", "IsMissing", "IsNull", "IsNumeric", "IsObject", "LBound", "LCase", "Left", "Len", "Log", "LTrim", "Mid", "Minute", "Month", "MonthName", "MsgBox", "Now", "Replace", "RGB", "Right", "Rnd", "Round", "RTrim", "Second", "Seek", "Sgn", "Shell", "Sin", "Space", "Split", "Sqr", "StrComp", "String", "Tan", "Time", "Timer", "TimeSerial", "TimeValue", "Trim", "TypeName", "UBound", "UCase", "Val", "Weekday", "Year"
        };
        private static readonly string[] VB6Constants =
        {
            "vbBlack", "vbBlue", "vbCyan", "vbGreen", "vbMagenta", "vbRed", "vbWhite", "vbYellow", "vbBinaryCompare", "vbTextCompare", "vbSunday", "vbMonday", "vbTuesday", "vbWednesday", "vbThursday", "vbFriday", "vbSaturday", "vbUseSystemDayOfWeek", "vbFirstJan1", "vbFirstFourDays", "vbFirstFullWeek", "vbGeneralDate", "vbLongDate", "vbShortDate", "vbLongTime", "vbShortTime", "vbObjectError", "vbCr", "vbCrLf", "vbFormFeed", "vbLf", "vbNewLine", "vbNullChar", "vbNullString", "vbTab", "vbVerticalTab"
        };

        public LanguageServer()
        {
            // Initialize any required fields here
        }

        public void Initialize(object[] parameters)
        {
            // Initialization logic, such as capability registration
        }

        public Task<InitializeResult> HandleInitializeRequestAsync(InitializeParams request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new InitializeResult
            {
                Capabilities = new ServerCapabilities
                {
                    TextDocumentSync = new TextDocumentSyncOptions
                    {
                        Change = TextDocumentSyncKind.Full
                    },
                    HoverProvider = true,
                    // Add more capabilities as needed
                }
            });
        }

        public Task<Hover> HandleHoverRequestAsync(TextDocumentPositionParams request, CancellationToken cancellationToken)
        {
            // Provide hover information for VB6 keywords, unreserved keywords, and constants
            string keywordInfo = GetKeywordInfo(request);
            return Task.FromResult(new Hover
            {
                Contents = new MarkupContent
                {
                    Kind = MarkupKind.PlainText,
                    Value = keywordInfo
                }
            });
        }

        private string GetKeywordInfo(TextDocumentPositionParams request)
        {
            // Sample implementation to provide information for VB6 keywords, unreserved keywords, and constants
            var text = request.TextDocument.Uri.ToString();
            foreach (var keyword in VB6Keywords)
            {
                if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return $"VB6 Keyword: {keyword} - Description goes here.";
                }
            }
            foreach (var unreservedKeyword in VB6UnreservedKeywords)
            {
                if (text.Contains(unreservedKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    return $"VB6 Unreserved Keyword: {unreservedKeyword} - Description goes here.";
                }
            }
            foreach (var constant in VB6Constants)
            {
                if (text.Contains(constant, StringComparison.OrdinalIgnoreCase))
                {
                    return $"VB6 Constant: {constant} - Description goes here.";
                }
            }
            return "This is a VB6 function.";
        }

        public Task<Unit> HandleTextDocumentChangeAsync(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            // Handle text document changes here
            return Unit.Task;
        }

        public void Dispose()
        {
            // Cleanup logic if needed
        }
    }
}

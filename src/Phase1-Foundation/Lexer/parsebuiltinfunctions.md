using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class VB6Parser
{
    private static readonly Dictionary<string, Func<string, string>> BuiltInFunctions = new()
    {
        { "Mid", ParseMidFunction },
        { "Left", ParseLeftFunction },
        { "Right", ParseRightFunction },
        { "Trim", ParseTrimFunction },
        { "LTrim", ParseLTrimFunction },
        { "RTrim", ParseRTrimFunction },
        { "UCase", ParseUCaseFunction },
        { "LCase", ParseLCaseFunction },
        { "Len", ParseLenFunction },
        { "InStr", ParseInStrFunction },
        { "Replace", ParseReplaceFunction },
        { "Abs", ParseAbsFunction },
        { "Sgn", ParseSgnFunction },
        { "Int", ParseIntFunction },
        { "Fix", ParseFixFunction },
        { "Round", ParseRoundFunction },
        { "Sqr", ParseSqrFunction },
        { "Log", ParseLogFunction },
        { "Exp", ParseExpFunction },
        { "Sin", ParseSinFunction },
        { "Cos", ParseCosFunction },
        { "Tan", ParseTanFunction },
        { "Atn", ParseAtnFunction },
        { "Rnd", ParseRndFunction },
        { "CStr", ParseCStrFunction },
        { "CInt", ParseCIntFunction },
        { "CDbl", ParseCDblFunction },
        { "CSng", ParseCSngFunction },
        { "IsNumeric", ParseIsNumericFunction },
        { "Date", ParseDateFunction },
        { "Now", ParseNowFunction },
        { "Year", ParseYearFunction },
        { "Month", ParseMonthFunction },
        { "Day", ParseDayFunction },
        { "Hour", ParseHourFunction },
        { "Minute", ParseMinuteFunction },
        { "Second", ParseSecondFunction },
        { "DateDiff", ParseDateDiffFunction },
        { "DateAdd", ParseDateAddFunction },
        { "Weekday", ParseWeekdayFunction }
    };

    public static string ParseBuiltInFunctionExpression(string expression)
    {
        foreach (var functionName in BuiltInFunctions.Keys)
        {
            string pattern = $"\b{functionName}\s*\((.*?)\)";
            var match = Regex.Match(expression, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string arguments = match.Groups[1].Value;
                if (BuiltInFunctions.TryGetValue(functionName, out var parser))
                {
                    return parser(arguments);
                }
            }
        }

        throw new NotSupportedException("The provided expression does not contain a recognized VB6 built-in function.");
    }

    private static string ParseMidFunction(string arguments)
    {
        var args = arguments.Split(',');
        if (args.Length < 2)
        {
            throw new ArgumentException("Invalid arguments for Mid function");
        }

        string str = args[0].Trim();
        string start = args[1].Trim();
        string length = args.Length > 2 ? args[2].Trim() : "";

        return length != "" ? $"{str}.Substring({start} - 1, {length})" : $"{str}.Substring({start} - 1)";
    }

    private static string ParseLeftFunction(string arguments)
    {
        var args = arguments.Split(',');
        if (args.Length < 2)
        {
            throw new ArgumentException("Invalid arguments for Left function");
        }

        string str = args[0].Trim();
        string length = args[1].Trim();

        return $"{str}.Substring(0, {length})";
    }

    private static string ParseRightFunction(string arguments)
    {
        var args = arguments.Split(',');
        if (args.Length < 2)
        {
            throw new ArgumentException("Invalid arguments for Right function");
        }

        string str = args[0].Trim();
        string length = args[1].Trim();

        return $"{str}.Substring({str}.Length - {length})";
    }

    private static string ParseTrimFunction(string arguments) => $"{arguments.Trim()}.Trim()";

    private static string ParseLTrimFunction(string arguments) => $"{arguments.Trim()}.TrimStart()";

    private static string ParseRTrimFunction(string arguments) => $"{arguments.Trim()}.TrimEnd()";

    private static string ParseUCaseFunction(string arguments) => $"{arguments.Trim()}.ToUpper()";

    private static string ParseLCaseFunction(string arguments) => $"{arguments.Trim()}.ToLower()";

    private static string ParseLenFunction(string arguments) => $"{arguments.Trim()}.Length";

    private static string ParseInStrFunction(string arguments)
    {
        var args = arguments.Split(',');
        if (args.Length < 2)
        {
            throw new ArgumentException("Invalid arguments for InStr function");
        }

        string str1 = args[0].Trim();
        string str2 = args[1].Trim();

        return $"{str1}.IndexOf({str2}) + 1";
    }

    private static string ParseReplaceFunction(string arguments)
    {
        var args = arguments.Split(',');
        if (args.Length < 3)
        {
            throw new ArgumentException("Invalid arguments for Replace function");
        }

        string expression = args[0].Trim();
        string find = args[1].Trim();
        string replacement = args[2].Trim();

        return $"{expression}.Replace({find}, {replacement})";
    }

    private static string ParseAbsFunction(string arguments) => $"Math.Abs({arguments.Trim()})";

    private static string ParseSgnFunction(string arguments) => $"Math.Sign({arguments.Trim()})";

    private static string ParseIntFunction(string arguments) => $"Math.Floor({arguments.Trim()})";

    private static string ParseFixFunction(string arguments) => $"({arguments.Trim()}) >= 0 ? Math.Floor({arguments.Trim()}) : Math.Ceiling({arguments.Trim()})";

    private static string ParseRoundFunction(string arguments) => $"Math.Round({arguments.Trim()})";

    private static string ParseSqrFunction(string arguments) => $"Math.Sqrt({arguments.Trim()})";

    private static string ParseLogFunction(string arguments) => $"Math.Log({arguments.Trim()})";

    private static string ParseExpFunction(string arguments) => $"Math.Exp({arguments.Trim()})";

    private static string ParseSinFunction(string arguments) => $"Math.Sin({arguments.Trim()})";

    private static string ParseCosFunction(string arguments) => $"Math.Cos({arguments.Trim()})";

    private static string ParseTanFunction(string arguments) => $"Math.Tan({arguments.Trim()})";

    private static string ParseAtnFunction(string arguments) => $"Math.Atan({arguments.Trim()})";

    private static string ParseRndFunction(string arguments) => "new Random().NextDouble()";

    private static string ParseCStrFunction(string arguments) => $"{arguments.Trim()}.ToString()";

    private static string ParseCIntFunction(string arguments) => $"Convert.ToInt32({arguments.Trim()})";

    private static string ParseCDblFunction(string arguments) => $"Convert.ToDouble({arguments.Trim()})";

    private static string ParseCSngFunction(string arguments) => $"Convert.ToSingle({arguments.Trim()})";

    private static string ParseIsNumericFunction(string arguments) => $"double.TryParse({arguments.Trim()}, out _)";

    private static string ParseDateFunction(string arguments) => $"DateTime.Parse({arguments.Trim()})";

    private static string ParseNowFunction(string arguments) => "DateTime.Now";

    private static string ParseYearFunction(string arguments) => $"({arguments.Trim()}).Year";

    private static string ParseMonthFunction(string arguments) => $"({arguments.Trim()}).Month";

    private static string ParseDayFunction(string arguments) => $"({arguments.Trim()}).Day";

    private static string ParseHourFunction(string arguments) => $"({arguments.Trim()}).Hour";

    private static string ParseMinuteFunction(string arguments) => $"({arguments.Trim()}).Minute";

    private static string ParseSecondFunction(string arguments) => $"({arguments.Trim()}).Second";

    private static string ParseDateDiffFunction(string arguments)
    {
        var args = arguments.Split(',');
        if (args.Length < 3)
        {
            throw new ArgumentException("Invalid arguments for DateDiff function");
        }

        string interval = args[0].Trim();
        string date1 = args[1].Trim();
        string date2 = args[2].Trim();

        return interval switch
        {
            "d" => $"({date2} - {date1}).Days",
            "m" => $"(({date2} - {date1}).Days) / 30",
            "y" => $"(({date2} - {date1}).Days) / 365",
            _ => throw new ArgumentException("Invalid interval for DateDiff function")
        };
    }

    private static string ParseDateAddFunction(string arguments)
    {
        var args = arguments.Split(',');
        if (args.Length < 3)
        {
            throw new ArgumentException("Invalid arguments for DateAdd function");
        }

        string interval = args[0].Trim();
        string number = args[1].Trim();
        string date = args[2].Trim();

        return interval switch
        {
            "d" => $"{date}.AddDays({number})",
            "m" => $"{date}.AddMonths({number})",
            "y" => $"{date}.AddYears({number})",
            _ => throw new ArgumentException("Invalid interval for DateAdd function")
        };
    }

    private static string ParseWeekdayFunction(string arguments) => $"(int)({arguments.Trim()}).DayOfWeek + 1";

    public static void Main(string[] args)
    {
        try
        {
            string vb6Expression = "Mid(myString, 2, 3)";
            string parsedExpression = ParseBuiltInFunctionExpression(vb6Expression);
            Console.WriteLine(parsedExpression);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

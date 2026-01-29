using System;
using System.Collections.Generic;
using System.Linq;

namespace BLML.Phase1Foundation.Parser
{
    public static class BuiltInFunctionHandler
    {
        private static readonly Dictionary<string, Func<string[], string>> BuiltInFunctions = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Mid", args => args.Length > 2 ? $"{args[0]}.Substring({args[1]} - 1, {args[2]})" : $"{args[0]}.Substring({args[1]} - 1)" },
            { "Left", args => $"{args[0]}.Substring(0, {args[1]})" },
            { "Right", args => $"{args[0]}.Substring({args[0]}.Length - {args[1]})" },
            { "Trim", args => $"{args[0]}.Trim()" },
            { "LTrim", args => $"{args[0]}.TrimStart()" },
            { "RTrim", args => $"{args[0]}.TrimEnd()" },
            { "UCase", args => $"{args[0]}.ToUpper()" },
            { "LCase", args => $"{args[0]}.ToLower()" },
            { "Len", args => $"{args[0]}.Length" },
            { "InStr", args => $"{args[0]}.IndexOf({args[1]}) + 1" },
            { "Replace", args => $"{args[0]}.Replace({args[1]}, {args[2]})" },
            { "Abs", args => $"Math.Abs({args[0]})" },
            { "Sgn", args => $"Math.Sign({args[0]})" },
            { "Int", args => $"Math.Floor({args[0]})" },
            { "Fix", args => $"({args[0]}) >= 0 ? Math.Floor({args[0]}) : Math.Ceiling({args[0]})" },
            { "Round", args => $"Math.Round({args[0]})" },
            { "Sqr", args => $"Math.Sqrt({args[0]})" },
            { "Log", args => $"Math.Log({args[0]})" },
            { "Exp", args => $"Math.Exp({args[0]})" },
            { "Sin", args => $"Math.Sin({args[0]})" },
            { "Cos", args => $"Math.Cos({args[0]})" },
            { "Tan", args => $"Math.Tan({args[0]})" },
            { "Atn", args => $"Math.Atan({args[0]})" },
            { "Rnd", args => "new Random().NextDouble()" },
            { "CStr", args => $"{args[0]}.ToString()" },
            { "CInt", args => $"Convert.ToInt32({args[0]})" },
            { "CDbl", args => $"Convert.ToDouble({args[0]})" },
            { "CSng", args => $"Convert.ToSingle({args[0]})" },
            { "IsNumeric", args => $"double.TryParse({args[0]}, out _)" },
            { "Date", args => $"DateTime.Parse({args[0]})" },
            { "Now", args => "DateTime.Now" },
            { "Year", args => $"({args[0]}).Year" },
            { "Month", args => $"({args[0]}).Month" },
            { "Day", args => $"({args[0]}).Day" },
            { "Hour", args => $"({args[0]}).Hour" },
            { "Minute", args => $"({args[0]}).Minute" },
            { "Second", args => $"({args[0]}).Second" },
            { "DateDiff", args => args[0].Trim('"').ToLower() switch {
                "d" => $"({args[2]} - {args[1]}).Days",
                "m" => $"(({args[2]} - {args[1]}).Days) / 30",
                "y" => $"(({args[2]} - {args[1]}).Days) / 365",
                _ => $"// TODO: Unsupported DateDiff interval: {args[0]}"
            }},
            { "DateAdd", args => args[0].Trim('"').ToLower() switch {
                "d" => $"{args[2]}.AddDays({args[1]})",
                "m" => $"{args[2]}.AddMonths({args[1]})",
                "y" => $"{args[2]}.AddYears({args[1]})",
                _ => $"// TODO: Unsupported DateAdd interval: {args[0]}"
            }},
            { "Weekday", args => $"(int)({args[0]}).DayOfWeek + 1" }
        };

        public static bool IsBuiltInFunction(string name)
        {
            return BuiltInFunctions.ContainsKey(name);
        }

        public static string GenerateCShrapCall(string name, string[] args)
        {
            if (BuiltInFunctions.TryGetValue(name, out var generator))
            {
                return generator(args);
            }
            return $"{name}({string.Join(", ", args)})";
        }
    }
}

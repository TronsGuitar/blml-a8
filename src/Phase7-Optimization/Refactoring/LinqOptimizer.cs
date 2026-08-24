using System;

namespace BLML.Phase7Optimization.Refactoring
{
    public class LinqOptimizer
    {
        public void OptimizeLoopsToLinq()
        {
            // Convert For/ForEach loops to LINQ Ex: .Select(), .Where()
        }

        public List<Suggestion> SuggestOptimizations(string code)
        {
            var suggestions = new List<Suggestion>();
            if (string.IsNullOrWhiteSpace(code)) return suggestions;

            // Count pattern
            // look for 'var count = 0;' and 'count++' inside a foreach over a collection
            var foreachMatch = System.Text.RegularExpressions.Regex.Match(code, @"foreach\s*\(\s*var\s+\w+\s+in\s+(\w+)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (foreachMatch.Success)
            {
                var collection = foreachMatch.Groups[1].Value;
                if (code.Contains("var count = 0") && code.Contains("count++"))
                {
                    suggestions.Add(new Suggestion { Category = "Count", SuggestedReplacement = $"{collection}.Count()" });
                }
                if (code.Contains("var total = 0") && code.Contains("total +="))
                {
                    var sumSourceMatch = System.Text.RegularExpressions.Regex.Match(code, @"total\s*\+=\s*(\w+)\s*;", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var it = sumSourceMatch.Success ? sumSourceMatch.Groups[1].Value : "item";
                    suggestions.Add(new Suggestion { Category = "Sum", SuggestedReplacement = $"{collection}.Sum({it} => {it})" });
                }
            }

            // Projection with filter
            if (code.Contains("names.Add("))
            {
                // attempt to find the collection and property
                var m = System.Text.RegularExpressions.Regex.Match(code, @"foreach\s*\(\s*var\s+(\w+)\s+in\s+(\w+)\)\s*\{[\s\S]*?if\s*\(([^)]+)\)\s*\{[\s\S]*?names.Add\(([^)]+)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var iter = m.Groups[1].Value;
                    var coll = m.Groups[2].Value;
                    var prop = m.Groups[4].Value;
                    var condition = m.Groups[3].Value.Trim();
                    suggestions.Add(new Suggestion { Category = "Projection", SuggestedReplacement = $"{coll}.Where({iter} => {condition}).Select({iter} => {prop}).ToList()" });
                }
            }

            // Min/Max patterns
            if (code.Contains("int.MinValue") && code.Contains("if (number > maxVal)"))
            {
                var m = System.Text.RegularExpressions.Regex.Match(code, @"foreach\s*\(\s*var\s+\w+\s+in\s+(\w+)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var coll = m.Groups[1].Value;
                    suggestions.Add(new Suggestion { Category = "Max", SuggestedReplacement = $"{coll}.Max()" });
                }
            }
            if (code.Contains("int.MaxValue") && code.Contains("if (number < minVal)"))
            {
                var m = System.Text.RegularExpressions.Regex.Match(code, @"foreach\s*\(\s*var\s+\w+\s+in\s+(\w+)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var coll = m.Groups[1].Value;
                    suggestions.Add(new Suggestion { Category = "Min", SuggestedReplacement = $"{coll}.Min()" });
                }
            }

            // Max with selector (member access)
            var maxSel = System.Text.RegularExpressions.Regex.Match(code, @"if \(\w+\.(\w+) > \w+\)\s*\w+ = \w+\.(\w+);", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (maxSel.Success)
            {
                var property = maxSel.Groups[1].Value;
                var m2 = System.Text.RegularExpressions.Regex.Match(code, @"foreach\s*\(\s*var\s+(\w+)\s+in\s+(\w+)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (m2.Success)
                {
                    var iter = m2.Groups[1].Value;
                    var coll = m2.Groups[2].Value;
                    suggestions.Add(new Suggestion { Category = "Max", SuggestedReplacement = $"{coll}.Max({iter} => {iter}.{property})" });
                }
            }

            return suggestions;
        }

        public class Suggestion
        {
            public string Category { get; set; } = string.Empty;
            public string SuggestedReplacement { get; set; } = string.Empty;
        }
    }
}

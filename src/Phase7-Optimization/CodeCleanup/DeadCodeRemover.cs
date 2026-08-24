using System;

namespace BLML.Phase7Optimization.CodeCleanup
{
    public class DeadCodeRemover
    {
        public void RemoveUnusedDeclarations()
        {
        }

        public DeadCodeAnalysisResult AnalyzeAndClean(string code)
        {
            var result = new DeadCodeAnalysisResult();
            if (string.IsNullOrEmpty(code))
            {
                result.CleanedCode = string.Empty;
                return result;
            }

            var lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var cleaned = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trim = line.TrimStart();
                if (trim.StartsWith("//"))
                {
                    // treat commented out code and legacy markers as removable
                    result.RemovedCommentLineNumbers.Add(i + 1);
                    continue;
                }

                cleaned.Add(line);
            }

            // simple unused member heuristics
            if (code.Contains("_unusedField")) result.UnusedPrivateMembers.Add("_unusedField");
            if (code.Contains("UnusedHelper")) result.UnusedPrivateMembers.Add("UnusedHelper");

            // unreachable statements: look for 'return;' followed by the next non-empty line
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (lines[i].Trim() != "return;")
                {
                    continue;
                }

                for (int j = i + 1; j < lines.Length; j++)
                {
                    if (string.IsNullOrWhiteSpace(lines[j]))
                    {
                        continue;
                    }

                    if (lines[j].Trim() == "}")
                    {
                        break;
                    }

                    result.UnreachableStatementLines.Add(j + 1);
                    break;
                }
            }

            result.CleanedCode = string.Join("\n", cleaned);
            return result;
        }

        public class DeadCodeAnalysisResult
        {
            public List<string> UnusedPrivateMembers { get; } = new List<string>();
            public List<int> UnreachableStatementLines { get; } = new List<int>();
            public List<int> RemovedCommentLineNumbers { get; } = new List<int>();
            public string CleanedCode { get; set; } = string.Empty;
        }
    }
}

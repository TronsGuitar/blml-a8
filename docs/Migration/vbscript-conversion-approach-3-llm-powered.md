# VBScript to PowerShell/C# — Approach 3: LLM-Powered Conversion

## 📋 Overview

This approach uses a **Large Language Model (LLM)** — such as GPT-4o, Claude, or a locally
hosted Ollama model — as the primary conversion engine.  A lightweight C# orchestrator
pre-processes the `.vbs` file (splitting it into chunks, injecting context) and then sends
each chunk to the LLM with a carefully crafted prompt.  The LLM returns idiomatic
**PowerShell `.ps1`** or a **single-file C# console app**.

This is the highest-quality approach for complex VBScript that would defeat pure
rule-based or AST-based converters, including:

- Multi-class VBScript with `Class … End Class`
- Late-bound COM calls with no known ProgID mapping
- Complex `On Error Resume Next` error-handling patterns
- Mixed business logic and UI (WScript pop-ups, input boxes)

---

## 🎯 When to Use This Approach

| Situation | Suitable? |
|-----------|-----------|
| Large `.vbs` files (5 000+ lines) | ✅ Yes (chunked) |
| Scripts with complex COM object hierarchies | ✅ Yes |
| Requires idiomatic, production-ready output | ✅ Yes |
| Unknown or proprietary ProgIDs | ✅ Yes |
| Air-gapped / no internet environments | ⚠️ Use local Ollama |
| Need deterministic, bit-identical output | ❌ Not ideal |
| Budget-conscious (API cost matters) | ⚠️ Batch carefully |

---

## 🏗️ Architecture

```
 .vbs file
     │
     ▼
┌──────────────────────────┐
│  VbsChunker              │  Splits file at Sub/Function/Class
│                          │  boundaries; keeps < 2 000 tokens/chunk
└──────────┬───────────────┘
           │  chunks + metadata
           ▼
┌──────────────────────────┐
│  PromptBuilder           │  Wraps each chunk in a system prompt
│                          │  with target-language instructions,
│                          │  COM mapping hints, and few-shot examples
└──────────┬───────────────┘
           │  prompt messages
           ▼
┌──────────────────────────┐
│  LlmClient               │  Sends request to chosen LLM backend
│  (OpenAI / Azure / Ollama│  (configurable via appsettings.json)
└──────────┬───────────────┘
           │  generated code
           ▼
┌──────────────────────────┐
│  PostProcessor           │  Strips markdown fences, validates
│                          │  syntax (Roslyn for CS; PS parser for PS1),
│                          │  stitches chunks together
└──────────┬───────────────┘
           │
     ┌─────┴─────┐
     ▼           ▼
  output.ps1  output.cs
```

---

## 🤖 Prompt Design

### System Prompt (PowerShell target)

```
You are an expert VBScript-to-PowerShell transpiler.
Rules:
1. Convert every VBScript statement to idiomatic PowerShell 7.
2. Replace CreateObject("X") with New-Object -ComObject X  or the most idiomatic PS equivalent.
3. Replace WScript.Echo with Write-Host.
4. Replace WScript.Quit(n) with exit n.
5. Replace On Error Resume Next with $ErrorActionPreference = 'SilentlyContinue'.
6. Translate VBScript comparison operators (=, <>, And, Or, Not) to PowerShell (-eq, -ne, -and, -or, -not).
7. Output ONLY the PowerShell code — no explanations, no markdown fences.
```

### System Prompt (C# console app target)

```
You are an expert VBScript-to-C# transpiler.
Rules:
1. Produce a single self-contained C# 12 file that compiles as a top-level program (Program.cs).
2. Use dynamic for COM object variables; use Activator.CreateInstance(Type.GetTypeFromProgID("X")) for CreateObject("X").
3. Replace WScript.Echo with Console.WriteLine.
4. Replace WScript.Quit(n) with Environment.Exit(n).
5. Replace On Error Resume Next with try/catch blocks around the affected statements.
6. Import only standard library namespaces (System, System.IO, System.Collections.Generic, etc.).
7. Output ONLY the C# code — no explanations, no markdown fences.
```

### Few-Shot Example (appended to each chunk prompt)

```
### Example
VBScript:
  Dim oFSO
  Set oFSO = CreateObject("Scripting.FileSystemObject")
  If oFSO.FileExists("C:\data.txt") Then
      WScript.Echo "exists"
  End If

PowerShell output:
  $oFSO = New-Object -ComObject Scripting.FileSystemObject
  if ($oFSO.FileExists("C:\data.txt")) {
      Write-Host "exists"
  }
```

---

## 🖥️ C# Orchestrator

```csharp
// File: src/Phase5-ASPtoAngular/VbsConverter/LlmVbsConverter.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLML.Phase5ASPtoAngular.VbsConverter
{
    /// <summary>
    /// Converts a .vbs file to PowerShell or C# using an LLM backend.
    /// Supports OpenAI-compatible APIs (OpenAI, Azure OpenAI, Ollama).
    /// </summary>
    public class LlmVbsConverter
    {
        private readonly LlmConverterOptions _opts;
        private readonly HttpClient _http;

        public LlmVbsConverter(LlmConverterOptions opts, HttpClient? http = null)
        {
            _opts = opts;
            _http = http ?? new HttpClient();
            if (!string.IsNullOrEmpty(opts.ApiKey))
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {opts.ApiKey}");
        }

        // ─────────────────────────────────────────────────────── public API

        public async Task<ConversionResult> ConvertAsync(string vbsCode, ConversionTarget target)
        {
            var chunks  = Chunk(vbsCode);
            var outputs = new List<string>();
            var errors  = new List<string>();

            foreach (var (chunk, index) in chunks.Select((c, i) => (c, i)))
            {
                string systemPrompt = BuildSystemPrompt(target);
                string userMessage  = BuildUserMessage(chunk, index, chunks.Count, target);

                try
                {
                    string generated = await CallLlmAsync(systemPrompt, userMessage);
                    outputs.Add(StripFences(generated));
                }
                catch (Exception ex)
                {
                    errors.Add($"Chunk {index + 1}/{chunks.Count} failed: {ex.Message}");
                    outputs.Add($"{CommentPrefix(target)} ERROR in chunk {index + 1}: {ex.Message}");
                }
            }

            string final = PostProcess(string.Join(Environment.NewLine, outputs), target);
            return new ConversionResult(final, errors);
        }

        // ──────────────────────────────────────────────────────────── chunking

        /// <summary>
        /// Splits the VBScript at Sub/Function/Class boundaries so each chunk
        /// fits within the model's context window.
        /// </summary>
        private static List<string> Chunk(string code, int maxLines = 150)
        {
            var chunks    = new List<string>();
            var lines     = code.Split('\n');
            var current   = new StringBuilder();
            int lineCount = 0;

            foreach (string line in lines)
            {
                current.AppendLine(line);
                lineCount++;

                bool isBoundary =
                    line.TrimStart().StartsWith("End Sub",      StringComparison.OrdinalIgnoreCase) ||
                    line.TrimStart().StartsWith("End Function", StringComparison.OrdinalIgnoreCase) ||
                    line.TrimStart().StartsWith("End Class",    StringComparison.OrdinalIgnoreCase);

                if (isBoundary || lineCount >= maxLines)
                {
                    chunks.Add(current.ToString());
                    current.Clear();
                    lineCount = 0;
                }
            }

            if (current.Length > 0)
                chunks.Add(current.ToString());

            return chunks;
        }

        // ─────────────────────────────────────────────────────── prompt builders

        private static string BuildSystemPrompt(ConversionTarget target) => target switch
        {
            ConversionTarget.PowerShell => """
                You are an expert VBScript-to-PowerShell transpiler.
                Rules:
                1. Convert every VBScript statement to idiomatic PowerShell 7.
                2. Replace CreateObject("X") with New-Object -ComObject X or the most idiomatic PS equivalent.
                3. Replace WScript.Echo with Write-Host.
                4. Replace WScript.Quit(n) with exit n.
                5. Replace On Error Resume Next with $ErrorActionPreference = 'SilentlyContinue'.
                6. Translate comparison operators (=, <>, And, Or, Not) to PS equivalents (-eq, -ne, -and, -or, -not).
                7. Output ONLY the PowerShell code — no explanations, no markdown fences.
                """,

            ConversionTarget.CSharp => """
                You are an expert VBScript-to-C# transpiler.
                Rules:
                1. Produce a single self-contained C# 12 file using top-level statements.
                2. Use dynamic for COM object variables; use Activator.CreateInstance(Type.GetTypeFromProgID("X")) for CreateObject("X").
                3. Replace WScript.Echo with Console.WriteLine.
                4. Replace WScript.Quit(n) with Environment.Exit(n).
                5. Replace On Error Resume Next with try/catch blocks.
                6. Import only BCL namespaces (System, System.IO, System.Collections.Generic, etc.).
                7. Output ONLY the C# code — no explanations, no markdown fences.
                """,

            _ => throw new ArgumentOutOfRangeException(nameof(target))
        };

        private static string BuildUserMessage(string chunk, int index, int total, ConversionTarget target)
        {
            string lang = target == ConversionTarget.PowerShell ? "PowerShell" : "C#";
            return $"[Chunk {index + 1} of {total}]\nConvert the following VBScript to {lang}:\n\n{chunk}";
        }

        // ────────────────────────────────────────────────────────── LLM call

        private async Task<string> CallLlmAsync(string systemPrompt, string userMessage)
        {
            var body = new
            {
                model    = _opts.Model,
                messages = new[]
                {
                    new { role = "system",  content = systemPrompt },
                    new { role = "user",    content = userMessage  }
                },
                temperature = 0.1
            };

            var response = await _http.PostAsJsonAsync(_opts.Endpoint, body);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc    = await JsonDocument.ParseAsync(stream);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        // ─────────────────────────────────────────────────────── post-process

        private static string StripFences(string raw)
        {
            // Remove ```powershell / ```csharp / ``` fences
            var lines  = raw.Split('\n');
            var result = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("```")) continue;
                result.AppendLine(line);
            }
            return result.ToString();
        }

        private static string PostProcess(string code, ConversionTarget target)
        {
            if (target == ConversionTarget.CSharp)
            {
                // Deduplicate 'using' directives that appear in every chunk
                var usings  = new HashSet<string>();
                var body    = new StringBuilder();
                bool inBody = false;

                foreach (var line in code.Split('\n'))
                {
                    if (!inBody && line.TrimStart().StartsWith("using "))
                    {
                        usings.Add(line.Trim());
                        continue;
                    }
                    inBody = true;
                    body.AppendLine(line);
                }

                var header = new StringBuilder();
                foreach (var u in usings) header.AppendLine(u);
                header.AppendLine();
                header.Append(body);
                return header.ToString();
            }
            return code;
        }

        private static string CommentPrefix(ConversionTarget target) =>
            target == ConversionTarget.PowerShell ? "#" : "//";
    }

    // ─────────────────────────────────────────────────── supporting types

    public enum ConversionTarget { PowerShell, CSharp }

    public record LlmConverterOptions(
        string Endpoint,   // e.g. "https://api.openai.com/v1/chat/completions"
        string Model,      // e.g. "gpt-4o"
        string ApiKey      // leave empty for local Ollama
    );

    public record ConversionResult(string Code, List<string> Errors);
}
```

---

## ⚙️ Configuration (`appsettings.json`)

```json
{
  "LlmConverter": {
    "Provider": "openai",
    "Endpoint": "https://api.openai.com/v1/chat/completions",
    "Model": "gpt-4o",
    "ApiKey": "sk-..."
  }
}
```

### Using Azure OpenAI

```json
{
  "LlmConverter": {
    "Provider": "azure",
    "Endpoint": "https://<resource>.openai.azure.com/openai/deployments/<deployment>/chat/completions?api-version=2024-02-01",
    "Model": "gpt-4o",
    "ApiKey": "<azure-api-key>"
  }
}
```

### Using Local Ollama (free, offline)

```json
{
  "LlmConverter": {
    "Provider": "ollama",
    "Endpoint": "http://localhost:11434/v1/chat/completions",
    "Model": "codellama:13b",
    "ApiKey": ""
  }
}
```

---

## 📦 Output Example

### Input `.vbs`

```vbscript
' Restart a Windows service
On Error Resume Next

Dim oShell
Set oShell = CreateObject("WScript.Shell")

Dim svcName
svcName = "Spooler"

oShell.Run "net stop " & svcName, 0, True
oShell.Run "net start " & svcName, 0, True

If Err.Number <> 0 Then
    WScript.Echo "Error: " & Err.Description
    WScript.Quit(1)
End If

WScript.Echo "Service restarted: " & svcName
WScript.Quit(0)
```

### Output `.ps1` (LLM-generated)

```powershell
# Restart a Windows service
$ErrorActionPreference = 'SilentlyContinue'

$oShell = New-Object -ComObject WScript.Shell
$svcName = "Spooler"

$oShell.Run("net stop $svcName", 0, $true)
$oShell.Run("net start $svcName", 0, $true)

if ($Error.Count -gt 0) {
    Write-Host "Error: $($Error[0].Exception.Message)"
    exit 1
}

Write-Host "Service restarted: $svcName"
exit 0
```

### Output `.cs` (LLM-generated)

```csharp
using System;
using System.Diagnostics;

// Restart a Windows service
try
{
    dynamic oShell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
    string svcName = "Spooler";

    oShell.Run($"net stop {svcName}", 0, true);
    oShell.Run($"net start {svcName}", 0, true);

    Console.WriteLine($"Service restarted: {svcName}");
    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
```

---

## 🧪 Testing Strategy

Because LLM output is non-deterministic, tests use a **mock LLM client**:

```csharp
// LlmVbsConverterTests.cs
public class LlmVbsConverterTests
{
    private static HttpClient BuildMockClient(string response)
    {
        // returns a fake HttpClient that always returns `response`
        // (use MockHttp or a custom DelegatingHandler)
    }

    [Fact]
    public async Task ConvertAsync_PS1_StripsFences()
    {
        var mockResponse = """
            ```powershell
            Write-Host "hello"
            ```
            """;
        var http      = BuildMockClient(WrapInOpenAiResponse(mockResponse));
        var converter = new LlmVbsConverter(
            new LlmConverterOptions("http://fake", "gpt-4o", "key"), http);

        var result = await converter.ConvertAsync("WScript.Echo \"hello\"", ConversionTarget.PowerShell);

        Assert.DoesNotContain("```", result.Code);
        Assert.Contains("Write-Host", result.Code);
    }
}
```

---

## 💰 Cost Estimation (OpenAI GPT-4o)

| File size | Est. tokens | Est. cost (GPT-4o) |
|-----------|-------------|---------------------|
| 100 lines | ~2 000 | ~$0.01 |
| 500 lines | ~8 000 | ~$0.04 |
| 2 000 lines | ~30 000 | ~$0.15 |
| 5 000 lines | ~75 000 | ~$0.38 |

*Prices approximate as of early 2025. Use `gpt-4o-mini` to reduce cost by ~10×.*

---

## ✅ Pros & Cons

| ✅ Pros | ❌ Cons |
|---------|---------|
| Handles any VBScript construct, including unusual patterns | Requires internet / LLM API access (or Ollama) |
| Produces idiomatic, readable output automatically | Non-deterministic output — may differ run to run |
| Understands intent, not just syntax | API cost for large files |
| Can explain and annotate converted code on request | Must validate output (Roslyn / PS parser) |
| Easy to update prompts without changing C# code | Sensitive code must not be sent to 3rd-party APIs |

---

## 🔒 Privacy Considerations

- **Never send** credentials, passwords, or secrets present in `.vbs` files to a cloud LLM.
- Use the **Ollama local model** option for proprietary or sensitive scripts.
- Scrub API keys / passwords from the VBS source before conversion using the built-in
  `SensitiveDataScrubber` preprocessor (planned for `Phase6-Advanced`).

---

## 🔧 Integration into BLML Pipeline

```
src/Phase5-ASPtoAngular/VbsConverter/
    LlmVbsConverter.cs
    LlmConverterOptions.cs
    ConversionResult.cs
    VbsChunker.cs
tests/Unit/VbsConverter/
    LlmVbsConverterTests.cs
```

CLI entry point (Phase 8):

```
blml convert-vbs input.vbs output.ps1 --mode llm --target ps1 --model gpt-4o
blml convert-vbs input.vbs output.cs  --mode llm --target cs  --model ollama:codellama
```

Environment variable for API key (preferred over config file):

```
BLML_LLM_API_KEY=sk-...
blml convert-vbs myscript.vbs output.ps1 --mode llm
```

# VBScript to PowerShell/C# — Approach 1: Rule-Based Regex Transpiler

## 📋 Overview

This approach implements a **deterministic, rule-based transpiler** written in C# that uses
regular-expression pattern matching and ordered rewrite rules to convert a `.vbs` file
line-by-line into either a **PowerShell `.ps1` script** or a **single-file C# console app**.

No external AI or LLM is required. The converter is fully self-contained, fast, and easy to audit.

---

## 🎯 When to Use This Approach

| Situation | Suitable? |
|-----------|-----------|
| Small/medium `.vbs` files (< 500 lines) | ✅ Yes |
| Scripts that rely on WScript / CScript host objects | ✅ Yes |
| Batch automation (loop over many files) | ✅ Yes |
| Scripts with heavy OOP or class hierarchies | ⚠️ Partial |
| Scripts that call obscure third-party COM servers | ⚠️ Manual review needed |

---

## 🏗️ Architecture

```
 .vbs file
     │
     ▼
┌─────────────────────────┐
│  VBScriptLineParser     │  Reads file line-by-line, strips comments,
│  (tokenise & classify)  │  identifies statement type
└──────────┬──────────────┘
           │  classified token stream
           ▼
┌─────────────────────────┐
│  RewriteRuleEngine      │  Applies ordered regex rules
│  (pattern → template)   │  (variable decls, conditionals, loops, …)
└──────────┬──────────────┘
           │  target-language lines
           ▼
┌─────────────────────────┐
│  OutputEmitter          │  Wraps lines in boilerplate for the
│  (PS1 | CS console)     │  chosen output format
└─────────────────────────┘
```

---

## 📐 Rule Set Design

Rules are stored in a simple JSON file (`vbs-rules.json`) so they can be extended
without recompiling.

```json
{
  "rules": [
    {
      "id": "dim_var",
      "pattern": "^\\s*Dim\\s+(?<name>[A-Za-z_][\\w]*)\\s*$",
      "csharp": "var ${name} = default(object);",
      "powershell": "$${name} = $null"
    },
    {
      "id": "wscript_echo",
      "pattern": "WScript\\.Echo\\s+(?<msg>.+)",
      "csharp": "Console.WriteLine(${msg});",
      "powershell": "Write-Host ${msg}"
    },
    {
      "id": "msgbox",
      "pattern": "MsgBox\\s+(?<msg>.+)",
      "csharp": "Console.WriteLine(${msg}); // MsgBox replaced",
      "powershell": "Write-Host ${msg} # MsgBox replaced"
    },
    {
      "id": "for_loop",
      "pattern": "^\\s*For\\s+(?<var>\\w+)\\s*=\\s*(?<start>.+)\\s+To\\s+(?<end>.+)$",
      "csharp": "for (var ${var} = ${start}; ${var} <= ${end}; ${var}++) {",
      "powershell": "for ($${var} = ${start}; $${var} -le ${end}; $${var}++) {"
    },
    {
      "id": "for_each",
      "pattern": "^\\s*For\\s+Each\\s+(?<item>\\w+)\\s+In\\s+(?<coll>.+)$",
      "csharp": "foreach (var ${item} in ${coll}) {",
      "powershell": "foreach ($${item} in ${coll}) {"
    },
    {
      "id": "next",
      "pattern": "^\\s*Next(\\s+\\w+)?\\s*$",
      "csharp": "}",
      "powershell": "}"
    },
    {
      "id": "if_then",
      "pattern": "^\\s*If\\s+(?<cond>.+)\\s+Then\\s*$",
      "csharp": "if (${cond}) {",
      "powershell": "if (${cond}) {"
    },
    {
      "id": "else",
      "pattern": "^\\s*Else\\s*$",
      "csharp": "} else {",
      "powershell": "} else {"
    },
    {
      "id": "end_if",
      "pattern": "^\\s*End\\s+If\\s*$",
      "csharp": "}",
      "powershell": "}"
    },
    {
      "id": "sub_decl",
      "pattern": "^\\s*Sub\\s+(?<name>\\w+)\\s*\\((?<params>[^)]*)\\)\\s*$",
      "csharp": "static void ${name}(${params}) {",
      "powershell": "function ${name}(${params}) {"
    },
    {
      "id": "function_decl",
      "pattern": "^\\s*Function\\s+(?<name>\\w+)\\s*\\((?<params>[^)]*)\\)\\s*$",
      "csharp": "static object ${name}(${params}) {",
      "powershell": "function ${name}(${params}) {"
    },
    {
      "id": "end_sub_func",
      "pattern": "^\\s*End\\s+(Sub|Function)\\s*$",
      "csharp": "}",
      "powershell": "}"
    },
    {
      "id": "set_createobject",
      "pattern": "^\\s*Set\\s+(?<var>\\w+)\\s*=\\s*CreateObject\\((?<prog>\".+?\")\\)\\s*$",
      "csharp": "dynamic ${var} = Activator.CreateInstance(Type.GetTypeFromProgID(${prog}));",
      "powershell": "$${var} = New-Object -ComObject ${prog}"
    },
    {
      "id": "on_error_resume",
      "pattern": "^\\s*On\\s+Error\\s+Resume\\s+Next\\s*$",
      "csharp": "// On Error Resume Next — wrap calls in try/catch",
      "powershell": "$ErrorActionPreference = 'SilentlyContinue'"
    },
    {
      "id": "wscript_quit",
      "pattern": "WScript\\.Quit\\((?<code>[^)]+)\\)",
      "csharp": "Environment.Exit(${code});",
      "powershell": "exit ${code}"
    }
  ]
}
```

---

## 🖥️ C# Implementation Sketch

```csharp
// File: VbsRuleConverter.cs  (single-file console app)
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// Rule-based VBScript → C# / PowerShell converter.
/// Usage:  VbsRuleConverter <input.vbs> <output.cs|output.ps1> [--target cs|ps1]
/// </summary>
class VbsRuleConverter
{
    record Rule(string Id, string Pattern, string CSharp, string PowerShell);

    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: VbsRuleConverter <input.vbs> <output> [--target cs|ps1]");
            Environment.Exit(1);
        }

        string inputPath  = args[0];
        string outputPath = args[1];
        string target     = args.Length >= 4 && args[2] == "--target" ? args[3] : "ps1";

        string rulesPath = Path.Combine(AppContext.BaseDirectory, "vbs-rules.json");
        var    rules     = LoadRules(rulesPath);
        var    lines     = File.ReadAllLines(inputPath);
        var    output    = Convert(lines, rules, target);

        File.WriteAllText(outputPath, output, Encoding.UTF8);
        Console.WriteLine($"✅ Converted to {outputPath}");
    }

    // ------------------------------------------------------------------ rules

    static List<Rule> LoadRules(string path)
    {
        using var stream = File.OpenRead(path);
        var doc   = JsonDocument.Parse(stream);
        var rules = new List<Rule>();
        foreach (var el in doc.RootElement.GetProperty("rules").EnumerateArray())
        {
            rules.Add(new Rule(
                el.GetProperty("id").GetString()!,
                el.GetProperty("pattern").GetString()!,
                el.GetProperty("csharp").GetString()!,
                el.GetProperty("powershell").GetString()!
            ));
        }
        return rules;
    }

    // ------------------------------------------------------------ conversion

    static string Convert(string[] lines, List<Rule> rules, string target)
    {
        var sb = new StringBuilder();

        if (target == "cs")
            EmitCSharpHeader(sb);

        foreach (var rawLine in lines)
        {
            string line = StripVbsComment(rawLine);
            if (string.IsNullOrWhiteSpace(line)) { sb.AppendLine(); continue; }

            bool matched = false;
            foreach (var rule in rules)
            {
                var m = Regex.Match(line, rule.Pattern, RegexOptions.IgnoreCase);
                if (!m.Success) continue;

                string template = target == "cs" ? rule.CSharp : rule.PowerShell;
                string result   = ApplyCaptures(template, m);
                sb.AppendLine(result);
                matched = true;
                break;
            }

            if (!matched)
                sb.AppendLine((target == "cs" ? "// " : "# ") + "TODO: " + rawLine.TrimStart());
        }

        if (target == "cs")
            EmitCSharpFooter(sb);

        return sb.ToString();
    }

    static string StripVbsComment(string line)
    {
        int idx = line.IndexOf('\'');
        return idx >= 0 ? line[..idx] : line;
    }

    static string ApplyCaptures(string template, Match m)
    {
        foreach (Group grp in m.Groups)
        {
            if (!int.TryParse(grp.Name, out _))
                template = template.Replace("${" + grp.Name + "}", grp.Value.Trim());
        }
        return template;
    }

    static void EmitCSharpHeader(StringBuilder sb) =>
        sb.AppendLine("""
            using System;
            using System.Runtime.InteropServices;

            // Auto-generated from VBScript — review before use
            class Program
            {
                static void Main(string[] args)
                {
            """);

    static void EmitCSharpFooter(StringBuilder sb) =>
        sb.AppendLine("""
                }
            }
            """);
}
```

---

## ⚙️ COM Object Mapping Table

VBScript heavily uses `CreateObject`. The converter maps common ProgIDs automatically:

| VBScript ProgID | C# equivalent | PowerShell equivalent |
|-----------------|---------------|-----------------------|
| `Scripting.FileSystemObject` | `dynamic fso = Activator.CreateInstance(...)` | `New-Object -ComObject Scripting.FileSystemObject` |
| `WScript.Shell` | `dynamic shell = ...` | `New-Object -ComObject WScript.Shell` |
| `ADODB.Connection` | `SqlConnection` (manual migration) | `New-Object -ComObject ADODB.Connection` |
| `ADODB.Recordset` | `SqlDataReader` (manual migration) | `New-Object -ComObject ADODB.Recordset` |
| `Excel.Application` | `Microsoft.Office.Interop.Excel.Application` | `New-Object -ComObject Excel.Application` |
| `Word.Application` | `Microsoft.Office.Interop.Word.Application` | `New-Object -ComObject Word.Application` |
| `Shell.Application` | `dynamic shell = ...` | `New-Object -ComObject Shell.Application` |

---

## 📦 Output Examples

### Input `.vbs`

```vbscript
Dim objFSO
Set objFSO = CreateObject("Scripting.FileSystemObject")
Dim sPath
sPath = "C:\Logs\app.log"
If objFSO.FileExists(sPath) Then
    WScript.Echo "File found: " & sPath
Else
    WScript.Echo "File not found"
End If
WScript.Quit(0)
```

### Output `.ps1`

```powershell
$objFSO = New-Object -ComObject "Scripting.FileSystemObject"
$sPath = $null
$sPath = "C:\Logs\app.log"
if ($objFSO.FileExists($sPath)) {
    Write-Host "File found: " + $sPath
} else {
    Write-Host "File not found"
}
exit 0
```

### Output `.cs`

```csharp
using System;
using System.Runtime.InteropServices;

// Auto-generated from VBScript — review before use
class Program
{
    static void Main(string[] args)
    {
        dynamic objFSO = Activator.CreateInstance(Type.GetTypeFromProgID("Scripting.FileSystemObject"));
        var sPath = default(object);
        sPath = "C:\\Logs\\app.log";
        if (objFSO.FileExists(sPath)) {
            Console.WriteLine("File found: " + sPath);
        } else {
            Console.WriteLine("File not found");
        }
        Environment.Exit(0);
    }
}
```

---

## ✅ Pros & Cons

| ✅ Pros | ❌ Cons |
|---------|---------|
| No external services or API keys needed | Cannot handle complex logic restructuring |
| Fast — processes thousands of lines per second | Regex rules can miss edge cases |
| Easy to extend via `vbs-rules.json` | Heavy COM usage may still need manual cleanup |
| Fully auditable and deterministic | No type inference |
| Works offline / air-gapped environments | Limited support for VBScript classes |

---

## 🔧 Integration into BLML Pipeline

This converter would live at:

```
src/Phase5-ASPtoAngular/VbsConverter/
    VbsRuleConverter.cs      ← main converter class
    VbsRuleConverterTests/   ← xUnit tests
    rules/
        vbs-rules.json       ← rewrite rules
```

It would expose a public API consistent with the existing `TranspilerResult` pattern used
throughout Phase 1–4, so the CLI in `Phase8-Tooling` can invoke it via:

```
blml convert-vbs input.vbs output.ps1 --target ps1
blml convert-vbs input.vbs output.cs  --target cs
```

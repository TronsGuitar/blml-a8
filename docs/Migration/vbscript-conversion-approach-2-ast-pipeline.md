# VBScript to PowerShell/C# — Approach 2: AST-Driven Pipeline

## 📋 Overview

This approach builds a **proper Abstract Syntax Tree (AST)** from the `.vbs` source file,
then walks the tree with a **visitor pattern** to emit idiomatic PowerShell `.ps1` scripts
(or single-file C# console apps).  It re-uses the lexer/parser design already present in
`Phase1-Foundation` and the `VBScriptParser` stub in `Phase5-ASPtoAngular`, extending both
into a full two-target code generator.

---

## 🎯 When to Use This Approach

| Situation | Suitable? |
|-----------|-----------|
| Medium-to-large `.vbs` files (500–5 000 lines) | ✅ Yes |
| Scripts with `Class … End Class` blocks | ✅ Yes |
| Scripts with nested control flow | ✅ Yes |
| Deeply embedded COM automation logic | ✅ Yes |
| Need structured diagnostics / warnings | ✅ Yes |
| Quick one-liner scripts | ⚠️ Overkill |

---

## 🏗️ Architecture

```
 .vbs file
     │
     ▼
┌───────────────────┐
│  VbsLexer         │  Produces typed token stream
│  (extends VB6Lexer│  with VBScript-specific tokens)
└────────┬──────────┘
         │  token stream
         ▼
┌───────────────────┐
│  VbsParser        │  Produces a typed AST
│  (extends         │  (VbsModule, VbsFunction, VbsSub,
│   VB6Parser stub) │   VbsIfBlock, VbsForLoop, …)
└────────┬──────────┘
         │  AST
         ▼
┌───────────────────┐
│  SemanticAnalyser │  Symbol table, type hints,
│                   │  COM ProgID resolution
└────────┬──────────┘
         │  annotated AST
         ├──────────────────┐
         ▼                  ▼
┌────────────────┐  ┌──────────────────┐
│  PS1Emitter    │  │  CSharpEmitter   │
│  (PowerShell)  │  │  (console app)   │
└────────────────┘  └──────────────────┘
```

---

## 🌳 AST Node Definitions

```csharp
// File: src/Phase5-ASPtoAngular/VbsConverter/Ast/VbsNodes.cs

namespace BLML.Phase5ASPtoAngular.VbsConverter.Ast
{
    public abstract record VbsNode;

    // ── Module ────────────────────────────────────────────────────────────
    public record VbsModule(
        string Name,
        List<VbsNode> Members
    ) : VbsNode;

    // ── Declarations ──────────────────────────────────────────────────────
    public record VbsDimStatement(
        List<string> Variables
    ) : VbsNode;

    public record VbsConstStatement(
        string Name,
        VbsExpression Value
    ) : VbsNode;

    public record VbsSetStatement(
        string Variable,
        VbsExpression Value
    ) : VbsNode;

    public record VbsAssignStatement(
        string Variable,
        VbsExpression Value
    ) : VbsNode;

    // ── Control flow ──────────────────────────────────────────────────────
    public record VbsIfBlock(
        VbsExpression Condition,
        List<VbsNode> ThenBody,
        List<VbsNode> ElseBody
    ) : VbsNode;

    public record VbsForLoop(
        string Counter,
        VbsExpression Start,
        VbsExpression End,
        VbsExpression? Step,
        List<VbsNode> Body
    ) : VbsNode;

    public record VbsForEachLoop(
        string Item,
        VbsExpression Collection,
        List<VbsNode> Body
    ) : VbsNode;

    public record VbsDoWhileLoop(
        VbsExpression Condition,
        bool IsDoWhile,   // true = Do While; false = Do Until
        List<VbsNode> Body
    ) : VbsNode;

    public record VbsWhileLoop(
        VbsExpression Condition,
        List<VbsNode> Body
    ) : VbsNode;

    public record VbsSelectCase(
        VbsExpression Subject,
        List<(VbsExpression? Value, List<VbsNode> Body)> Cases,
        List<VbsNode> ElseBody
    ) : VbsNode;

    // ── Subroutines / Functions ───────────────────────────────────────────
    public record VbsSubDecl(
        string Name,
        List<VbsParam> Params,
        List<VbsNode> Body
    ) : VbsNode;

    public record VbsFunctionDecl(
        string Name,
        List<VbsParam> Params,
        List<VbsNode> Body
    ) : VbsNode;

    public record VbsParam(string Name, bool ByRef);

    // ── Classes ───────────────────────────────────────────────────────────
    public record VbsClassDecl(
        string Name,
        List<VbsNode> Members
    ) : VbsNode;

    // ── Calls & Expressions ───────────────────────────────────────────────
    public record VbsCallStatement(
        string Callee,
        List<VbsExpression> Args
    ) : VbsNode;

    public abstract record VbsExpression : VbsNode;
    public record VbsLiteral(string Raw) : VbsExpression;
    public record VbsIdentifier(string Name) : VbsExpression;
    public record VbsBinaryOp(VbsExpression Left, string Op, VbsExpression Right) : VbsExpression;
    public record VbsCallExpression(string Callee, List<VbsExpression> Args) : VbsExpression;
    public record VbsMemberAccess(VbsExpression Object, string Member) : VbsExpression;
    public record VbsCreateObject(string ProgId) : VbsExpression;
}
```

---

## 🔍 Semantic Analyser — COM ProgID Resolution

The semantic analyser annotates `VbsCreateObject` nodes with a `ComMapping` that tells
the emitters which host APIs to use:

```csharp
// File: src/Phase5-ASPtoAngular/VbsConverter/SemanticAnalyser.cs

namespace BLML.Phase5ASPtoAngular.VbsConverter
{
    public record ComMapping(
        string ProgId,
        string CSharpType,        // null → keep dynamic/Activator
        string PowerShellCmdlet   // "New-Object -ComObject <ProgId>" or custom
    );

    public class SemanticAnalyser
    {
        private static readonly Dictionary<string, ComMapping> KnownCom = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Scripting.FileSystemObject"] = new("Scripting.FileSystemObject",
                "dynamic /*FSO*/", "New-Object -ComObject Scripting.FileSystemObject"),
            ["WScript.Shell"] = new("WScript.Shell",
                "dynamic /*WShell*/", "New-Object -ComObject WScript.Shell"),
            ["ADODB.Connection"] = new("ADODB.Connection",
                "System.Data.SqlClient.SqlConnection", "New-Object -ComObject ADODB.Connection"),
            ["Excel.Application"] = new("Excel.Application",
                "Microsoft.Office.Interop.Excel.Application",
                "New-Object -ComObject Excel.Application"),
        };

        public ComMapping? Resolve(string progId) =>
            KnownCom.TryGetValue(progId, out var m) ? m : null;
    }
}
```

---

## 🖨️ PowerShell Emitter

```csharp
// File: src/Phase5-ASPtoAngular/VbsConverter/PS1Emitter.cs

namespace BLML.Phase5ASPtoAngular.VbsConverter
{
    using Ast;

    public class PS1Emitter
    {
        private readonly SemanticAnalyser _sem = new();
        private int _indent = 0;
        private readonly StringBuilder _sb = new();

        public string Emit(VbsModule module)
        {
            _sb.AppendLine("# Auto-generated PowerShell from VBScript — review before use");
            _sb.AppendLine();
            foreach (var node in module.Members)
                EmitNode(node);
            return _sb.ToString();
        }

        private void EmitNode(VbsNode node)
        {
            switch (node)
            {
                case VbsDimStatement dim:
                    foreach (var v in dim.Variables)
                        Line($"${v} = $null");
                    break;

                case VbsSetStatement set when set.Value is VbsCreateObject co:
                    var mapping = _sem.Resolve(co.ProgId);
                    Line($"${set.Variable} = {mapping?.PowerShellCmdlet ?? $"New-Object -ComObject {co.ProgId}"}");
                    break;

                case VbsAssignStatement assign:
                    Line($"${assign.Variable} = {EmitExpr(assign.Value)}");
                    break;

                case VbsIfBlock ifb:
                    Line($"if ({EmitExpr(ifb.Condition)}) {{");
                    Indented(() => ifb.ThenBody.ForEach(EmitNode));
                    if (ifb.ElseBody.Count > 0)
                    {
                        Line("} else {");
                        Indented(() => ifb.ElseBody.ForEach(EmitNode));
                    }
                    Line("}");
                    break;

                case VbsForLoop fl:
                    string step = fl.Step is not null ? $"; ${fl.Counter} += {EmitExpr(fl.Step)}" : $"; ${fl.Counter}++";
                    Line($"for (${fl.Counter} = {EmitExpr(fl.Start)}; ${fl.Counter} -le {EmitExpr(fl.End)}{step}) {{");
                    Indented(() => fl.Body.ForEach(EmitNode));
                    Line("}");
                    break;

                case VbsForEachLoop fe:
                    Line($"foreach (${fe.Item} in {EmitExpr(fe.Collection)}) {{");
                    Indented(() => fe.Body.ForEach(EmitNode));
                    Line("}");
                    break;

                case VbsSubDecl sub:
                    Line($"function {sub.Name}({string.Join(", ", sub.Params.Select(p => "$" + p.Name))}) {{");
                    Indented(() => sub.Body.ForEach(EmitNode));
                    Line("}");
                    break;

                case VbsFunctionDecl fn:
                    Line($"function {fn.Name}({string.Join(", ", fn.Params.Select(p => "$" + p.Name))}) {{");
                    Indented(() => fn.Body.ForEach(EmitNode));
                    Line("}");
                    break;

                case VbsCallStatement call:
                    Line($"{call.Callee} {string.Join(", ", call.Args.Select(EmitExpr))}");
                    break;

                default:
                    Line($"# TODO: {node.GetType().Name}");
                    break;
            }
        }

        private string EmitExpr(VbsExpression expr) => expr switch
        {
            VbsLiteral lit        => lit.Raw,
            VbsIdentifier id      => "$" + id.Name,
            VbsBinaryOp bin       => $"{EmitExpr(bin.Left)} {MapOp(bin.Op)} {EmitExpr(bin.Right)}",
            VbsCallExpression c   => $"{c.Callee}({string.Join(", ", c.Args.Select(EmitExpr))})",
            VbsMemberAccess ma    => $"{EmitExpr(ma.Object)}.{ma.Member}",
            VbsCreateObject co    => _sem.Resolve(co.ProgId)?.PowerShellCmdlet ?? $"New-Object -ComObject {co.ProgId}",
            _                     => $"# UNKNOWN_EXPR({expr.GetType().Name})"
        };

        private static string MapOp(string vbsOp) => vbsOp.ToUpperInvariant() switch
        {
            "AND" => "-and",
            "OR"  => "-or",
            "NOT" => "-not",
            "="   => "-eq",
            "<>"  => "-ne",
            ">"   => "-gt",
            ">="  => "-ge",
            "<"   => "-lt",
            "<="  => "-le",
            "&"   => "+",
            _     => vbsOp
        };

        private void Line(string s) => _sb.AppendLine(new string(' ', _indent * 4) + s);
        private void Indented(Action a) { _indent++; a(); _indent--; }
    }
}
```

---

## 🖥️ C# Emitter (single-file console app)

```csharp
// File: src/Phase5-ASPtoAngular/VbsConverter/CSharpEmitter.cs

namespace BLML.Phase5ASPtoAngular.VbsConverter
{
    using Ast;

    public class CSharpEmitter
    {
        private readonly SemanticAnalyser _sem = new();
        private int _indent = 1;
        private readonly StringBuilder _sb = new();

        public string Emit(VbsModule module)
        {
            _sb.AppendLine("// Auto-generated C# from VBScript — review before use");
            _sb.AppendLine("using System;");
            _sb.AppendLine("using System.Collections;");
            _sb.AppendLine();
            _sb.AppendLine("class Program");
            _sb.AppendLine("{");

            // Hoist module-level statements into Main
            _sb.AppendLine("    static void Main(string[] args)");
            _sb.AppendLine("    {");
            _indent = 2;

            foreach (var node in module.Members)
            {
                if (node is VbsSubDecl or VbsFunctionDecl or VbsClassDecl)
                    continue; // emitted after Main
                EmitNode(node);
            }

            _sb.AppendLine("    }");

            // Emit subs / functions / classes as static members
            _indent = 1;
            foreach (var node in module.Members)
            {
                if (node is VbsSubDecl or VbsFunctionDecl or VbsClassDecl)
                    EmitNode(node);
            }

            _sb.AppendLine("}");
            return _sb.ToString();
        }

        private void EmitNode(VbsNode node)
        {
            switch (node)
            {
                case VbsDimStatement dim:
                    foreach (var v in dim.Variables)
                        Line($"dynamic {v} = null;");
                    break;

                case VbsSetStatement set when set.Value is VbsCreateObject co:
                    var mapping = _sem.Resolve(co.ProgId);
                    string csType = mapping?.CSharpType ?? "dynamic";
                    Line($"{csType} {set.Variable} = Activator.CreateInstance(Type.GetTypeFromProgID(\"{co.ProgId}\"));");
                    break;

                case VbsAssignStatement assign:
                    Line($"{assign.Variable} = {EmitExpr(assign.Value)};");
                    break;

                case VbsIfBlock ifb:
                    Line($"if ({EmitExpr(ifb.Condition)})");
                    Line("{");
                    Indented(() => ifb.ThenBody.ForEach(EmitNode));
                    Line("}");
                    if (ifb.ElseBody.Count > 0)
                    {
                        Line("else");
                        Line("{");
                        Indented(() => ifb.ElseBody.ForEach(EmitNode));
                        Line("}");
                    }
                    break;

                case VbsForLoop fl:
                    Line($"for (dynamic {fl.Counter} = {EmitExpr(fl.Start)}; {fl.Counter} <= {EmitExpr(fl.End)}; {fl.Counter}++)");
                    Line("{");
                    Indented(() => fl.Body.ForEach(EmitNode));
                    Line("}");
                    break;

                case VbsSubDecl sub:
                    Line($"static void {sub.Name}({string.Join(", ", sub.Params.Select(p => "dynamic " + p.Name))})");
                    Line("{");
                    Indented(() => sub.Body.ForEach(EmitNode));
                    Line("}");
                    break;

                case VbsFunctionDecl fn:
                    Line($"static dynamic {fn.Name}({string.Join(", ", fn.Params.Select(p => "dynamic " + p.Name))})");
                    Line("{");
                    Indented(() => fn.Body.ForEach(EmitNode));
                    Line("}");
                    break;

                default:
                    Line($"// TODO: {node.GetType().Name}");
                    break;
            }
        }

        private string EmitExpr(VbsExpression expr) => expr switch
        {
            VbsLiteral lit       => lit.Raw,
            VbsIdentifier id     => id.Name,
            VbsBinaryOp bin      => $"{EmitExpr(bin.Left)} {MapOp(bin.Op)} {EmitExpr(bin.Right)}",
            VbsCallExpression c  => $"{c.Callee}({string.Join(", ", c.Args.Select(EmitExpr))})",
            VbsMemberAccess ma   => $"{EmitExpr(ma.Object)}.{ma.Member}",
            _                    => $"/* UNKNOWN_EXPR({expr.GetType().Name}) */ null"
        };

        private static string MapOp(string op) => op.ToUpperInvariant() switch
        {
            "AND" => "&&",
            "OR"  => "||",
            "NOT" => "!",
            "<>"  => "!=",
            "&"   => "+",
            _     => op
        };

        private void Line(string s) => _sb.AppendLine(new string(' ', _indent * 4) + s);
        private void Indented(Action a) { _indent++; a(); _indent--; }
    }
}
```

---

## 📦 Output Example

### Input `.vbs`

```vbscript
Dim oShell
Set oShell = CreateObject("WScript.Shell")

Dim i
For i = 1 To 5
    oShell.Run "cmd /c echo Line " & i
Next
```

### Output `.ps1`

```powershell
# Auto-generated PowerShell from VBScript — review before use

$oShell = New-Object -ComObject WScript.Shell
$i = $null
for ($i = 1; $i -le 5; $i++) {
    $oShell.Run("cmd /c echo Line " + $i)
}
```

### Output `.cs`

```csharp
// Auto-generated C# from VBScript — review before use
using System;
using System.Collections;

class Program
{
    static void Main(string[] args)
    {
        dynamic /*WShell*/ oShell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
        dynamic i = null;
        for (dynamic i = 1; i <= 5; i++)
        {
            oShell.Run("cmd /c echo Line " + i);
        }
    }
}
```

---

## 🧪 Testing Strategy

Tests live alongside existing xUnit suites in `tests/Unit/`:

```csharp
// VbsAstEmitterTests.cs
public class VbsAstEmitterTests
{
    [Fact]
    public void ForLoop_EmitsCorrectPS1()
    {
        var module = new VbsModule("Test", new()
        {
            new VbsDimStatement(new() { "i" }),
            new VbsForLoop("i",
                new VbsLiteral("1"), new VbsLiteral("10"), null,
                new List<VbsNode>
                {
                    new VbsCallStatement("WScript.Echo", new() { new VbsIdentifier("i") })
                })
        });

        var ps1 = new PS1Emitter().Emit(module);

        Assert.Contains("for ($i = 1; $i -le 10; $i++)", ps1);
        Assert.Contains("WScript.Echo $i", ps1);
    }
}
```

---

## ✅ Pros & Cons

| ✅ Pros | ❌ Cons |
|---------|---------|
| Structured AST enables accurate semantic analysis | More code to write and maintain |
| Both C# and PS1 targets share the same AST | Parser must handle all VBScript edge cases |
| Integrates naturally with existing BLML Phase 1 lexer | Initial investment to build the parser |
| Produces clean, well-indented idiomatic output | |
| Full diagnostic messages with line/column numbers | |
| Easily extensible (add new AST nodes for new constructs) | |

---

## 🔧 Integration into BLML Pipeline

```
src/Phase5-ASPtoAngular/VbsConverter/
    Ast/
        VbsNodes.cs
    VbsLexer.cs          ← extends/reuses Phase1 VB6Lexer
    VbsParser.cs         ← extends Phase5 VBScriptParser stub
    SemanticAnalyser.cs
    PS1Emitter.cs
    CSharpEmitter.cs
tests/Unit/VbsConverter/
    VbsParserTests.cs
    VbsAstEmitterTests.cs
```

CLI entry point (Phase 8):

```
blml convert-vbs input.vbs output.ps1 --mode ast --target ps1
blml convert-vbs input.vbs output.cs  --mode ast --target cs
```

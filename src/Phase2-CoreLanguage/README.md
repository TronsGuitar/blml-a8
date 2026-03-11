# Phase2 Core Language

## Current Phase2 surface area

The current `Phase2-CoreLanguage` folder now retains only code that is not already implemented elsewhere in the repository:

- `src/Phase2-CoreLanguage/Converters/ErrorHandlingConverter.cs`

## Why the rest of old Phase2 was removed

Most earlier `Phase2` content had already been superseded by active implementations in other phases.

### Implemented in `Phase1-Foundation`

The following core-language responsibilities are already handled by the active parser/code-generation pipeline in `Phase1`:

- variable declarations and scope parsing
- VB6 type mapping to C# types
- arithmetic, comparison, and concatenation operators
- `If`, `For`, `While`, `Do/Loop`, and `Select Case`
- `Function` and `Sub` conversion
- `ReDim` parsing
- many VB6 built-in string and math functions via `BuiltInFunctionHandler`
- Roslyn-based C# emission through `VB6CodeGenerator`

Primary files:

- `src/Phase1-Foundation/Parser/VB6Parser.cs`
- `src/Phase1-Foundation/AST/AstBuilder.cs`
- `src/Phase1-Foundation/Parser/VB6CodeGenerator.cs`
- `src/Phase1-Foundation/Parser/BuiltInFunctionHandler.cs`

### Implemented in `Phase3-FormsUI`

The old `vb62cs12cvrt.cs` file was really form/control conversion work and is now superseded by the active `Phase3` form pipeline:

- `src/Phase3-FormsUI/FormParsing/frmParser.cs`
- `src/Phase3-FormsUI/FormParsing/Vb6FormCodeGenerator.cs`

### Removed as obsolete or experimental

The following old `Phase2` files were removed because they were duplicates, prototypes, or no longer the active implementation path:

- `Converters/vb62cs12cvrt.cs`
- `Converters/ControlFlowConverter.cs`
- `Converters/VariableConverter.cs`
- `CodeGeneration/codedom.cs`
- `CodeGeneration/codedomType.cs`
- `CodeGeneration/mistralVB6Compiler.cs`
- `CodeGeneration/roslynReplacesCodeDom.cs`
- `Project/csprojclass.cs`
- `Project/csprojgenerator.cs`
- `Constants/VB6Constants.cs`
- `Constants/constants.cs`

## Remaining implementation target

### `src/Phase2-CoreLanguage/Converters/ErrorHandlingConverter.cs`

This remains the primary Phase2-specific gap because equivalent functionality is not yet implemented elsewhere.

It now provides a stronger implementation for:

- parsed procedure-state modeling for VB6 error directives, labels, resume statements, and executable statements
- `On Error GoTo <label>` conversion into `try/catch` plus handler-label output
- explicit label-target modeling across the converted procedure
- `On Error Resume Next` conversion into per-statement `try/catch` wrappers
- `On Error GoTo 0` reset handling
- `Err.Raise(...)` conversion into `throw new Vb6RuntimeException(...)`
- `Error <number>` conversion into `throw new Vb6RuntimeException(...)`
- `Err.Number`, `Err.Description`, `Err.Source`, and `Err.Clear` mapping through a generated `__vb6Err` state object
- explicit `goto <label>` output for resumptions that name a concrete target label
- manual-review markers for unsupported `Resume` and `Resume Next` flows that still cannot be safely inferred

## Suggested implementation plan

Completed in this pass:

1. moved error-handling parsing into dedicated procedure state rather than only direct line-based conversion
2. modeled label targets and explicit resume-to-label flow across the converted procedure
3. improved `Err` object mapping beyond generic exception messages
4. converted more VB6 procedure syntax around handlers, including `Call`, `Exit Sub/Function/Property`, `Exit Do/For`, `GoTo`, and `Err.Clear`
5. broadened tests for multi-label handlers and more complex resume patterns

Remaining follow-up:

1. move error-handling modeling into the shared Phase1 AST instead of keeping it local to `ErrorHandlingConverter`
2. model `Resume Next` and plain `Resume` with more exact control-flow reconstruction
3. improve `Err` mapping for richer VB6-compatible fields and behaviors
4. connect error-handling conversion to the main transpiler pipeline rather than using it only as a focused helper
5. add more fixture-based tests for larger real-world procedures with labels and mixed handler modes

## What is left to do now

- keep the current error-handling slice, but move it from a focused helper into the shared parser/AST/codegen pipeline
- finish the hard VB6 control-flow cases around `Resume`, `Resume Next`, and mixed label-based handlers
- extend the active Phase2 coverage beyond error handling only when a gap is not already owned by Phase1 or Phase3
- add larger real-world business-logic fixtures so the current implementation is validated against realistic procedures

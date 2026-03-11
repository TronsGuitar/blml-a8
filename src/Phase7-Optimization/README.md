# Phase7 Optimization

## Status

- **Current status:** multiple helper slices are active
- **Validated state:** XML documentation generation, dead-code cleanup analysis, and LINQ suggestion helpers are covered by executable tests
- **Known gap:** optimization remains file- or request-scoped rather than whole-project and rewrite-capable

## Current Phase7 surface area

The current `Phase7-Optimization` folder contains three tracked components:

- `src/Phase7-Optimization/Documentation/XmlDocGenerator.cs`
- `src/Phase7-Optimization/CodeCleanup/DeadCodeRemover.cs`
- `src/Phase7-Optimization/Refactoring/LinqOptimizer.cs`

## Implemented in this pass

The active Phase7 implementation slices are now in:

### `src/Phase7-Optimization/Documentation/XmlDocGenerator.cs`

It now supports:

- parsing VB6 `Sub`, `Function`, and `Property Get/Let/Set` headers
- parsing VB6 parameter metadata including `Optional`, `ByVal`, `ByRef`, `ParamArray`, and default values
- converting leading VB6 apostrophe comments into C# XML documentation comments
- inferring `<summary>`, `<param>`, and `<returns>` content when explicit templates are not supplied
- normalizing legacy task comments such as `TODO`, `FIXME`, `HACK`, and `UNDONE`
- applying external documentation templates through request-provided template mappings

### `src/Phase7-Optimization/CodeCleanup/DeadCodeRemover.cs`

It now supports:

- flagging unused private fields, properties, and methods in a C# source file
- flagging potentially dead public members for manual review when they appear only at declaration sites in the analyzed file
- detecting statements that appear after terminating statements such as `return`, `throw`, `break`, `continue`, and `goto`
- removing commented-out code lines that still resemble executable C#
- removing common conversion-era legacy marker comments from cleaned output

### `src/Phase7-Optimization/Refactoring/LinqOptimizer.cs`

It now supports:

- suggesting `.Count()` replacements for manual foreach counting loops
- suggesting `.Count(predicate)` replacements for simple conditional counting loops
- suggesting `.Sum(...)` replacements for manual accumulator loops
- suggesting `.Select().ToList()` and `.Where().Select().ToList()` replacements for common projection loops

### Validation added

Added executable tests for:

- XML doc generation from VB6 comments and function headers
- task-comment normalization
- template-based documentation overrides
- dead-code analysis and cleanup of commented-out conversion remnants
- loop-to-LINQ suggestion generation for count, sum, and projection patterns
- Phase7 README and status-document coverage

## Existing prerequisites already present in the repository

### `src/Phase7-Optimization/CodeCleanup/DeadCodeRemover.cs`

- uses syntax-tree analysis on a single C# input file rather than whole-solution semantic analysis
- currently reports unused members heuristically by identifier occurrence counts within the analyzed file

### `src/Phase7-Optimization/Refactoring/LinqOptimizer.cs`

- currently focuses on common foreach-loop patterns rather than full expression-tree or semantic-query rewriting
- currently emits suggestions rather than rewriting source automatically

## Not implemented yet

### `src/Phase7-Optimization`

- XML doc generation currently operates on supplied VB6 signatures and comments rather than a whole-project analysis pipeline
- dead-code analysis currently works per file and does not yet use semantic symbol resolution across projects
- LINQ optimization currently covers only count, sum, and projection suggestion patterns
- there is no migration report generator yet
- there is no manual review list generator yet
- there are no code-metrics helpers yet
- there is no formatter, naming-normalization pass, or async/nullable modernization pipeline yet

## TODO

1. extend `DeadCodeRemover.cs` from per-file heuristics into symbol-aware whole-project analysis
2. extend `LinqOptimizer.cs` beyond suggestion-only output into optional safe rewrites for more loop and sorting patterns
3. add migration-report and manual-review-list generators
4. add code-metrics output for complexity and maintainability reporting
5. connect `XmlDocGenerator` to parsed project/module output instead of using only direct signature requests
6. add broader Phase7 tests with representative converted-code fixtures

## What is left to do now

- turn the current helper-style utilities into project-aware analysis and reporting tools
- move dead-code detection from heuristic per-file scans to symbol-aware whole-project analysis
- expand LINQ optimization from suggestions into optional safe rewrites for more patterns
- add migration reports, manual review outputs, and code metrics so Phase7 helps guide remediation work at scale

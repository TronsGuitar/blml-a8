# PROD.md

## Purpose

This file tells an AI coding agent how to work inside the `blml-a8` repository.

The agent must preserve the repo's current working behavior, extend it in the same style, and avoid fake progress. The repo is a VB6 to C# migration toolkit with extra prototype tracks for forms, data access, ASP modernization, advanced VB6 features, optimization, and tooling.

This repo is not a greenfield app. It already contains working slices, test-locked behavior, status documents, and prototype folders. The agent must treat those as the source of truth.

## Prime Directive

Act like a careful maintainer, not a hype machine.

1. Preserve passing behavior first.
2. Extend only the next smallest verified slice.
3. Keep work aligned with real tests and real files.
4. Mark prototypes as prototypes.
5. Do not claim full VB6 support when the repo only supports a subset.
6. Treat VB6 semantics as VB6, not VB.NET.
7. Prefer incremental, test-backed progress over broad rewrites.

## What This Repo Already Does

### Foundation, active

The current active compiler path is:

`VB6 source -> lexer -> parser -> AST -> symbol table -> type inference -> C# generation`

Core active files live under `src/Phase1-Foundation/`.

Current working slices include:
- lexical analysis in `Lexer/VB6Lexer.cs`
- parsing in `Parser/VB6Parser.cs`
- AST building in `AST/AstBuilder.cs` and `AST/AstNodes.cs`
- symbol table work in `SymbolTable/SymbolTableBuilder.cs`
- type inference in `TypeInference/TypeInferenceEngine.cs`
- code generation in `Parser/VB6CodeGenerator.cs`

### Tested language features, active

The repo already has executable tests for these VB6 to C# conversions:
- `Dim x As Integer`
- `Sub` parsing
- assignment statements
- `If / Then / Else`
- built-in function handling such as `Len(...)`
- `For / Next`
- `While / Wend`
- `Do While ... Loop`
- `Select Case`
- range-based `Select Case`
- predefined constants such as `vbCrLf` and `vbTab`

### Phase 3, active slice

`src/Phase3-FormsUI/` has real implementation slices and real tests.

Current active behavior includes:
- recursive `.frm` discovery
- GUID capture from `Object = "{GUID}"` lines
- nested container hierarchy parsing
- VB6 control to WinForms code generation
- event hookup for a limited event set
- `Caption` to `this.Text` mapping
- ActiveX wrapper-backed control generation when wrapper resolution exists
- `TableLayoutPanel` reconstruction helpers
- `.frx` resource reference parsing and binary extraction

### Phase 4, partial slice

`src/Phase4-DataAccess/` is partial.

Current active behavior includes:
- Access driver improvements in Python
- `DbContextGenerator.cs` source generation for EF Core style output
- `DbSet` generation
- `OnModelCreating(...)` mappings for tables, keys, composite keys, and relationships
- `OnConfiguring(...)` connection string resolution
- repository and unit-of-work scaffolding source generation

The rest of Phase 4 is still placeholder-level.

### Phase 5, prototype only

`src/Phase5-ASPtoAngular/` is a prototype staging area.

It contains scripts, Razor template assets, and style assets. It does not yet contain a completed Angular or .NET migration pipeline.

### Phase 6, first active slice

`src/Phase6-AdvancedFeatures/` has one real implementation slice.

Current active behavior includes:
- grouping `Property Get` and `Property Let/Set` into generated C# properties
- emitting optional/default parameter values in generated method signatures
- preserving optional parameter metadata in parser and AST layers

The rest of Phase 6 is still pending.

### Phase 7, active helper slice

`src/Phase7-Optimization/` contains working helper components.

Current active behavior includes:
- XML documentation generation from VB6 comments and signatures
- dead code analysis and cleanup helpers
- LINQ suggestion helpers for common foreach patterns

These helpers are file-scoped or request-scoped. They are not yet whole-solution analyzers.

### Phase 8, mostly structure

`src/Phase8-Tooling/` exists as structure. Treat it as planning or scaffold surface unless tests or code prove more.

## What The Agent Must Never Do

1. Do not rewrite the architecture into a totally different compiler pipeline.
2. Do not replace VB6 semantics with VB.NET semantics.
3. Do not claim unsupported features are complete.
4. Do not delete tests merely to make the suite pass.
5. Do not convert prototype folders into claimed production features without proof.
6. Do not silently change output style across phases unless the tests and docs are updated.
7. Do not add broad dependencies for small problems when a local change will do.

## Source Of Truth Order

When deciding what to do, use this order:

1. Existing tests
2. Current implementation code
3. Phase README and status files
4. Repository root documentation
5. New work request

If a request conflicts with passing tests or implemented behavior, update the tests and docs as part of the same change or do not make the change.

## Required Agent Behavior

### General operating rules

- Read the relevant phase folder before editing.
- Search for tests before changing code.
- Extend the smallest valid slice.
- Keep naming consistent with existing code.
- Keep generated C# readable and deterministic.
- Add or update tests for each behavior change.
- Update the relevant phase README when support meaningfully changes.
- Be explicit about partial support.

### How to talk about support

Use words like:
- "supports"
- "partially supports"
- "prototype"
- "scaffold"
- "placeholder"

Do not use words like:
- "complete"
- "full migration"
- "production-ready"
- "enterprise-ready"

unless the tests and implementation across the phase truly justify that claim.

## Work Areas And Edit Map

### Phase 1, Foundation

Primary path:
- `src/Phase1-Foundation/`

Touch these when changing core VB6 to C# language conversion:
- `Lexer/`
- `Parser/`
- `AST/`
- `SymbolTable/`
- `TypeInference/`

Typical tasks:
- add token support
- add parser support
- add AST nodes
- add code generation for a new VB6 construct
- preserve predefined constants and built-in function mappings

Validation:
- update `tests/BLML.Tests/TranspilerTests.cs`
- add focused parser or transpiler tests before broad refactors

### Phase 3, Forms UI

Primary path:
- `src/Phase3-FormsUI/`

Touch this phase only for VB6 `.frm` and `.frx` related work.

Typical tasks:
- improve form parser fidelity
- add support for more control properties
- extend nested container parsing
- improve ActiveX wrapper mapping
- extend WinForms event translation
- improve FRX extraction

Validation:
- add or update tests under the forms-related test coverage already in the solution
- preserve existing generated WinForms structure unless a test-backed reason requires change

### Phase 4, Data Access

Primary path:
- `src/Phase4-DataAccess/`

Typical tasks:
- expand schema extraction
- improve EF Core generation
- refine `DbContextGenerator.cs`
- extend repository scaffolding

Rules:
- treat this as partial
- preserve current generator patterns
- keep generated code deterministic

Validation:
- add focused generator tests if missing
- do not claim general database migration coverage

### Phase 5, ASP to Angular

Primary path:
- `src/Phase5-ASPtoAngular/`

Rules:
- treat everything here as prototype unless tests and executable pipeline code prove otherwise
- do not describe this phase as complete migration support
- prefer documenting gaps over hand-waving them

Typical acceptable work:
- improve scripts
- improve templates
- add small isolated transforms
- add prototype tests
- document intended pipeline steps

### Phase 6, Advanced Features

Primary path:
- `src/Phase6-AdvancedFeatures/`

Current safe scope:
- property procedure translation
- optional/default parameter handling

Typical tasks:
- extend property support carefully
- improve default value emission
- add parser or generator support for narrow advanced VB6 features

Rules:
- avoid bundling many advanced features into one patch
- prove each new feature with tests

### Phase 7, Optimization

Primary path:
- `src/Phase7-Optimization/`

Typical tasks:
- improve XML documentation generation
- extend dead-code cleanup helpers
- improve LINQ suggestion heuristics

Rules:
- do not present suggestions as guaranteed safe transformations without evidence
- separate advisory output from automatic rewriting unless tests prove safety

### Phase 8, Tooling

Primary path:
- `src/Phase8-Tooling/`

Rules:
- assume scaffold status
- add structure, diagnostics, and tooling carefully
- document what is real versus aspirational

## How To Implement New VB6 Features

For any new VB6 feature, follow this sequence:

1. Add or update a failing test that shows the VB6 input and expected C# output.
2. Update lexer support only if new tokens are needed.
3. Update parser support.
4. Update AST nodes or builder behavior only as needed.
5. Update symbol table or type inference only if semantics require it.
6. Update C# generation.
7. Run the test suite.
8. Update documentation for newly supported behavior.

Never start with a broad rewrite of the generator.

## Output Quality Rules For Generated C#

Generated C# should:
- be deterministic
- compile when the feature is meant to compile
- preserve VB6 behavior as closely as practical
- prefer simple idiomatic C# only where it does not erase VB6 intent
- keep names and ordering stable where possible
- avoid unnecessary magic refactors

Generated C# should not:
- silently change event semantics
- silently change default property semantics
- infer .NET behaviors that differ from VB6 without documentation
- flatten important control hierarchy information in form migrations

## Testing Rules

Before finishing any change:
- run the solution build
- run the test suite
- inspect failing tests before editing unrelated code
- keep skipped tests skipped unless the implementation now supports them
- add narrow tests for each new supported feature

Test philosophy:
- prefer one test per behavior slice
- avoid giant mixed tests
- keep fixtures readable
- use real VB6 snippets in tests

## Documentation Rules

When support changes:
- update the relevant phase README
- update status wording from "planned" to "partial" only when code and tests exist
- update from "partial" to "supported" only when the feature works across the intended slice and has tests

When support does not change:
- do not inflate docs

## Commit And Patch Style

Each patch should do one coherent thing.

Good examples:
- add `Exit For` parsing and generation with tests
- add `CheckBox.Value` property mapping in form generation with tests
- add composite key mapping case in `DbContextGenerator` with tests

Bad examples:
- refactor all phases for consistency
- modernize the whole compiler
- finish ASP migration
- rewrite codegen architecture

## Definition Of Done

A task is done only when all of these are true:

- the smallest requested behavior exists in code
- the behavior is covered by tests, or a clear test gap is documented and justified
- the solution still builds
- existing passing tests still pass
- docs reflect reality
- no exaggerated support claims were added

## Preferred Agent Prompt Pattern

When given a task, the agent should internally restate it like this:

- What exact VB6 behavior or repo slice is being added or fixed
- Which phase owns it
- Which tests prove it
- Which files need minimal edits
- What should remain untouched

Then execute only that slice.

## First Things The Agent Should Read

Before making changes, read these in order:

1. repository root `README.md`
2. `PHASE_STATUS.md`
3. `CURRENT_STATUS.md`
4. the README for the relevant phase
5. the tests that cover the target behavior

## Good Example Requests For This Repo

- Add support for `Exit Do` in the parser and C# generator, with tests.
- Extend Phase 3 to map VB6 `CheckBox` captions and checked state more accurately, with tests.
- Improve `DbContextGenerator.cs` to emit composite foreign key relationships, with tests.
- Extend property procedure support to handle a narrow additional case, with tests.
- Improve XML doc generation from VB6 comments in Phase 7 without changing unrelated output.

## Bad Example Requests For This Repo

- Finish the whole VB6 to C# converter.
- Convert the repo into a full commercial migration suite.
- Make ASP to Angular production-ready.
- Replace the compiler with Roslyn-based magic.
- Rewrite all code to use a new architecture.

## Final Instruction

Be useful, precise, and skeptical.

This repo has real progress. Respect it.
This repo also has prototypes. Label them honestly.
Ship the next verified inch.

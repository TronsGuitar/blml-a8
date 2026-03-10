# Phase3 Forms UI

## Status

- **Current status:** partially implemented and actively tested
- **Validated state:** form parsing, control generation, layout conversion helpers, and resource extraction have executable coverage in `BLML.Tests`
- **Known gap:** `vb6binary.cs` remains intentionally isolated and is not part of the active build

This file tracks the current `Phase3` implementation status.

## Implemented in the current Phase3 pass

### `src/Phase3-FormsUI/FormParsing/frmParser.cs`

- `ParseContent(string vb6FormContent)` now preserves nested control hierarchy
- `ConvertToIntermediateFormat(Vb6FormDefinition form)` now renders nested controls recursively

### `src/Phase3-FormsUI/FormParsing/Vb6FormCodeGenerator.cs`

- emits declarations for all parsed controls, including nested controls
- emits nested `Controls.Add(...)` statements for container hierarchies
- maps more intrinsic controls through `FrmParser.MapToCSharpControlType(...)`
- emits simple event subscriptions for `Click`, `DoubleClick`, and `Change`
- maps form-level `Caption` to `this.Text`

### `src/Phase3-FormsUI/ControlMapping/ActiveXFormCodeGenerator.cs`

- resolves wrapper-backed control types when an OCX path and wrapper path are available
- emits wrapper-backed field declarations and initialization statements
- supports end-to-end code generation using injected or default wrapper resolution

### `src/Phase3-FormsUI/ControlMapping/WinFormsTableLayoutConverter.cs`

- extracts designer control geometry
- rebuilds layout into multi-row and multi-column `TableLayoutPanel` output
- removes original `Location` and `Size` statements from rewritten output
- preserves unrelated metadata such as anchor statements

### `src/Phase3-FormsUI/Resources/ResourceExtractor.cs`

- parses `file:offset` references
- parses hex payload strings
- reads binary resource data from `.frx` offsets
- exports extracted resource bytes to output files

## Still partial

### `src/Phase3-FormsUI/FormParsing/Vb6FormCodeGenerator.cs`

- still emits a simplified WinForms class rather than full designer partials
- property mapping is still limited compared to real VB6 form metadata
- event generation is limited to a small set of event names

### `src/Phase3-FormsUI/ControlMapping/ActiveXFormCodeGenerator.cs`

- runtime success still depends on local registry state and `aximp.exe`
- wrapper namespace and emitted type naming are based on the generated wrapper file name convention

### `src/Phase3-FormsUI/ControlMapping/WinFormsTableLayoutConverter.cs`

- layout rebuilding is position-based and does not yet model all anchoring and docking intent
- nested designer container reconstruction is still limited

### `src/Phase3-FormsUI/Layout/LayoutConverter.cs`

- still provides planning helpers only
- does not yet emit full WinForms, Razor, or XAML layout output

### `src/Phase3-FormsUI/Resources/ResourceExtractor.cs`

- binary extraction is implemented, but resource-type detection and `.resx` conversion are still not implemented

## Intentionally isolated

### `src/Phase3-FormsUI/FormParsing/vb6binary.cs`

This file is still not part of the active build.

Reason:
- it depends on VB runtime compatibility types that are not present in the current workspace
- it has not yet been moved into a dedicated compatibility project

## Remaining follow-up

1. move `vb6binary.cs` into a compatibility project if that code path is still needed
2. expand golden-file coverage with more real `.frm` samples, especially nested, resource-heavy, and container-heavy forms
3. broaden property mapping, layout generation, and resource conversion beyond the current tested subset

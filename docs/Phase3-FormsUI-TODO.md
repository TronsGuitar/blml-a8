# Phase3 Forms UI status

## Completed

### Refactored Phase3 services

The prototype `Phase3` work now has callable, namespaced classes backed by shared typed models:

- `src/Phase3-FormsUI/FormParsing/frmParser.cs`
- `src/Phase3-FormsUI/FormParsing/Vb6FormCodeGenerator.cs`
- `src/Phase3-FormsUI/ControlMapping/ActiveXFormCodeGenerator.cs`
- `src/Phase3-FormsUI/ControlMapping/WinFormsTableLayoutConverter.cs`
- `src/Phase3-FormsUI/Layout/LayoutConverter.cs`
- `src/Phase3-FormsUI/Resources/ResourceExtractor.cs`
- `src/Phase3-FormsUI/Models/Vb6FormDefinition.cs`
- `src/Phase3-FormsUI/Models/Vb6ControlDefinition.cs`

### Tooling project

The old utility-style entry points were moved out of the main library into:

- `tools/BLML.Phase3.Tools/BLML.Phase3.Tools.csproj`
- `tools/BLML.Phase3.Tools/Program.cs`

Supported commands:

- `frm-parse <input.frm> <output.frmx>`
- `form-codegen <input.frm> <output.cs>`
- `activex-codegen <input.frm> <output.cs>`
- `tablelayout <inputDesigner> <outputDesigner> <inputResx> <outputResx>`

### Validation added

#### Existing parser coverage retained

`Vb6FormParser.Parser.FormParser.ParseForms` is still covered for:

- control discovery from `.frm` files
- recursive scanning of nested folders
- GUID capture from VB6 `Object = "{GUID}"` lines
- null GUID behavior when no object reference exists

#### New Phase3 coverage

Added active tests for:

- `FrmParser.ParseAndConvertToCSharp`
- nested container hierarchy parsing in `FrmParser.ParseContent`
- `Vb6FormCodeGenerator.ConvertToCSharp`
- nested container code generation and event hookup output
- `ActiveXFormCodeGenerator.ConvertToCSharp` with wrapper-backed output
- `LayoutConverter.BuildRowPlan`
- `LayoutConverter.ConvertTwipsToPixels`
- `ResourceExtractor.TryParseResourceReference`
- `ResourceExtractor.ParseHexPayload`
- `ResourceExtractor.ExtractBinaryResource`
- `WinFormsTableLayoutConverter.ExtractControls`
- `WinFormsTableLayoutConverter.RebuildWithTableLayout`

Golden-file fixtures were added under:

- `tests/BLML.Tests/TestData/Phase3FormsUi/`

## Intentionally isolated

`src/Phase3-FormsUI/FormParsing/vb6binary.cs` remains excluded from compilation.

Reason:

- it depends on VB runtime compatibility types that are not present in this workspace
- it appears to be compatibility/reference code rather than part of the active `Phase3` pipeline

## Remaining follow-up

1. Decide whether `vb6binary.cs` should move into a dedicated compatibility project with restored dependencies.
2. Expand the golden-file set with more real-world `.frm` samples, especially more container-heavy and resource-heavy forms.
3. Broaden property mapping and layout generation beyond the current tested subset.

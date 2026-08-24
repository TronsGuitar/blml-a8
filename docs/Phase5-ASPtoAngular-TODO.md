# Phase5 ASP to Angular status

## Completed

### Documented current prototype surface area

The repository now explicitly tracks the current `Phase5` prototype files under:

- `src/Phase5-ASPtoAngular/RazorPages/Scripts/`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/`
- `src/Phase5-ASPtoAngular/RazorPages/Styles/`

### Prototype assets inventoried

Current script assets:

- `acesss2razor.ps1`
- `accdb2sql.ps1`
- `providers.ps1`

Current template assets:

- `blazer.razor`
- `gindex.razor`
- `gnav.razor`
- `gnavmenu.razor`
- `glayiut.razor`
- `gtableviewer.razor`
- `gqueryeditor.razor`
- `gformviewer.razor`
- `greportviewer.razor`

Current style asset:

- `gapp.css`

### Validation added

Added repository-level tests for:

- expected `Phase5` script, template, and style file presence
- route and section placeholder coverage across the current `.razor` prototypes
- `src/Phase5-ASPtoAngular/README.md` content that documents the current state
- this `docs/Phase5-ASPtoAngular-TODO.md` status file

## Current state

The `Phase5` folder is still a prototype staging area.

It currently contains PowerShell, Blazor, and Razor Pages artifacts that sketch an Access-like web UI, but it does **not** yet implement the planned Angular migration pipeline described in `ProjectPlan.md`.

## Remaining follow-up

1. ~~Build the actual ASP analysis and conversion pipeline.~~ Done - see below.
2. ~~Decide whether the target output is Angular, Blazor, Razor Pages, or a split architecture.~~
   Decided: Angular. The `RazorPages/` prototype above is unaffected/unchanged; it just isn't
   the direction taken.
3. Replace placeholder navigation and viewer templates with generated artifacts driven by real
   schema and page metadata. (Applies to the `RazorPages/` prototype specifically; the Angular
   pipeline below generates its own templates from real page metadata.)
4. ~~Add executable conversion tests with sample classic ASP inputs and expected outputs.~~ Done
   for the Angular pipeline - see `tests/BLML.Tests/Phase5AspToAngularTests.cs`.
5. Bring naming, packaging, and project integration up to production quality. (Still applies to
   `RazorPages/`; not attempted here.)

## Angular pipeline added

A real ASP-to-Angular conversion pipeline now exists under `src/Phase5-ASPtoAngular/` alongside
(not replacing) the `RazorPages/` prototype: `AspParser/` (classic ASP/VBScript lexer and
parser), `Analysis/` (business-logic classification, session tracking, ADO call analysis, page
flow), `Backend/` (.NET 8 Web API generation), `Frontend/` (standalone Angular 17+ generation
with `AngularAntiPatternChecker`), and `Database/` (delegates to Phase 4's EF Core/schema
generators). `AspProjectConverter` runs the whole thing end-to-end, exposed via the CLI as
`convert-asp-project`. See `src/Phase5-ASPtoAngular/README.md`'s "Platform decision" section for
the full breakdown, and `tests/BLML.Tests/Phase5AspToAngularTests.cs` for coverage of every
piece plus an end-to-end test.

Follow-up items 3 and 5 above still apply to the `RazorPages/` prototype specifically, which was
out of scope for this pass.

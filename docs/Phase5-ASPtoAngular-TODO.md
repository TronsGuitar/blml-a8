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

1. Build the actual ASP analysis and conversion pipeline.
2. Decide whether the target output is Angular, Blazor, Razor Pages, or a split architecture.
3. Replace placeholder navigation and viewer templates with generated artifacts driven by real schema and page metadata.
4. Add executable conversion tests with sample classic ASP inputs and expected outputs.
5. Bring naming, packaging, and project integration up to production quality.

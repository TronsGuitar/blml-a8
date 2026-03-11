# Phase5 ASP to Angular

## Status

- **Current status:** prototype-only and documentation-backed
- **Validated state:** repository tests verify current script/template inventory and README/status coverage
- **Known gap:** there is still no active ASP migration pipeline, Angular project, or executable web-generation flow

## Current Phase5 surface area

The current `Phase5-ASPtoAngular` folder contains prototype assets under `RazorPages`:

- `src/Phase5-ASPtoAngular/RazorPages/Scripts/acesss2razor.ps1`
- `src/Phase5-ASPtoAngular/RazorPages/Scripts/accdb2sql.ps1`
- `src/Phase5-ASPtoAngular/RazorPages/Scripts/providers.ps1`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/blazer.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/gindex.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/gnav.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/gnavmenu.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/glayiut.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/gtableviewer.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/gqueryeditor.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/gformviewer.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Templates/greportviewer.razor`
- `src/Phase5-ASPtoAngular/RazorPages/Styles/gapp.css`

## Implemented in this pass

The current Phase5 inventory is now documented and covered by lightweight repository tests.

The existing prototype assets currently provide:

- Access database inspection through `providers.ps1`
- Access-to-SQL export scaffolding through `accdb2sql.ps1`
- Access-to-Razor Pages scaffolding through `acesss2razor.ps1`
- placeholder Blazor navigation through `gindex.razor`, `gnav.razor`, and `gnavmenu.razor`
- placeholder section views for tables, queries, forms, and reports
- a layout and stylesheet prototype through `glayiut.razor` and `gapp.css`
- a monolithic Access-like Blazor shell prototype in `blazer.razor`

## Not implemented yet

### `src/Phase5-ASPtoAngular/RazorPages`

- no Angular project, components, routing module, or TypeScript services exist yet
- no classic ASP parser or migration pipeline is present in the active codebase
- no generated .NET API layer exists for the placeholder pages to call
- no real metadata-driven table, query, form, or report rendering is implemented
- no build integration currently compiles or validates the `.razor` templates
- several filenames are still prototype-quality, including `acesss2razor.ps1` and `glayiut.razor`

## TODO

1. decide whether Phase5 should stay Blazor/Razor-based, or align the implementation with the planned Angular target in `ProjectPlan.md`
2. replace placeholder `.razor` templates with metadata-driven generation from classic ASP and Access artifacts
3. add a real parser and conversion pipeline for ASP pages, inline VBScript, and page flow
4. generate an API/backend layer for data access, authentication, and query execution
5. normalize prototype file names and fold the scripts/templates into a supported tool or project structure
6. add executable tests that validate generated web artifacts instead of only repository/documentation coverage

## What is left to do now

- make the platform decision for this phase: Angular as planned, or a supported Blazor/Razor target
- replace the current prototype scripts and placeholder templates with a real migration pipeline
- add ASP/VBScript parsing, backend generation, and metadata-driven UI generation
- move from documentation-only coverage to executable generation and validation of produced web artifacts

# Phase8 Tooling - IDE

## Purpose

This folder should become the IDE integration surface for BLML.

Its project should provide editor-aware tooling for VB6 source files, beginning with language-server support and expanding into richer IDE workflows.

## Current assets

- `vb6-lsp.cs` contains an early language server prototype with initialize, hover, and text-change handlers.
- `lspconfig.json` contains a starter configuration for mapping VB6 file patterns to the language server executable.
- repository planning documents also reference `blmlide.cs`, `blmlide.designer.cs`, and `vb6languageserverclient.cs`, which are not currently present in this folder.

## Project scope

The future `IDE` project should:

- host a VB6 language server process
- integrate BLML parsing and semantic analysis into editor features
- support Visual Studio and LSP-capable editors where practical
- expose diagnostics and quick feedback while users inspect legacy VB6 code

## Functional requirements

### Language server foundation

The project must support:

- LSP initialization and shutdown
- document open, change, save, and close notifications
- text synchronization for VB6 project and source file types
- server capability advertisement
- configuration-driven startup for editor clients

### Editor features

The project should implement:

- syntax-aware hover information
- diagnostics for parse and conversion issues
- completion for VB6 keywords, constants, and common built-ins
- go-to definition for symbols resolved by the BLML symbol table
- find references where Phase1 symbol analysis is available
- document symbols and outline support

### Conversion-aware tooling

The IDE integration should expose:

- warnings for constructs that need manual migration
- links between VB6 source regions and generated C# output when available
- phase-specific recommendations based on parser and analyzer output
- surfaced TODO or unsupported-feature reports directly in the editor

## Non-functional requirements

- target `.NET 8`
- keep the language server responsive for interactive editing
- separate transport concerns from parsing and analysis logic
- avoid editor-specific dependencies in the core LSP host where possible
- support structured logging for troubleshooting client/server issues

## Current gaps

The existing prototype is only a starting point.

Current limitations include:

- hover currently inspects the document URI string rather than document text
- no workspace or document store is present
- no diagnostics pipeline is connected to the parser
- no completion, definitions, references, or semantic symbol support is implemented yet
- no Visual Studio-specific integration project exists yet

## Implementation plan

1. create a dedicated project for the LSP host in this folder
2. add a document manager for in-memory text synchronization
3. route parse requests through `Phase1-Foundation` services
4. implement diagnostics from lexer, parser, and symbol analysis
5. add completion, document symbols, definitions, and references
6. separate reusable protocol handlers from any Visual Studio-specific client layer
7. add integration tests for initialize, hover, completion, and diagnostics flows

## Initial project structure

Recommended files for the future project:

- `Program.cs`
- `LanguageServerHost.cs`
- `Documents/DocumentStore.cs`
- `Handlers/InitializeHandler.cs`
- `Handlers/HoverHandler.cs`
- `Handlers/CompletionHandler.cs`
- `Handlers/DiagnosticsHandler.cs`
- `Handlers/DefinitionHandler.cs`
- `Configuration/LspOptions.cs`
- `Services/Vb6AnalysisService.cs`

## Open questions

- whether Visual Studio integration should remain in this folder or move into a sibling project once it grows beyond pure LSP support
- whether the language server should analyze single files only or understand full `.vbp` project context from the start
- how generated C# mapping should be stored for source-to-output navigation

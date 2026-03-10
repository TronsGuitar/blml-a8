# Phase8 Tooling - VSCode

## Status

- **Current status:** grammar-only prototype asset
- **Validated state:** the folder currently contains a starter `VB6.tmLanguage` grammar file
- **Known gap:** there is no VS Code extension project, activation code, or language-server integration yet

## Purpose

This folder should become the Visual Studio Code packaging layer for BLML editor support.

Its project should wrap grammar files, extension metadata, configuration, and language-server wiring into a usable VS Code extension.

## Current assets

- `VB6.tmLanguage` provides a starter TextMate grammar for VB6 syntax highlighting.

## Project scope

The future `VSCode` project should:

- register VB6-related file extensions
- provide syntax highlighting and language configuration
- start the BLML language server from the `IDE` tooling output
- expose commands and settings for VB6 analysis and conversion workflows

## Functional requirements

### Language registration

The extension must register support for common VB6 file types, including:

- `.vbp`
- `.bas`
- `.cls`
- `.frm`
- `.ctl`
- `.pag`
- `.dsr`
- `.dob`

### Syntax highlighting

The extension should provide:

- keyword highlighting
- string and comment highlighting
- intrinsic type and constant highlighting
- function and subroutine declaration highlighting
- better grammar coverage for line continuations, attributes, and VB6-specific declarations

### Language server integration

The extension must:

- launch the language server executable from the `IDE` project output
- pass configuration to the server
- support trace logging for troubleshooting
- handle restart and crash scenarios cleanly

### UX requirements

The extension should include:

- commands for analyze and convert actions
- settings for tool paths and logging levels
- problem reporting through the VS Code diagnostics experience
- basic documentation for installation and troubleshooting

## Non-functional requirements

- keep extension startup lightweight
- avoid duplicating semantic logic already handled by the language server
- keep grammar-only functionality usable even when the server is unavailable
- package the extension so it can be published or side-loaded easily

## Current gaps

The current folder only contains a grammar file.

Missing pieces include:

- `package.json`
- language configuration rules
- extension activation code
- wiring to the language server
- automated extension tests

## Implementation plan

1. create a VS Code extension project in this folder
2. keep `VB6.tmLanguage` as the initial grammar source
3. add `package.json` contributions for language registration and grammar mapping
4. add extension activation code that starts the BLML language server
5. add user settings for server path, trace mode, and conversion commands
6. add smoke tests for activation and grammar registration

## Initial project structure

Recommended files for the future project:

- `package.json`
- `language-configuration.json`
- `syntaxes/VB6.tmLanguage`
- `src/extension.ts`
- `src/languageClient.ts`
- `README.md`
- `.vscodeignore`

## Open questions

- whether the extension should bundle the language server or require a separate install
- whether conversion commands should be local extension commands or delegated fully to the CLI project
- whether grammar maintenance should stay in XML plist format or move to JSON-based grammar sources

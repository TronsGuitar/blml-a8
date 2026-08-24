# Phase8 Tooling - CLI

## Status

- **Current status:** dedicated `.NET 8` console application with active command handling
- **Validated state:** the CLI now has its own project, references the transpiler, and is covered by executable tests
- **Known gap:** command coverage is still partial and project-wide orchestration remains limited

## Purpose

This folder should become the command-line entry point for the BLML toolchain.

Its project should provide a stable automation surface for running VB6 analysis and conversion steps without requiring an IDE.

## Current assets

- `BLML.Tooling.Cli.csproj` is the dedicated console application project.
- `CommandLineInterface.cs` now contains command parsing, dispatch, progress reporting, and exit-code handling.
- `mainprogm.cs` is now the real console entry point.

## Project scope

The future `CLI` project should:

- accept VB6 project, folder, or file input
- invoke the existing BLML phase libraries
- write generated output to a user-specified directory
- expose analysis, conversion, reporting, and validation commands
- support local use, CI, and scripted automation

## Functional requirements

### Implemented command surface

The CLI currently supports:

- `analyze`
- `convert`
- `validate`
- `form-export`
- `help`

Supported options include:

- `--input` / `-i`
- `--output` / `-o`
- `--phase` / `-p`
- `--verbose` / `-v`
- `--help` / `-h`

### Command surface

The CLI should support commands such as:

- `analyze`
- `convert`
- `validate`
- `report`
- `form-export`

Each command should support `--help` output and consistent argument naming.

### Input handling

The CLI must accept:

- `.vbp` project files
- individual VB6 source files such as `.bas`, `.cls`, and `.frm`
- folder-based project roots
- optional `.zip` packages for batch workflows

### Output handling

The CLI must support:

- output directory selection
- overwrite or safe-write modes
- machine-readable report output such as JSON
- human-readable console summaries
- generated artifacts such as C# files, CSV exports, and migration reports

### Execution behavior

The CLI must provide:

- deterministic exit codes
- structured error messages
- verbose and diagnostic logging modes
- progress reporting for long-running conversions
- cancellation support where practical

## Non-functional requirements

- target `.NET 8` (`net8.0-windows` for generated WinForms projects, `net8.0` for libraries)
- generated projects use **C# 12** (`LangVersion 12`)
- keep startup fast for simple commands such as `--help` and validation
- avoid direct UI dependencies
- be script-friendly for PowerShell and CI pipelines
- keep dependencies minimal unless a command parser library clearly improves maintainability

## Implementation plan

Completed in this pass:

1. created a dedicated console project in this folder
2. moved `mainprogm.cs` sample behavior behind formal commands
3. replaced placeholder `CommandLineInterface` logic with a real command dispatcher
4. connected commands to active transpiler entry points in Phase1 and Phase3
5. added JSON report output for automation scenarios
6. added tests for argument parsing, exit codes, and file-output flows

Remaining follow-up:

1. broaden command coverage to include richer project-wide conversion orchestration
2. add more reporting, validation, and diagnostics commands
3. decide whether to adopt `System.CommandLine` or keep the lightweight parser
4. connect the CLI to more Phase4 through Phase7 orchestration paths
5. add more failure-path and end-to-end integration tests

## Initial project structure

Recommended files for the future project:

- `Program.cs`
- `Commands/AnalyzeCommand.cs`
- `Commands/ConvertCommand.cs`
- `Commands/ValidateCommand.cs`
- `Commands/FormExportCommand.cs`
- `Options/*.cs`
- `Services/CliOrchestrationService.cs`
- `Services/ConsoleReporter.cs`
- `Models/CliResult.cs`

## Open questions

- whether to use `System.CommandLine` or a lightweight custom parser
- how much of Phase3 form export should remain a standalone command versus part of `convert`
- whether report generation belongs here or in a shared orchestration library

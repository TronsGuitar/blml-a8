# Phase8 Tooling - CLI

## Purpose

This folder should become the command-line entry point for the BLML toolchain.

Its project should provide a stable automation surface for running VB6 analysis and conversion steps without requiring an IDE.

## Current assets

- `CommandLineInterface.cs` contains the intended CLI responsibilities as TODO notes.
- `mainprogm.cs` is an early sample program that parses VB6 form files and emits CSV and project output.

## Project scope

The future `CLI` project should:

- accept VB6 project, folder, or file input
- invoke the existing BLML phase libraries
- write generated output to a user-specified directory
- expose analysis, conversion, reporting, and validation commands
- support local use, CI, and scripted automation

## Functional requirements

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

- target `.NET 8`
- keep startup fast for simple commands such as `--help` and validation
- avoid direct UI dependencies
- be script-friendly for PowerShell and CI pipelines
- keep dependencies minimal unless a command parser library clearly improves maintainability

## Implementation plan

1. create a dedicated console project in this folder
2. move `mainprogm.cs` sample behavior behind a formal command
3. replace placeholder `CommandLineInterface` logic with a real command dispatcher
4. connect commands to Phase1 through Phase7 library entry points
5. add JSON report models for automation scenarios
6. add tests for argument parsing, exit codes, and failure paths

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

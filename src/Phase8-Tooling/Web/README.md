# Phase8 Tooling - Web

## Status

- **Current status:** placeholder implementation only
- **Validated state:** `WebConverter.cs` is present and the placeholder tooling file compiles as part of the active `.NET 8` solution
- **Known gap:** there is no ASP.NET Core host, UI, background job system, or artifact delivery pipeline yet

## Purpose

This folder should become the web-hosted tooling surface for BLML.

Its project should provide a browser-based experience for uploading VB6 applications, running conversion workflows, reviewing results, and downloading generated output.

## Current assets

- `WebConverter.cs` contains placeholder requirements for a web-based conversion dashboard.

## Project scope

The future `Web` project should:

- host a web UI for BLML conversion workflows
- orchestrate conversion jobs against the shared phase libraries
- present progress, reports, and downloadable artifacts
- support review scenarios where users inspect differences before accepting generated output

## Functional requirements

### Upload and project intake

The web application must support:

- upload of `.vbp` files and supporting VB6 source files
- upload of `.zip` packages for multi-file projects
- server-side validation of accepted file types
- storage isolation per job or session

### Conversion workflow

The web application must provide:

- job creation and tracking
- selection of target output type or migration phase
- queued or background execution for long-running conversions
- progress updates during analysis and generation

### Review experience

The web application should include:

- a summary dashboard of detected modules, forms, and risks
- diagnostics and unsupported-feature reporting
- side-by-side or unified diff views comparing VB6 input and generated C# output
- downloadable reports and artifacts

### Delivery

The web application must support:

- artifact download as a `.zip` package
- retry or rerun of failed jobs
- basic audit metadata for job history

## Non-functional requirements

- target `.NET 8`
- isolate conversion work from the web request thread
- protect uploaded code and temporary artifacts
- support cancellation, timeout handling, and cleanup of abandoned jobs
- keep orchestration logic reusable so it can also be called from the CLI

## Current gaps

The current folder does not yet contain a web application.

Missing pieces include:

- web host project files
- controllers or endpoints
- UI pages or components
- background job orchestration
- storage abstraction for uploads and generated output
- authentication or tenancy decisions

## Implementation plan

1. create a dedicated ASP.NET Core web project in this folder
2. move `WebConverter.cs` responsibilities into web-facing services and job orchestration
3. add upload endpoints and validation
4. add background processing for conversion runs
5. add SignalR or equivalent progress updates
6. add diff and artifact download views
7. add tests for upload validation, job execution, and artifact delivery

## Initial project structure

Recommended files for the future project:

- `Program.cs`
- `Controllers/ConversionController.cs`
- `Hubs/ConversionProgressHub.cs`
- `Services/ConversionJobService.cs`
- `Services/ArtifactPackagingService.cs`
- `Models/ConversionJob.cs`
- `Models/UploadRequest.cs`
- `Views` or `Components` depending on the chosen UI stack

## Open questions

- whether this project should be MVC, Razor Pages, Blazor Server, or a thin API plus separate frontend
- what storage model should be used for uploads and generated artifacts
- whether authentication is required for the first version or only for hosted multi-user deployment

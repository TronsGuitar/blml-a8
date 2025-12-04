# AngularTutor bootstrap instructions

This repository contains a starter Visual Studio 2022 solution (`AngularTutor.sln`) with an ASP.NET Core Web API project intended to back an Angular front end.
Use this guide to finish setting up the stack and create a working site that combines Angular, C# .NET Core, and SQL Server.

## Prerequisites
- Visual Studio 2022 with **ASP.NET and web development** workload installed.
- SQL Server (Express or Developer) reachable from your development machine.
- Node.js 18+ and the Angular CLI (`npm install -g @angular/cli`) for building the client.

## Solution layout
```
AngularTutor.sln
└─ AngularTutor\              # ASP.NET Core Web API
   ├─ AngularTutor.csproj
   ├─ Program.cs
   ├─ Controllers\StatusController.cs
   └─ appsettings.json        # contains a starter SQL Server connection string
```

### How to open in VS 2022
1. Open `AngularTutor.sln` in Visual Studio 2022.
2. Restore NuGet packages if prompted.
3. Set **AngularTutor** as the startup project.
4. Press **F5** to launch the API with Swagger UI (available in Development environment).

## Building the API
1. Right-click the **AngularTutor** project → **Manage NuGet Packages** → install:
   - `Microsoft.EntityFrameworkCore.SqlServer`
   - `Microsoft.EntityFrameworkCore.Tools`
   - `Swashbuckle.AspNetCore` (already referenced in `Program.cs` for Swagger)
2. Update `appsettings.json` with your SQL Server connection string under `ConnectionStrings:SqlServer`.
3. Add your data models and `DbContext`, e.g. `Data/ApplicationDbContext.cs` with `DbSet<T>` properties.
4. Register the context in `Program.cs`:
   ```csharp
   builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
   ```
5. Create a RESTful controller per resource (e.g. `Controllers/CoursesController.cs`) using `[ApiController]`, `GET/POST/PUT/DELETE` actions, and `IActionResult` return types.
6. Use Entity Framework Core migrations to create/update the database:
   - **Package Manager Console**: `Add-Migration InitialCreate` then `Update-Database`.
   - Or **dotnet CLI**: `dotnet ef migrations add InitialCreate` then `dotnet ef database update`.

## Building the Angular client
1. From the repository root, scaffold an Angular app (outside the API project folder):
   ```bash
   ng new angular-tutor-client --routing --style=scss
   cd angular-tutor-client
   ng serve -o
   ```
2. Create Angular services that consume the API via `HttpClient` (e.g. `status.service.ts`, `courses.service.ts`). Point the base URL to your API host/port.
3. Build pages/components that match your API resources. Use Angular routing to organize feature modules.
4. For a unified developer experience, configure an Angular proxy (`proxy.conf.json`) to forward `/api` calls to `https://localhost:5001` and run `ng serve --proxy-config proxy.conf.json`.

## Connecting Angular and ASP.NET Core
- Enable CORS in `Program.cs` so the Angular dev server can call the API:
  ```csharp
  var allowedOrigins = "AngularDev";
  builder.Services.AddCors(options =>
      options.AddPolicy(allowedOrigins, policy =>
          policy.WithOrigins("https://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()));

  app.UseCors(allowedOrigins);
  ```
- Expose well-documented RESTful endpoints and verify them via Swagger before wiring up Angular.
- If you prefer a single deployable artifact, you can host the Angular build output in `wwwroot` by adding `builder.Services.AddSpaStaticFiles` and copying `dist/` assets into the API project during publish.

## Database considerations
- Use code-first migrations for schema evolution; keep migrations checked into source control.
- Seed reference data in a hosted service or migration to keep environments consistent.
- For local development, use SQL Server Express LocalDB or a containerized SQL Server instance.

## Quality and testing
- Write integration tests against in-memory test servers using `WebApplicationFactory` to validate REST endpoints.
- Add Angular unit tests (`ng test`) and end-to-end tests (`ng e2e`) as you build features.
- Use Swagger/OpenAPI for contract verification between the Angular client and ASP.NET Core API.

With these steps, you can grow the starter API into a full Angular + ASP.NET Core + SQL Server application while staying productive in Visual Studio 2022.

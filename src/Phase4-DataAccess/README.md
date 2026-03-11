# Phase4 Data Access

## Status

- **Current status:** partially implemented
- **Validated state:** `DbContextGenerator.cs` has executable coverage and the current Phase4 README status is covered by tests
- **Known gap:** `EntityGenerator.cs`, `SchemaGenerator.cs`, `DataMigration.cs`, and `AdoConverter.cs` are still placeholder-level

## Current Phase4 surface area

The current `Phase4-DataAccess` folder contains the following active or partially active utilities:

- `src/Phase4-DataAccess/Access/msaccess64bit.py`
- `src/Phase4-DataAccess/EntityFramework/DbContextGenerator.cs`
- `src/Phase4-DataAccess/EntityFramework/EntityGenerator.cs`
- `src/Phase4-DataAccess/SqlServer/SchemaGenerator.cs`
- `src/Phase4-DataAccess/SqlServer/DataMigration.cs`
- `src/Phase4-DataAccess/ADO/AdoConverter.cs`

## Implemented in this pass

The Access driver now supports:

- safer connection lifecycle management
- context-manager usage via `with Access64Driver(...)`
- dependency injection of the ODBC module for tests
- connection string construction through `_build_connection_string()`
- table discovery through `list_tables()`
- idempotent `connect()` behavior
- better cleanup in `close()`
- file existence validation before opening the database

The Entity Framework generator now supports:

- generating an EF Core `DbContext` source file as a string without taking a runtime dependency on EF Core in this project
- generating `DbSet<TEntity>` properties for configured tables
- generating `OnModelCreating(...)` mappings with `ToTable(...)`, single-column keys, composite keys, and relationship `HasOne(...).WithMany(...).HasForeignKey(...)` output
- generating `OnConfiguring(...)` code that resolves the connection string from environment variables and `appsettings.json`
- generating optional repository and unit-of-work scaffolding source for downstream projects

## Not implemented yet

### `src/Phase4-DataAccess/Access/msaccess64bit.py`

- no schema introspection beyond `list_tables()`
- no transaction rollback helper
- no parameter type normalization for Access-specific edge cases
- no migration pipeline from VB6 DAO/ADO code into this driver
- no structured logging or retry policy
- no packaging or install story for Python dependencies such as `pyodbc`

### `src/Phase4-DataAccess/EntityFramework/DbContextGenerator.cs`

- the generator emits source text only; it does not yet compile or apply the generated code
- entity metadata still has to be supplied manually because schema extraction is not wired in yet
- navigation-property generation only covers the relationship metadata explicitly provided to the generator
- repository scaffolding is generic and not yet integrated with a concrete application architecture

### Other Phase4 .NET stubs

- `EntityGenerator.cs`, `SchemaGenerator.cs`, `DataMigration.cs`, and `AdoConverter.cs` are still placeholders
- there is not yet an end-to-end Access-to-SQL-Server-to-EF-Core migration pipeline
- there are no integration tests yet that connect the Python Access driver to the .NET generators

## TODO

1. add schema helpers such as column discovery and primary-key inspection to the Access driver
2. implement `SchemaGenerator.cs` so `DbContextGenerator` and `EntityGenerator` can consume real metadata instead of hand-authored definitions
3. implement `EntityGenerator.cs` and `DataMigration.cs` for end-to-end SQL Server and EF Core scaffolding
4. implement `AdoConverter.cs` to migrate VB6/ADO usage toward ADO.NET or repository-based access
5. add integration tests that connect schema extraction, EF scaffolding, and a real `.mdb` or `.accdb` sample when fixtures are available

## What is left to do now

- move from manually supplied metadata to real schema extraction
- implement the placeholder .NET generators so Phase4 becomes an end-to-end migration pipeline rather than isolated helpers
- connect Access discovery, SQL Server scaffolding, EF Core generation, and data migration into a single supported flow
- add integration coverage against representative Access fixtures when they are available

# Backend Conventions (C# / .NET)

## Hard rules (never violate)

- No business logic in API endpoints or gRPC services.
- No mapping outside `/Core/` (use `*MappingHelpers` static helpers).
- No direct gRPC usage outside `Client`/`Service` layers.
- No manual `new Service()` — use DI.
- No `.Result` or `.Wait()` — `async` only.
- Every public async method takes a `CancellationToken`.
- No circular dependencies.
- No DbContext usage outside `*.Data` projects.

## Per-layer rules

**API (`MTGArchitectServices.ApiService/Endpoint.cs`)**
- Use `EndpointExtensions`, call only the Controller.
- No LINQ, no mapping, no logic.
- Pattern:
  ```csharp
  app.MapGet("/route", async (..., [FromServices] XController ctrl, CancellationToken ct)
      => await ctrl.Method(...));
  ```

**Controller**
- Holds business logic and orchestration.
- Depends on interfaces only.
- No mapping, no proto types, no `ServerCallContext`.

**Client (`Scryfall.Client`)**
- Wraps gRPC calls behind interfaces.
- Maps proto ⇄ domain via `MappingHelpers`.
- No business logic.

**gRPC Service (`Scryfall.Service`)**
- Delegates to Controller and returns mapped result.
- No logic, no loops, no conditionals.

**Core / Mapping**
- Only static helpers and pure functions live here.
- No services, no logic.

**Data (`*.Data` projects, e.g. `MTGArchitect.Data`)**
- All database access (EF Core, DbContext) lives exclusively here.
- No direct DbContext usage outside a `*.Data` project.
- Every data method is exposed through an interface (`I*Repository`, `I*DataService`).
- Implementations are `internal sealed` — never expose concrete classes outside the project.
- Register via a dedicated extension method (`Add*Data()`), not inline in the host.
- Naming: `*Repository` for CRUD/query access, `*DataService` for multi-step data operations.

## Golden rule

> **Lowest valid layer wins. Never break separation of concerns.**


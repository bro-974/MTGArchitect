# CLAUDE.md

Guidance for Claude when working in the **MTGArchitect** repository.
A web UI for creating and managing Magic: The Gathering (MTG) card decks, with AI-assisted features and Scryfall integration.

This file consolidates conventions from the existing Copilot instructions
(`back/.copilot-instructions.md`, `.github/instructions/dotnet.instructions.md`,
`.github/instructions/angular.instructions.md`) and the repository layout.
Read those files for full detail; this document is the authoritative summary for Claude.

---

## 1. High-Level Architecture

MTGArchitect is a distributed application orchestrated by **.NET Aspire**.

```
front/  ───────────── Angular 21 SPA (PrimeNG + Transloco)
back/
  ├── AppHost/                 .NET Aspire orchestrator (Redis + Postgres + services + frontend)
  ├── Api/                     MTGArchitect.Api         — public REST API (decks, cards, user)
  ├── Auth.Api/                MTGArchitect.Auth.Api    — JWT auth (register / login / me / logout)
  ├── Scryfall.Service/        gRPC service wrapping Scryfall
  ├── Scryfall.Client/         gRPC client used by Api
  ├── Scryfall.Contracts/      shared DTOs (Card, CardQuerySearch, …)
  ├── Data/                    EF Core models, repositories, auth data service
  └── ServiceDefaults/         Aspire service defaults
MCP/spec-mcp/                  MCP server exposing MTG rules/spec PDFs as tools
```

### Mandatory call chain (backend)

```
API (endpoint) → Controller → Client → gRPC Service → Core (Mapping)
```

**Never skip layers.** Every cross-boundary call goes through the layer above it.

### Runtime services (Aspire)

- `cache` — Redis
- `postgres` / `authdb` — Postgres database
- `scryfallservice` — gRPC
- `authapiservice` — REST, depends on `authdb`
- `apiservice` — REST, depends on `scryfallservice` + `authapiservice` + `authdb`, exposed on port **4300**
- `frontend` — `npm start` from `../../front`, port **4201**

Start the whole stack from the repo root with:
```
dotnet run --project .\back\MTGArchitectServices.AppHost\MTGArchitectServices.AppHost.csproj
```

---

## 2. Backend Conventions (C# / .NET)

Source of truth: `back/.copilot-instructions.md` and `.github/instructions/dotnet.instructions.md`.
Applies to `back/**/*.{cs,csproj,props,targets,json}`.

### Hard rules (never violate)

- No business logic in API endpoints or gRPC services.
- No mapping outside `/Core/` (use `*MappingHelpers` static helpers).
- No direct gRPC usage outside `Client`/`Service` layers.
- No manual `new Service()` — use DI.
- No `.Result` or `.Wait()` — `async` only.
- Every public async method takes a `CancellationToken`.
- No circular dependencies.

### Per-layer rules

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

### Naming

| Kind        | Suffix / prefix       |
|-------------|-----------------------|
| Controller  | `*Controller`         |
| Handler     | `*Handler`            |
| Service     | `*Service`            |
| Client      | `*Client`             |
| Interface   | `I*`                  |
| Mapping     | `*MappingHelpers`     |

### Method rules

- `async` only, `CancellationToken` required.
- ≤ 20 lines recommended.

### Golden rule

> **Lowest valid layer wins. Never break separation of concerns.**

### AI decision guide

- Business logic → Controller
- External call → Client
- Mapping → Core
- HTTP endpoint → API
- gRPC exposure → Service + Controller

### Pre-generation checklist

- Correct layer?
- No forbidden patterns?
- Mapping isolated in `/Core/`?
- Interfaces used?
- Async respected?

---

## 3. Frontend Conventions (Angular / TypeScript)

Source of truth: `.github/instructions/angular.instructions.md`.
Applies to `front/**/*.{ts,tsx,js,jsx,html,css,scss}`.

Stack: **Angular 21**, **PrimeNG 21** + PrimeFlex + `@primeuix/themes` (Aura preset),
**Transloco** for i18n, Vitest + jsdom for tests, Prettier (print width 100, single quotes).

### TypeScript

- Strict type checking.
- Prefer type inference when the type is obvious.
- Avoid `any`; use `unknown` when type is uncertain.

### Angular

- Standalone components only — **do not** set `standalone: true` (it's the default in v20+).
- Use **signals** for state; `computed()` for derived state; `update`/`set` (never `mutate`).
- Lazy-load feature routes.
- Do **not** use `@HostBinding` / `@HostListener` — use the `host` object in the decorator.
- Use `NgOptimizedImage` for static images (not for base64).
- `changeDetection: ChangeDetectionStrategy.OnPush` on every `@Component`.
- `input()` / `output()` functions — not decorators.
- Reactive forms over template-driven.
- Use `class` and `style` bindings — not `ngClass` / `ngStyle`.
- Use native control flow: `@if`, `@for`, `@switch` (not `*ngIf`, `*ngFor`, `*ngSwitch`).
- Use the async pipe for observables.
- Don't assume globals (e.g. `new Date()`) — inject.

### Services

- Single responsibility.
- `providedIn: 'root'` for singletons.
- Use the `inject()` function — not constructor injection.

### Environment & API URLs

- **Never** hardcode API base URLs.
- All API roots come from `src/environments/environment.ts` (dev) and `environment.prod.ts` (prod).
- `angular.json` production config must include a `fileReplacements` entry for these.
- Example:
  ```typescript
  import { environment } from '../../../environments/environment';
  this.http.get(`${environment.authApiUrl}/api/auth/me`);
  ```

### PrimeNG theme

- PrimeNG is the default visual system; configure via `providePrimeNG` in `src/app/app.config.ts`.
- Default preset: **Aura**.
- Dark mode via CSS class `.app-dark` on `document.documentElement`.
- Persist theme in `localStorage`, restore at startup.
- Prefer PrimeNG components (`p-button`, `p-select`, …) and PrimeFlex utilities over custom layout.
- Ask the user before deep-customizing PrimeNG visuals/behavior.

### i18n (Transloco)

- `@jsverse/transloco` runtime i18n (not Angular compile-time).
- Supported languages: `en`, `fr`.
- Translation files: `public/i18n/{lang}.json`.
- Config in `src/app/app.config.ts`:
  - `availableLangs: ['en', 'fr']`
  - `defaultLang: 'en'`
  - `fallbackLang: 'en'`
  - `reRenderOnLangChange: true`
- Use stable, namespaced keys: `app.title`, `navbar.home`, `navbar.language`.
- Never hardcode user-facing strings in templates.
- Persist chosen language in `localStorage`, update `document.documentElement.lang`.
- Keep PrimeNG locale in sync with Transloco through a dedicated service.

### Accessibility

- Must pass all AXE checks.
- Must meet WCAG AA minimums: focus management, color contrast, ARIA attributes.

### Frontend layout

```
front/src/app/
├── app.config.ts / app.ts / app.routes.ts / app.html / app.css
├── core/
│   ├── auth/          authentication service, guards, interceptors
│   ├── i18n/          Transloco wiring, locale sync with PrimeNG
│   ├── models/        shared TypeScript contracts
│   └── theme/         PrimeNG theme + dark-mode management
├── feature/
│   ├── card-explorer / card-explorer-list / card-explorer-search / card-explorer-search-advanced
│   ├── deck-form
│   ├── home
│   ├── login
│   ├── search-show
│   ├── server-status
│   ├── workspace / workspace-deck-selected / workspace-layout
└── navbar/
```

---

## 4. Domain Knowledge

### Core concepts

- **Deck** — a named collection owned by a user, with a format `type` (e.g. `Commander`), optional `note`, a list of saved `querySearches`, and a list of `cards`.
- **DeckCard** — `{ cardName, scryFallId, quantity, type, cost, isSideBoard }`. Main deck vs. sideboard distinguished by `isSideBoard`.
- **QueryInfo / CardQuerySearch** — a persisted, structured search that can be replayed to repopulate cards. Stored as serialized JSON with a `searchEngine` tag (currently `scryfall`).
- **Scryfall** — external MTG card database; accessed through the internal gRPC Scryfall Service, never called directly from the frontend or from the `Api` project.

### Data ownership

- Deck endpoints (`/api/decks/all`, `/api/deck/{id}`, `/api/deck`, `/api/deck/{deckId}/query-search/…`) are **per-user**. Every mutation must verify the deck is owned by the current JWT subject.
- Auth is JWT Bearer. Protected requests send `Authorization: Bearer <token>`; the issuer/audience/key come from Aspire env (`Jwt__Issuer`, `Jwt__Audience`, `Jwt__Key`).

### Endpoint contracts (authoritative)

The README at `back/README` lists the full contract for `MTGArchitect.Auth.Api` and `MTGArchitect.Api`. Any change to endpoints must be reflected there, and validated with:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Check-EndpointContracts.ps1
```
(run from the repository root).

### MCP spec server

`MCP/spec-mcp` is a Node MCP server that loads MTG spec PDFs from `specs/` and exposes `list_specs` / `search_spec` tools. Use it when you need to consult rules text that isn't in the codebase.

---

## 5. Working Instructions for Claude

1. **Respect the architecture.** Before adding or moving code, identify the correct layer and consult Section 2 (backend) or Section 3 (frontend). When in doubt, "lowest valid layer wins."
2. **Prefer small, verifiable diffs.** Methods should stay short (≤ 20 lines backend; small focused components frontend). Refactor boldly only when explicitly asked.
3. **Keep endpoints thin.** Business logic goes in `*Controller` (or `*Service` for data concerns under `MTGArchitect.Data`), never in `Endpoint.cs` or gRPC `Services/`.
4. **Keep mapping isolated.** All DTO ⇄ domain ⇄ proto conversion lives in `Core/*MappingHelpers.cs` or `Scryfall.Client/MappingHelpers.cs`.
5. **Update `back/README`** whenever backend endpoints change, and run `scripts/Check-EndpointContracts.ps1` before handing back.
6. **Translate every user-facing string** you add via Transloco; add both `en` and `fr` entries in `public/i18n/`.
7. **Read environment-scoped URLs** from `environment.ts` / `environment.prod.ts` — never hardcode ports or hosts.
8. **Ask before deep styling changes** to PrimeNG components beyond Aura theme capabilities.
9. **Prefer signals** over RxJS for local component state; reach for observables when the source is already async (HTTP, events).
10. **Run the Aspire stack** (see Section 1) for integration checks; tests live in `MTGArchitect.Data.Tests` and `MTGArchitect.Scryfall.Service.Tests` on the backend, and use Vitest on the frontend (`npm test`).

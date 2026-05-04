# Plan — API Integration Tests (`MTGArchitect.Api.Integration.Tests`)

## Goal

Verify that every API endpoint works correctly end-to-end with the full Aspire stack running: real Postgres, real gRPC Scryfall service, real JWT middleware, and real HTTP routing.

---

## Decisions

| # | Topic | Decision |
|---|-------|----------|
| 1 | Infrastructure | `Aspire.Hosting.Testing 13.1.0` — spins up the full AppHost graph |
| 2 | Auth | `POST /api/auth/login` on `authapiservice` using seeded user `nordyn@hotmail.fr` / `Aqw1!` |
| 3 | Scryfall | Real `api.scryfall.com` calls accepted |
| 4 | Test isolation | Single shared `DistributedApplication` (`[SetUpFixture]`); each test is a self-contained flow (create → assert → delete) |
| 5 | Project name | `MTGArchitect.Api.Integration.Tests` |
| 6 | Coverage | All 13 endpoints get at least one happy-path test + 1 auth middleware smoke (no token → 401) |

---

## Files to Create

### `back/MTGArchitect.Api.Integration.Tests/MTGArchitect.Api.Integration.Tests.csproj`

- `Aspire.Hosting.Testing 13.1.0`
- NUnit 4.3.2 + NUnit3TestAdapter 5.0.0 + coverlet.collector 6.0.4
- `Microsoft.NET.Test.Sdk 17.14.0`
- `ProjectReference` → `MTGArchitectServices.AppHost/MTGArchitect.AppHost.csproj`
- No reference to the API project — all HTTP via `HttpClient`, responses deserialized into local records

### `IntegrationTestFixture.cs` — `[SetUpFixture]`

- `DistributedApplicationTestingBuilder.CreateAsync<Projects.MTGArchitect_AppHost>()`
- Overrides `appHost.Configuration["Jwt:Key"]` with a test-only key (≥ 32 chars for HMAC-SHA256)
- `WaitForHealthyAsync("apiservice")` + `WaitForHealthyAsync("authapiservice")`
- Calls `POST /api/auth/login` → stores `BearerToken` and `UserId` as `static` properties
- Exposes `static HttpClient ApiClient` (pre-set `Authorization: Bearer` header)
- Exposes `static HttpClient AnonClient` (no auth header — for 401 smoke tests)

### `HealthTests.cs`

| Test | Endpoint | Expected |
|------|----------|----------|
| `ServerStatus_ReturnsOk` | `GET /api/server-status` | 200 |

### `CardSearchTests.cs`

| Test | Endpoint | Expected |
|------|----------|----------|
| `QuerySearch_ReturnsOk` | `GET /api/cards/search?q=bolt` | 200, non-empty array |
| `AdvancedSearch_ReturnsOk` | `POST /api/cards/search/advanced` (empty body) | 200 |
| `GetCardDetail_WhenExists_ReturnsOk` | `GET /api/cards/{knownScryfallId}` | 200, card detail populated |
| `GetCardDetail_WhenNotFound_ReturnsNotFound` | `GET /api/cards/nonexistent-id` | 404 |

### `UserTests.cs`

| Test | Endpoint | Expected |
|------|----------|----------|
| `AuthSmoke_WithToken_ReturnsOk` | `GET /api/user/private` (with token) | 200 |
| `AuthSmoke_WithoutToken_ReturnsUnauthorized` | `GET /api/user/private` (no token) | 401 |
| `GetSettings_ReturnsOk` | `GET /api/user/settings` | 200, user object |
| `GetDecks_ReturnsOk` | `GET /api/decks/all` | 200, array |

### `DeckTests.cs` — ordered lifecycle (single `[Test]` per step, instance fields carry state)

| Test | Endpoint | Expected |
|------|----------|----------|
| `CreateDeck_ReturnsCreated` | `POST /api/deck` | 201, stores `_deckId` |
| `GetDeckById_ReturnsOk` | `GET /api/deck/{_deckId}` | 200 |
| `UpdateDeck_ReturnsOk` | `PUT /api/deck/{_deckId}` | 200, name updated |
| `AddCardToDeck_ReturnsOk` | `POST /api/deck/{_deckId}/card` | 200 |
| `AddQuerySearch_ReturnsCreated` | `POST /api/deck/{_deckId}/query-search` | 201, stores `_queryId` |
| `RemoveQuerySearch_ReturnsNoContent` | `DELETE /api/deck/{_deckId}/query-search/{_queryId}` | 204 |
| `DeleteDeck_ReturnsNoContent` | `DELETE /api/deck/{_deckId}` | 204 (cleanup) |

---

## Files to Modify

### `back/MTGArchitect.slnx`

Add to `/Tests/` folder:
```xml
<Project Path="MTGArchitect.Api.Integration.Tests/MTGArchitect.Api.Integration.Tests.csproj" />
```

---

## Key Implementation Notes

- `DeckTests` uses NUnit's natural declaration order. Instance fields `_deckId` and `_queryId` are set by earlier tests and read by later ones — tests in this class must not be run in parallel.
- Request bodies are anonymous objects serialized with `JsonContent.Create(...)`. No dependency on production contract types.
- Response DTOs are local `record` types defined in each test file (e.g. `record DeckResponse(Guid Id, string Name, string Type)`).
- The `Jwt:Key` override must be set before `appHost.BuildAsync()`. The AppHost passes it to both `authapiservice` and `apiservice` via `WithEnvironment`.
- Docker Desktop must be running — Postgres and Redis are started as containers by Aspire.
- The known Scryfall card ID used in `GetCardDetail_WhenExists_ReturnsOk` should be a stable well-known card (e.g. Lightning Bolt: `e3285e6b-3e79-4d7c-bf96-d920f973b122`).

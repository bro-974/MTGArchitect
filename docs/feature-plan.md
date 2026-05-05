# Plan: MTGArchitect.Api.Integration.Tests

## Goal
Full-stack integration tests for the `apiservice` that locally start all Aspire services and dependencies, then assert every API endpoint works end-to-end.

## Decisions

| # | Decision | Choice |
|---|----------|--------|
| 1 | Infrastructure | `Aspire.Hosting.Testing 13.1.0` — full Aspire stack (Postgres, Redis, gRPC Scryfall service, auth service, API service) |
| 2 | Auth | `POST /api/auth/login` on `authapiservice` with seeded user `nordyn@hotmail.fr` / `Aqw1!` |
| 3 | Scryfall | Real `api.scryfall.com` calls accepted |
| 4 | Test isolation | Single shared `DistributedApplication` instance (`[SetUpFixture]`), each test is a self-contained flow |
| 5 | Project name | `MTGArchitect.Api.Integration.Tests` |
| 6 | Coverage | All 13 endpoints + 1 auth middleware smoke (no token → 401) |

## Files to Create

### `back/MTGArchitect.Api.Integration.Tests/MTGArchitect.Api.Integration.Tests.csproj`
- `Aspire.Hosting.Testing 13.1.0`
- NUnit 4.3.2 + NUnit3TestAdapter + coverlet
- `ProjectReference` → `MTGArchitectServices.AppHost/MTGArchitect.AppHost.csproj`
- No reference to the API project — all HTTP via `HttpClient`, JSON via `System.Text.Json`

### `back/MTGArchitect.Api.Integration.Tests/IntegrationTestFixture.cs`
`[SetUpFixture]` — shared across all test classes:
- `DistributedApplicationTestingBuilder.CreateAsync<Projects.MTGArchitect_AppHost>()`
- Injects `Jwt:Key` via `appHost.Configuration["Jwt:Key"]`
- `WaitForHealthyAsync("apiservice")` + `WaitForHealthyAsync("authapiservice")`
- Logs in with seeded user, stores `BearerToken` and `UserId` as static properties
- Exposes `static HttpClient ApiClient` (with `Authorization: Bearer` pre-set)
- Exposes `static HttpClient AnonClient` (no header — for 401 tests)

### `back/MTGArchitect.Api.Integration.Tests/HealthTests.cs`
| Test | Endpoint | Expected |
|------|----------|---------|
| `GetServerStatus_ReturnsOk` | `GET /api/server-status` | 200 |

### `back/MTGArchitect.Api.Integration.Tests/CardSearchTests.cs`
| Test | Endpoint | Expected |
|------|----------|---------|
| `SearchCards_WithValidQuery_ReturnsOk` | `GET /api/cards/search?q=bolt` | 200, non-empty array |
| `AdvancedSearch_WithEmptyBody_ReturnsOk` | `POST /api/cards/search/advanced` | 200 |
| `GetCardDetail_WithKnownId_ReturnsOk` | `GET /api/cards/{knownScryfallId}` | 200 |
| `GetCardDetail_WithUnknownId_ReturnsNotFound` | `GET /api/cards/nonexistent-id` | 404 |

### `back/MTGArchitect.Api.Integration.Tests/UserTests.cs`
| Test | Endpoint | Expected |
|------|----------|---------|
| `GetPrivate_WithToken_ReturnsOk` | `GET /api/user/private` | 200 (auth smoke) |
| `GetPrivate_WithoutToken_ReturnsUnauthorized` | `GET /api/user/private` | 401 (auth smoke) |
| `GetSettings_ReturnsOk` | `GET /api/user/settings` | 200 |
| `GetDecks_ReturnsOk` | `GET /api/decks/all` | 200 |

### `back/MTGArchitect.Api.Integration.Tests/DeckTests.cs`
Self-contained lifecycle — each test stores state in instance fields for the next step:
| Test | Endpoint | Expected |
|------|----------|---------|
| `CreateDeck_ReturnsCreated` | `POST /api/deck` | 201, stores `_deckId` |
| `GetDeckById_ReturnsOk` | `GET /api/deck/{id}` | 200 |
| `UpdateDeck_ReturnsOk` | `PUT /api/deck/{id}` | 200 |
| `AddCardToDeck_ReturnsOk` | `POST /api/deck/{id}/card` | 200 |
| `AddQuerySearch_ReturnsCreated` | `POST /api/deck/{id}/query-search` | 201, stores `_queryId` |
| `RemoveQuerySearch_ReturnsNoContent` | `DELETE /api/deck/{id}/query-search/{queryId}` | 204 |
| `DeleteDeck_ReturnsNoContent` | `DELETE /api/deck/{id}` | 204 (cleanup) |

## Files to Modify

### `back/MTGArchitect.slnx`
Add to `/Tests/` folder:
```xml
<Project Path="MTGArchitect.Api.Integration.Tests/MTGArchitect.Api.Integration.Tests.csproj" />
```

## Key Implementation Notes
- `AnonClient` is a plain `HttpClient` with no auth header — used only for the 401 smoke tests
- `DeckTests` stores `_deckId` and `_queryId` in instance fields; NUnit runs tests in declaration order within a class
- Request bodies use anonymous objects serialized by `System.Text.Json` — no dependency on production types
- Responses deserialized into local `record` types defined inline in each test file
- `Jwt:Key` override in `appHost.Configuration` must be ≥ 32 characters for HMAC-SHA256
- Known stable Scryfall card ID for `GetCardDetail` test: Lightning Bolt `e3285e6b-3e79-4d7c-bf96-d920f973b122`

# Feature Plan

> Issue tracker for planned features. Each entry follows the PRD template.

---

## [PRD] Add card to deck from saved-query search results

**Status:** completed 
**Type:** enhancement

---

### Problem Statement

When a user selects a deck and opens a saved search query tab, they can browse card results but have no way to add a card directly to their deck from that view. They are forced to leave the workflow and manage cards elsewhere, breaking their deck-building flow.

### Solution

Add a hover overlay to each card in the saved-query search results (`SearchShow`) that reveals two action buttons: one to add the card to the **mainboard** and one to the **sideboard**. Clicking either button adds the card to the currently selected deck via a new dedicated API endpoint, with optimistic UI feedback.

### User Stories

1. As a deck builder, I want a (+) button to appear when I hover over a card in my saved search results, so that I can quickly add it to my deck's mainboard without leaving the search view.
2. As a deck builder, I want a second button to appear on hover, so that I can add a card directly to my sideboard from the search results.
3. As a deck builder, I want the add buttons to only appear on hover, so that the card grid stays clean and uncluttered when I am just browsing.
4. As a deck builder, I want clicking the mainboard button to add 1 copy of the card to my mainboard, so that I can build up my deck incrementally.
5. As a deck builder, I want clicking the sideboard button to add 1 copy of the card to my sideboard, so that I can manage my sideboard from the same view.
6. As a deck builder, I want adding a card that is already in the deck to increment its quantity by 1, so that I can easily add multiple copies without extra steps.
7. As a deck builder, I want to see a success toast notification when a card is added, so that I have immediate confirmation the action worked.
8. As a deck builder, I want the deck card list to update immediately after I click add (without waiting for a server round-trip), so that the UI feels fast and responsive.
9. As a deck builder, I want to see an error toast if adding a card fails, so that I know the action did not succeed.
10. As a deck builder, I want the deck state to be re-fetched from the server after a failed add, so that the displayed state is always accurate and consistent with the backend.
11. As a deck builder, I want the add buttons to be accessible with a keyboard and screen reader, so that the feature meets accessibility standards.
12. As a deck builder, I want the success and error messages to appear in my chosen language (English or French), so that the UI is consistent with the rest of the application.
13. As a developer, I want the add-card endpoint to accept a quantity delta rather than an absolute value, so that future bulk-add interactions (e.g. +4, +20) can reuse the same contract.

### Implementation Decisions

**New backend endpoint**
- `POST /api/deck/{deckId}/card`
- Request body: `{ scryFallId, cardName, type, cost, isSideBoard, quantity }`
- Behaviour: if a card with the same `scryFallId` and matching `isSideBoard` already exists in the deck, increment its `quantity` by the given delta; otherwise create a new entry.
- Response: `200 OK` with the updated `WorkspaceDeckCard`.
- Requires authentication; deck must be owned by the calling user.

**Frontend service — WorkspaceService**
- New `addCardToDeck(deckId, payload)` method calling the new endpoint.
- Returns an `Observable<WorkspaceDeckCard>`.

**Frontend state — WorkspaceDeckStateService**
- On optimistic add: update `selectedDeck` signal immediately by merging the new/incremented card into `selectedDeck.cards`.
- On API error: roll back to the pre-add snapshot and trigger a full deck re-fetch via `WorkspaceService`.

**SearchShow component**
- Wrap each card `<article>` in a relative-positioned container.
- Add an absolutely-positioned hover overlay with two `p-button` icon buttons (mainboard and sideboard).
- Overlay visibility controlled by CSS `:hover` — no Angular state needed.
- On click, dispatch to `WorkspaceService.addCardToDeck` with `quantity: 1` and the appropriate `isSideBoard` value.
- Show a PrimeNG `MessageService` toast on success and on error.

**i18n**
- Add translation keys for success and error toasts in both `en.json` and `fr.json`.

**Card field mapping from `CardExplorerCard` to request**
- `id → scryFallId`
- `name → cardName`
- `typeLine → type`
- `manaCost → cost`

### Testing Decisions

A good test verifies observable external behaviour visible through public APIs or rendered DOM — not private methods, signal internals, or HTTP interceptor wiring.

**Modules to test**
- **Backend endpoint**: integration tests covering (a) card created when not present, (b) quantity incremented when present, (c) 403 when deck is not owned by caller, (d) 404 when deck does not exist.
- **WorkspaceService.addCardToDeck**: unit test verifying the correct HTTP call is made with the expected payload.
- **WorkspaceDeckStateService optimistic update**: unit test verifying the signal updates immediately and reverts on rollback.
- **SearchShow component**: component test verifying (a) overlay buttons appear on hover, (b) correct service method is called with the right `isSideBoard` value per button.

Prior art: existing `WorkspaceService` tests for query-search add/remove follow the same HTTP-mock pattern.

### Out of Scope

- Bulk-add buttons (+4, +20) — endpoint supports a quantity delta but UI for this is a separate feature.
- Add-to-deck from the Card Explorer page — only the saved-query `SearchShow` tab is in scope.
- Removing or decrementing card quantity from the search results view.
- Deck card list management UI (reorder, delete, edit quantity).
- Sideboard vs mainboard default preference setting.

### Further Notes

- `WorkspaceDeckStateService.selectedDeck` is a global signal; `SearchShow` can inject it directly to read the current deck ID without prop-drilling.
- The (+) button always sends `quantity: 1` in this iteration — the endpoint's delta parameter is intentionally forward-compatible for future bulk-add use cases.





# Feature Plan: Sideboard Display & Mana Cost Column

## Problem Statement

When reviewing a deck in the Workspace, users can add cards to either the mainboard or the sideboard using the dual-button UI in search results. However, the deck view only displays mainboard cards — sideboard cards are silently stored but never shown. Users have no way to review, count, or verify their sideboard within the deck view. Additionally, the card table omits the mana cost column, forcing users to remember or look up costs separately.

## Solution

Add a sideboard display section directly below the mainboard list on the Mainboard tab, separated by a visual divider. The sideboard section appears only when the deck contains at least one sideboard card. Simultaneously, add a mana cost column (rendered as colored pastille pills) to both the mainboard and sideboard card tables. Surface a sideboard card count on the Overview tab alongside the existing mainboard count.

## User Stories

1. As a deck builder, I want to see my sideboard cards listed below the mainboard, so that I can review the full deck composition in one place.
2. As a deck builder, I want the sideboard section to be hidden when empty, so that casual and Commander decks are not cluttered with an empty sideboard area.
3. As a deck builder, I want the sideboard section headed with "Sideboard (N)", so that I can immediately see how many sideboard cards I have without counting manually.
4. As a deck builder, I want the sideboard and mainboard separated by a visual divider, so that the two zones are clearly distinguished at a glance.
5. As a deck builder, I want the sideboard table to have the same columns as the mainboard (quantity, name, cost, type), so that I can compare cards across both zones consistently.
6. As a deck builder, I want mana costs displayed as colored pills (white, blue, black, red, green, colorless) in the card table, so that I can quickly scan the mana curve without reading raw symbol strings.
7. As a deck builder, I want numeric and special mana tokens (e.g. `{2}`, `{X}`) rendered as gray colorless pills, so that the cost column is visually uniform regardless of card type.
8. As a deck builder, I want cards with no mana cost (lands, tokens) to show an empty cost cell, so that the absence of a cost is represented cleanly without a distracting placeholder.
9. As a deck builder, I want the Overview tab to show my sideboard card count alongside the mainboard count, so that I get a complete deck summary at a glance.
10. As a deck builder, I want the sideboard count on the Overview tab to appear only when I have sideboard cards, so that the overview stays uncluttered for decks without a sideboard.
11. As a deck builder, I want mana cost pills to be smaller inside the card table than elsewhere in the app, so that the table rows remain compact and scannable.
12. As a developer, I want a single `small` flag on the mana cost component to control pill size, so that all table-context pill sizes can be changed in one place.

## Implementation Decisions

### Modules to build or modify

**New — `ManaCostComponent` (shared, in core)**
- A standalone Angular component placed in the core layer, usable anywhere in the app.
- Inputs: `cost` (`string | null`, default `null`) and `small` (`boolean`, default `false`).
- Parses the cost string by extracting `{X}` tokens via regex. Renders nothing when cost is null or empty.
- Maps each token to a background color, border color, and text color using the existing mana color palette (W/U/B/R/G/C), with colorless gray as the fallback for numeric and special tokens.
- Renders each token as a `<span>` with CSS custom property bindings for color (same pattern as the advanced search color filter).
- CSS: a `small` modifier class reduces pill size for table row contexts; default size matches the existing search-form pastille.
- No external dependencies — pure CSS, no web fonts or SVG fetches.

**Modified — `WorkspaceDeckSelected`**
- Add a `sideboard` computed signal mirroring the existing `mainboard` computed signal, filtering `isSideBoard === true`.
- Mainboard tab template: add a Cost column to the mainboard table; append a sideboard section (divider + heading + table) rendered only when `sideboard().length > 0`.
- Overview tab template: add a sideboard count stat entry, rendered only when `sideboard().length > 0`.
- Import and use `ManaCostComponent` with `small=true` in both card tables.

**Modified — i18n translation files (`en.json`, `fr.json`)**
- Add keys under the existing `workspace.selected` namespace:
  - `workspace.selected.cost` — column header for mana cost
  - `workspace.selected.sideboard` — sideboard section heading (supports `{{ count }}` interpolation)
  - `workspace.selected.sideboardCount` — overview stat label for sideboard card count

### Architectural decisions

- The mana color palette (background, border, text per symbol) is defined once inside `ManaCostComponent` as a private constant map — not pulled from a service — since it is static domain data that never changes at runtime.
- The `small` boolean input controls a CSS modifier class; exact pixel sizes live only in the component's stylesheet, making a global size change a single-file edit.
- The sideboard section is conditionally rendered with Angular's native `@if` control flow, not a CSS `display:none`, so no DOM is created for empty sideboards.
- No new API endpoints or backend changes are required — the `isSideBoard` flag is already stored and returned by the backend on every `WorkspaceDeckCard`.

## Testing Decisions

### What makes a good test here

Test observable output from the component's public API — what the template renders given specific inputs — not internal parse functions or private maps. Tests should not know that a regex is used internally.

### Modules to test

**`ManaCostComponent`** — primary test target, highest value:
- `cost = null` → renders no elements
- `cost = ''` → renders no elements
- `cost = '{G}{G}'` → renders exactly 2 pill elements with green styling
- `cost = '{2}{W}'` → renders 2 pills: first with colorless gray, second with white styling
- `cost = '{X}'` → renders 1 pill with colorless gray styling
- `small = true` → host element carries the small modifier CSS class
- `small = false` (default) → small modifier class absent

**`WorkspaceDeckSelected`** — computed signal logic:
- `sideboard()` returns only cards where `isSideBoard === true`
- `mainboard()` returns only cards where `isSideBoard === false`

### Prior art

Existing component tests in the frontend use Vitest + jsdom. The `card-explorer-search-advanced` component provides a reference for how mana color options are structured and can serve as a visual reference for expected pill appearance.

## Out of Scope

- Removing cards from the sideboard (no delete action on table rows — that is a separate feature).
- Reordering cards within the sideboard or mainboard.
- Drag-and-drop between mainboard and sideboard zones.
- Mana curve visualization or CMC statistics.
- Displaying mana cost in the Card Explorer list (currently shows plain text; upgrading it to use `ManaCostComponent` is a follow-up).
- Backend changes — the `isSideBoard` field is already persisted and returned correctly.

## Further Notes

- The mana color palette values (hex codes for W/U/B/R/G/C) should match exactly what is already defined in `card-explorer-search-advanced` to ensure visual consistency across the app. Consolidation of those values into a shared constant is a worthwhile follow-up but not required for this feature.
- The `{cost}` string format coming from Scryfall (via the backend) uses curly-brace notation: `{2}{G}{G}`, `{W}{U}`, `{X}{B}{B}{B}`. The parser should handle multi-character tokens (e.g. `{15}`, `{W/U}`) gracefully — unknown tokens fall back to the colorless gray pill.


# PRD — Card Detail Side Panel (Workspace Search Tabs)

## Problem Statement

When a user is browsing card search results inside a deck's saved-query tab (the Workspace), they can see a card's image but have no way to read its oracle text, check its rulings, compare printings, or see its format legality without leaving the application. This forces deck-builders to context-switch to external tools (Scryfall, etc.) for information they need to make deckbuilding decisions.

---

## Solution

When a user clicks a card in a workspace search-result tab, a detail panel slides in from the right of the card grid. The panel displays the full card profile — image, oracle text, type, mana cost, power/toughness, rarity, set, format legalities, flavor text, artist, all available printings (with a clickable set list that swaps the displayed image), and a collapsible rulings section. The panel also exposes "Add to Mainboard" and "Add to Sideboard" actions so the user never needs to close it to act on the information.

---

## User Stories

1. As a deck-builder, I want to click a card in a search-result tab and see its oracle text, so that I can understand its rules without leaving the application.
2. As a deck-builder, I want to see a card's power, toughness, and loyalty in the detail panel, so that I can evaluate its combat relevance.
3. As a deck-builder, I want to see a card's mana cost and converted mana value in the detail panel, so that I can assess its place in my mana curve.
4. As a deck-builder, I want to see a card's rarity and set in the detail panel, so that I know how easy it is to obtain.
5. As a deck-builder, I want to see a card's format legality in the detail panel, so that I can avoid adding illegal cards to my deck.
6. As a deck-builder, I want to see a card's flavor text and artist in the detail panel, so that I can appreciate its lore and art.
7. As a deck-builder, I want a list of all printings of a card in the detail panel, so that I can choose my preferred edition.
8. As a deck-builder, I want clicking a printing in the printing list to swap the card image to that edition, so that I can visually compare arts.
9. As a deck-builder, I want the card's rulings to be available in the detail panel (collapsed by default), so that I can look them up without switching to another site.
10. As a deck-builder, I want "Add to Mainboard" and "Add to Sideboard" buttons in the detail panel, so that I can act on the information without closing the panel.
11. As a deck-builder, I want the detail panel to slide in from the right without replacing the card grid, so that I can still see the other search results.
12. As a deck-builder, I want to close the detail panel with an X button or the ESC key, so that I can reclaim the full grid width.
13. As a deck-builder, I want the detail panel to stay open when I switch between search-result tabs, so that I can compare a card against different queries.
14. As a deck-builder, I want clicking a different card to update the panel with the new card's details, so that I can browse quickly.
15. As a deck-builder, I want skeleton placeholders while the panel is loading card data, so that the layout does not jump.
16. As a mobile user, I want the detail panel to appear as a bottom sheet instead of a side panel, so that it is usable on small screens.
17. As a deck-builder, I want the panel width to be fixed and predictable, so that the card grid always has the same amount of space.
18. As a deck-builder, I want all user-facing strings in the panel to respect my language preference (EN/FR), so that the experience is consistent.

---

## Implementation Decisions

### Backend — new card-detail endpoint

- Add `GET /api/cards/{id}` to the REST API (ApiService).
- This endpoint accepts a Scryfall card UUID and returns a full card detail contract including: id, name, mana cost, CMC, type line, oracle text, power, toughness, loyalty, rarity, set code, set name, format legalities map, flavor text, artist, image URIs (normal + large), collector number.
- Printings (other editions of the same card) are fetched from a second Scryfall call using the card's `prints_search_uri` and mapped to a slim list of `{ setCode, setName, rarity, imageUrl }`.
- Rulings are fetched from the card's `rulings_uri` and returned as a list of `{ publishedAt, comment }`.
- The gRPC proto (`greet.proto`) must be extended with a new `GetCardDetail` RPC method and corresponding request/reply messages covering all the above fields.
- The gRPC server (`GreeterService`) maps Scryfall REST JSON to the new proto reply.
- The gRPC client (`ScryfallClient`) exposes a corresponding `GetCardDetail` method to the ApiService.
- The `CardItem` model is not changed — it remains the search-result contract.
- A new `CardDetailResponse` contract is defined for this endpoint only.

### Frontend — CardDetailPanelComponent

- A new standalone component `CardDetailPanel` is created under `feature/card-detail-panel/`.
- It receives the Scryfall card ID as an `input()` signal and manages its own data-fetch lifecycle.
- A new `CardDetailService` (singleton, `providedIn: 'root'`) calls `GET /api/cards/{id}` and caches the last N results to avoid redundant fetches during a browsing session.
- The panel width is fixed at 380 px on desktop via a host CSS variable; below the `md` breakpoint it renders as a full-width bottom sheet using a PrimeNG Drawer.
- The panel contains: large card image, name, mana cost (reusing the existing `ManaCostComponent`), type line, oracle text, P/T / loyalty, rarity chip, set name + set code, format legality badges, flavor text (italic), artist credit, a scrollable printing list (each item is a clickable set chip that emits a `printingSelected` event to swap the image URL signal), and a PrimeNG Accordion for rulings.
- Skeleton layout uses PrimeNG Skeleton components matching the panel structure.
- "Add to Mainboard" and "Add to Sideboard" buttons in the panel emit the same `addCard` output as the existing hover overlay in `SearchShow`.
- `SearchShow` is updated to: (a) wire a click handler on each card article that sets the active card ID signal, and (b) render `CardDetailPanel` beside the grid inside a flex row container. The panel is conditionally rendered with `@if`.
- Panel persistence across tabs: the active card ID signal lives in a parent component or a shared service so switching tabs does not reset it.
- The ESC key closes the panel via the `host` keyboard event binding (not `@HostListener`).
- All user-facing strings are added to `public/i18n/en.json` and `public/i18n/fr.json` under a `cardDetail.*` namespace.
- `ChangeDetectionStrategy.OnPush` on all new components.

### API contract addition

```
GET /api/cards/{id}
Response 200: CardDetailResponse {
  id, name, manaCost, cmc, typeLine, oracleText,
  power, toughness, loyalty,
  rarity, setCode, setName,
  legalities: { [format]: "legal" | "not_legal" | "banned" | "restricted" },
  flavorText, artist,
  imageUrl, imageLargeUrl,
  printings: [{ setCode, setName, rarity, imageUrl }],
  rulings: [{ publishedAt, comment }]
}
Response 404: card not found
```

---

## Testing Decisions

**What makes a good test here:** test the observable contract of each module — what goes in, what comes out — not how it is implemented internally. For services, test the HTTP calls and data mapping. For components, test rendered output and emitted events in response to inputs, not internal signal values.

### Modules to test

| Module | What to test |
|---|---|
| `CardDetailService` | Given a valid Scryfall ID, expect the correct `GET /api/cards/{id}` call and a mapped `CardDetailResponse`; given a 404, expect a handled error state. |
| `CardDetailPanelComponent` | Given a loaded card input, expect key fields (name, type, oracle text) to appear in the DOM; expect `addCard` output to emit when the add buttons are clicked; expect the printing list to swap the image signal when a set chip is clicked; expect the panel to emit close when ESC is pressed. |
| `SearchShow` (updated) | Expect clicking a card to set the active card ID and render the panel; expect the panel not to render when no card is selected. |

**Prior art:** existing component tests in `card-explorer-list` and service tests in `card-explorer.service` follow the same Vitest + jsdom pattern and serve as references.

---

## Out of Scope

- Card detail panel in the standalone Card Explorer pages (only Workspace search tabs are in scope for this PRD).
- Editable card quantity from the panel.
- Card price display (USD/EUR/TIX).
- Wishlist or collection tracking from the panel.
- Persistent cross-session panel state (last opened card is not remembered on page reload).
- Offline / cached card data in the database.
- Keyboard navigation within the printing list.

---

## Further Notes

- Scryfall's public API is rate-limited to ~10 req/s. The backend should add a brief per-request delay or use a Polly rate-limiting policy to stay within limits, especially since each panel open may trigger two Scryfall calls (card detail + rulings).
- The `prints_search_uri` returned by Scryfall for a card detail can return a large number of results for popular cards (e.g., Lightning Bolt). Consider capping the printing list at 20 entries sorted by release date descending.
- Rulings are often empty for most cards — the accordion section should be hidden entirely when the rulings list is empty.
- The `CardDetailResponse` legalities map uses format strings that match the existing `CardFormat` enum on the frontend — no new enum values needed.
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

# Feature Plan: Color Identity & Commander in Deck API

## Context

The `DeckForm` UI already sends `colorIdentity` (e.g. `"WUB"`) and `commander` (e.g. `"Atraxa, Praetors' Voice"`) to the API. The backend currently ignores both fields. This plan wires them end-to-end.

## Decisions

- **Storage format**: `string?` — matches the frontend representation, nullable for existing decks.
- **Validation**: `colorIdentity` must match `^[WUBRGC]*$`; invalid values → `400 Bad Request`.
- **Scope**: both `colorIdentity` and `commander` added together (same request/response shape, same DB change).
- **DB migrations**: none — DB is recreated on each dev startup via `EnsureCreated`.
- **Frontend read side**: add both fields to `WorkspaceDeck` and display them in the overview tab.

## Checklist

### Backend

- [x] `back/MTGArchitect.Data/Models/Deck.cs` — add `Commander: string?` and `ColorIdentity: string?`
- [x] `back/MTGArchitect.Data/Data/AuthDbContext.cs` — configure both columns in `OnModelCreating` (max lengths: Commander 200, ColorIdentity 6)
- [x] `back/MTGArchitectServices.ApiService/Core/ApiContracts.cs` — add both fields to `DeckUpsertRequest` and `DeckResponse`
- [x] `back/MTGArchitectServices.ApiService/Core/MappingHelpers.cs` — map both fields in `ToDeckResponse`, `ToDeck`, `Apply`
- [x] `back/MTGArchitectServices.ApiService/Services/DeckService.cs` — validate `colorIdentity` against `^[WUBRGC]*$`, return `400` if invalid

### Frontend

- [x] `front/src/app/feature/workspace/workspace.models.ts` — add `commander: string | null` and `colorIdentity: string | null` to `WorkspaceDeck`
- [x] `front/src/app/feature/workspace-deck-selected/workspace-deck-selected.ts` — add computed to convert `"WUB"` → `"{W}{U}{B}"` for `ManaCostComponent`
- [x] `front/src/app/feature/workspace-deck-selected/workspace-deck-selected.html` — display commander name and color pips in the overview tab
- [x] `front/src/assets/i18n/en.json` and `fr.json` — add translation keys for commander and color identity labels

### Docs

- [x] `docs/api.endpoint.md` — update `DeckResponse` and `POST /api/deck` / `PUT /api/deck/{id}` request body contracts

# Feature Plan: Pagination for Workspace Search Results

## Goal
Add server-side page navigation to the `search-show` component so users can browse beyond the first 30 results of a saved search query.

## Decisions
| Decision | Choice |
|---|---|
| Strategy | Server-side pages |
| UI | PrimeNG `<p-paginator>` — `<<` `<` `{start}–{end} of {total}` `>` `>>` |
| Page size | Hardcoded 30 |
| Paginator position | Bottom of card grid |

## Checklist

### Backend — Scryfall Service
- [x] `SearchCardsResult` record: add `TotalCount: int`
- [x] `greet.proto`: add `int32 total_count` to `SearchCardsReply`; add `int32 page` to the advanced search request message
- [x] `CardController.cs`: accept `page` param, pass it to Scryfall (1-based), capture `total_cards` from Scryfall response, populate `TotalCount`
- [x] `MappingHelpers.cs` (scryfall service): map `TotalCount` in `ToReply`

### Backend — API Service
- [x] gRPC client call: pass `page`, read `TotalCount` from reply
- [x] Change response from flat `CardExplorerCard[]` to `{ cards: CardExplorerCard[], totalCount: int }`

### Frontend
- [x] Add `CardExplorerSearchResult { cards: CardExplorerCard[], totalCount: number }` type
- [x] `CardExplorerService.searchCardsAdvanced()`: accept `page` argument, return `CardExplorerSearchResult`
- [x] `search-show.ts`: add `currentPage` signal (0-based), add `totalCount` signal, re-fetch on page change, reset to page 0 when `queryText` input changes
- [x] `search-show.html`: add `<p-paginator>` at bottom with custom first/prev/next/last icon template, `[rows]="30"`, `[totalRecords]="totalCount()"`

# Feature Plans

## Card Hover Preview (workspace-deck-selected)

**Goal:** When hovering a card row in the mainboard/sideboard table, display the card image next to the cursor.

| Decision | Choice |
|---|---|
| Image source | Scryfall CDN: `cards.scryfall.io/normal/front/{id[0]}/{id[1]}/{id}.jpg` |
| Architecture | Standalone Angular directive (`CardHoverPreviewDirective`) |
| Overlay | Single `<img>` appended to `document.body`, `position: fixed` |
| Positioning | Follows mouse via `transform: translate(x, y)`, offset 16px right + 8px up |
| Edge detection | Flip left if cursor within 250px of right edge; flip up if within 320px of bottom |
| Hover delay | 150ms `setTimeout`, cancelled on `mouseleave` |
| Image display size | 220×307px (from Scryfall `normal` source) |
| Scope | `workspace-card-table__row` divs only (mainboard + sideboard sections) |

**Files:**
- New: `front/src/app/core/directives/card-hover-preview.directive.ts`
- Modify: `front/src/app/feature/workspace-deck-selected/workspace-deck-selected.html`
- Modify: `front/src/app/feature/workspace-deck-selected/workspace-deck-selected.ts`

# Feature Plans

## Card Hover Preview (workspace-deck-selected)

**Goal:** When hovering a card row in the mainboard/sideboard table, display the card image next to the cursor.

| Decision | Choice |
|---|---|
| Image source | Scryfall CDN: `cards.scryfall.io/normal/front/{id[0]}/{id[1]}/{id}.jpg` |
| Architecture | Standalone Angular directive (`CardHoverPreviewDirective`) |
| Overlay | Single `<img>` appended to `document.body`, `position: fixed` |
| Positioning | Follows mouse via `transform: translate(x, y)`, offset 16px right + 8px up |
| Edge detection | Flip left if cursor within 250px of right edge; flip up if within 320px of bottom |
| Hover delay | 150ms `setTimeout`, cancelled on `mouseleave` |
| Image display size | 320×446px (from Scryfall `normal` source) |
| Scope | `workspace-card-table__row` divs only (mainboard + sideboard sections) |

**Files:**
- New: `front/src/app/core/directives/card-hover-preview.directive.ts`
- Modify: `front/src/app/feature/workspace-deck-selected/workspace-deck-selected.html`
- Modify: `front/src/app/feature/workspace-deck-selected/workspace-deck-selected.ts`

---

## Mana Symbol SVG (mana-cost display)

**Goal:** Replace colored pill spans with Scryfall SVG mana symbols in the mana-cost component.

| Decision | Choice |
|---|---|
| Symbol source | Scryfall CDN, constructed directly: `svgs.scryfall.io/card-symbols/{symbol}.svg` |
| Render method | `<img>` tag per token |
| Transform | `token.slice(1, -1).replace('/', '').toUpperCase()` + `.svg` |
| Component | Update `ManaCostComponent` in place — interface unchanged |
| Null/empty cost | Already guarded, renders nothing |
| card-explorer-list | Replace `{{ card.manaCost }}` with `<app-mana-cost [cost]="card.manaCost" />` |

**Files:**
- Modify: `front/src/app/core/components/mana-cost/mana-cost.component.ts`
- Modify: `front/src/app/core/components/mana-cost/mana-cost.component.html`
- Modify: `front/src/app/core/components/mana-cost/mana-cost.component.css`
- Modify: `front/src/app/feature/card-explorer-list/card-explorer-list.html`
- Modify: `front/src/app/feature/card-explorer-list/card-explorer-list.ts`

---

## Mana Symbol Toggle Buttons (search forms)

**Goal:** Replace colored pill toggle buttons in color/color-identity filters with Scryfall SVG mana symbols.

| Decision | Choice |
|---|---|
| Symbol render | `<img>` from `svgs.scryfall.io/card-symbols/{X}.svg` |
| Selected state | Glowing ring (`box-shadow`) + scale-up, no checkmark |
| Button background | Transparent — SVG is self-colored |
| CSS cleanup | Remove label, check, CSS vars, background/border/color fields entirely |
| TS cleanup | Remove `background`, `border`, `color` from `ManaColorOption` in both components |
| CSS duplication | Keep per-component (sizes differ: 2.5rem vs 2.25rem) |

**Files:**
- Modify: `front/src/app/feature/card-explorer-search-advanced/card-explorer-search-advanced.ts`
- Modify: `front/src/app/feature/card-explorer-search-advanced/card-explorer-search-advanced.html`
- Modify: `front/src/app/feature/card-explorer-search-advanced/card-explorer-search-advanced.css`
- Modify: `front/src/app/feature/workspace/workspace-search/workspace-search-form.ts`
- Modify: `front/src/app/feature/workspace/workspace-search/workspace-search-form.html`
- Modify: `front/src/app/feature/workspace/workspace-search/workspace-search-form.css`

# AI Client Refactor — Clean Architecture

## Decisions

| Question | Decision |
|---|---|
| Contract types location | `MTGArchitect.AI.Contract` (existing, netstandard2.1) |
| Service class name in ApiService | `MindServices` |
| HTTP endpoint | `GET /api/ai/chat?prompt=...` |
| Streaming format | SSE — `data: {"content":"...","type":"Answer\|Reasoning\|Metadata"}` |
| Authentication | Required (JWT) |
| Endpoint file | New `AiEndpoint.cs` |

## Files

### MTGArchitect.AI.Contract/ChatChunk.cs — new
```csharp
public record ChatChunk(string Content, ChunkType Type);
public enum ChunkType { Reasoning, Answer, Metadata }
```

### MTGArchitect.AI.Client/Services/MindClient.cs — new
- `IMindClient` interface: `IAsyncEnumerable<ChatChunk> StreamChatAsync(string prompt, CancellationToken ct)`
- `MindClient` implementation wrapping proto `MindService.MindServiceClient`
- Maps `ReasoningChunk` → `ChunkType.Reasoning`, `AnswerChunk` → `ChunkType.Answer`

### MTGArchitect.AI.Client/Module.cs — update
- Add `services.AddScoped<IMindClient, MindClient>()`

### MTGArchitectServices.ApiService/Services/MindServices.cs — new
- Injects `IMindClient`
- Writes SSE to `HttpResponse` from `IAsyncEnumerable<ChatChunk>`

### MTGArchitectServices.ApiService/AiEndpoint.cs — new
- `MapAiChat` extension on `WebApplication`
- `GET /api/ai/chat?prompt=...`, `RequireAuthorization()`, `text/event-stream`

### MTGArchitectServices.ApiService/Endpoint.cs — update
- Call `app.MapAiChat(api)` inside `MapApiEndpoints`

### MTGArchitectServices.ApiService/Program.cs — update
- Add `builder.Services.AddScoped<MindServices>()`
- Remove `/test-ai` anonymous endpoint

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

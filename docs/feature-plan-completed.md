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

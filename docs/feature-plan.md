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

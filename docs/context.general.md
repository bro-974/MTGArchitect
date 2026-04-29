# MTGArchitect — General Context

## What the app does

MTGArchitect is a Magic: The Gathering deck-building tool. Users can:
- Search cards (simple text or advanced multi-field filters via Scryfall)
- Create and manage decks (with format, commander, color identity, notes)
- Save search queries to a deck for reuse
- Browse a deck's mainboard card list

## Key feature areas

### Card Explorer
Standalone search page. Two modes:
- Simple: debounced text search → `GET /api/cards/search`
- Advanced: multi-field form → `POST /api/cards/search/advanced`

Results shown in a responsive grid (`CardExplorerList`). Cards display image, name, type line, mana cost, set code.
Model: `CardExplorerCard { id, name, manaCost, typeLine, setCode, imageUrl }`

### Workspace
The main deck-management area, built from a vertical splitter layout:
- **Left panel** — deck list (tree grouped by format) + advanced search form
- **Right panel** — selected deck view (tabbed: Overview, Mainboard, + saved-query tabs)
- **Bottom panel** — placeholder (not yet used)

Selecting a deck sets `WorkspaceDeckStateService.selectedDeck` (a signal, globally accessible).

### Deck data model
```
WorkspaceDeck {
  id, name, type, note,
  querySearches: WorkspaceQuerySearch[],  // saved search queries
  cards: WorkspaceDeckCard[]
}

WorkspaceDeckCard {
  id, cardName, scryFallId, quantity, type, cost, isSideBoard
}

WorkspaceQuerySearch { id, query, searchEngine }
```

Upsert variants exist for all three (`WorkspaceDeckUpsert`, `WorkspaceDeckCardUpsert`, `WorkspaceQuerySearchUpsert`).

### WorkspaceService — known endpoints
| Verb | Path | Purpose |
|------|------|---------|
| GET  | `/api/decks/all` | fetch all decks |
| POST | `/api/deck` | create deck |
| PUT  | `/api/deck/{id}` | update deck |
| POST | `/api/deck/{id}/query-search` | save a query to deck |
| DELETE | `/api/deck/{id}/query-search/{queryId}` | remove saved query |

**No card-add endpoint is wired up yet on the frontend.**

### SearchShow
Component rendered inside a deck's saved-query tab. Lazy-loads results when the tab is active. Currently read-only — displays cards but has no "add to deck" action.

## Tech stack reminder
Angular 21 · signals · PrimeNG 21 (Aura) · Transloco (en/fr) · OnPush everywhere · standalone components · `input()`/`output()` functions · no `ngClass`/`ngStyle`/`*ngIf`/`*ngFor`

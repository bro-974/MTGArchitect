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

# Backend Endpoint Contracts

This section describes the HTTP contracts for:
- `MTGArchitect.Auth.Api`
- `MTGArchitect.Api`

For protected routes, send:
- `Authorization: Bearer <jwt-token>`

---

## MTGArchitect.Auth.Api

### Public endpoints

#### `GET /`
- Description: Health text endpoint for auth service.
- Response: `"Auth API service is running."`

#### `POST /api/auth/register`
- Description: Register a new user.
- Request body:
```json
{
  "email": "user@email.com",
  "password": "string"
}
```
- Success response: `201 Created`
```json
{
  "id": "user-id",
  "email": "user@email.com"
}
```

#### `POST /api/auth/login`
- Description: Authenticate a user and return JWT token.
- Request body:
```json
{
  "email": "user@email.com",
  "password": "string"
}
```
- Success response: `200 OK`
```json
{
  "accessToken": "jwt-token",
  "expiresAtUtc": "2026-01-01T00:00:00Z",
  "userId": "user-id",
  "email": "user@email.com"
}
```

### Endpoints requiring authentication

#### `GET /api/auth/me`
- Description: Return current authenticated user identity from JWT claims.
- Response: `200 OK`
```json
{
  "userId": "user-id",
  "email": "user@email.com"
}
```

#### `POST /api/auth/logout`
- Description: Logout current authenticated user.
- Response: `204 No Content`

---

## MTGArchitect.Api

### Public endpoints

#### `GET /`
- Description: Health text endpoint for API service.
- Response: `"API service is running."`

#### `GET /api/server-status`
- Description: Server health status.
- Response:
```json
{
  "status": "Healthy",
  "checkedAt": "2026-01-01T00:00:00+00:00"
}
```

#### `GET /api/cards/search?q={query}&pageSize={optional}`
- Description: Search cards with simple query.
- Query params:
  - `q` (required)
  - `pageSize` (optional)
- Response: Scryfall search payload.

#### `POST /api/cards/search/advanced`
- Description: Advanced search with structured filters.
- Request body: `CardQuerySearch` contract.
- Response: Scryfall advanced search payload.

### Endpoints requiring authentication

#### `GET /api/user/private`
- Description: Check if JWT is valid for current user.
- Response: `200 OK`

#### `GET /api/user/settings`
- Description: Return current user settings.
- Response:
```json
{
  "displayName": "Tester",
  "language": "fr",
  "theme": "dark"
}
```

#### `GET /api/decks/all`
- Description: Return all decks of logged user.
- Response:
```json
[
  {
    "id": "deck-guid",
    "name": "My Elf Deck",
    "type": "Commander",
    "note": "string",
    "querySearches": [
      {
        "id": "query-guid",
        "query": "serialized CardQuerySearch JSON",
        "searchEngine": "scryfall"
      }
    ],
    "cards": [
      {
        "id": "card-guid",
        "cardName": "Llanowar Elves",
        "scryFallId": "external-id",
        "quantity": 2,
        "type": "Creature",
        "cost": "G",
        "isSideBoard": false
      }
    ]
  }
]
```

#### `GET /api/deck/{id}`
- Description: Return one deck by id (only if owned by logged user).
- Response: `DeckResponse`

#### `POST /api/deck`
- Description: Create a new deck for logged user.
- Request body:
```json
{
  "name": "My Elf Deck",
  "type": "Commander",
  "note": "my deck",
  "querySearches": [
    {
      "id": null,
      "queryJson": "{\"filters\": {...}, \"limits\": {...}}",
      "searchEngine": "scryfall"
    }
  ],
  "cards": [
    {
      "cardName": "Llanowar Elves",
      "scryFallId": "external-id",
      "quantity": 1,
      "type": "Creature",
      "cost": "G",
      "isSideBoard": false
    }
  ]
}
```
- Response: `201 Created` with `DeckResponse`

#### `PUT /api/deck/{id}`
- Description: Update an existing deck (owned by logged user).
- Request body: same as `POST /api/deck`
- Response: `200 OK` with updated `DeckResponse`

#### `DELETE /api/deck/{id}`
- Description: Delete a deck (owned by logged user).
- Response: `204 No Content`

#### `POST /api/deck/{deckId}/query-search`
- Description: Add a query search to a deck (owned by logged user).
- Request body:
```json
{
  "id": null,
  "queryJson": "{\"filters\": {...}, \"limits\": {...}}",
  "searchEngine": "scryfall"
}
```
- Response: `201 Created` with `QueryInfoResponse`

#### `DELETE /api/deck/{deckId}/query-search/{queryId}`
- Description: Remove a query search from a deck (owned by logged user).
- Response: `204 No Content`

## Documentation validation skill

Run this command from repository root to validate README endpoint contracts against source endpoints:

`powershell -ExecutionPolicy Bypass -File ..\back\scripts\Check-EndpointContracts.ps1`

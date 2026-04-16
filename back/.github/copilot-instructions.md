# Copilot Instructions

## Project Guidelines
- Prefer thin endpoint handlers with minimal logic; move business logic into dedicated services (e.g., UserService, DeckService) and centralize mapping in /Core/MappingHelpers static helpers.
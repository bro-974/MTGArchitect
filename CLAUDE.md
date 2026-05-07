# Monorepo – Global Rules
## 1. Context files – load when relevant

| Condition | File to read |
| Working in /front or Angular code | `front/angular.claude.md` |
| Working in /back or .NET code | `back/dotnet.claude.md` |
| Referencing or creating API endpoints | `docs/api.endpoint.md` |
| Referencing DB models or schemas | `docs/db.schema.md` |

Always read the relevant file(s) before answering tasks in that domain.

## 2. High-Level Architecture

MTGArchitect is a distributed application orchestrated by **.NET Aspire**.
/front --> Angular 21 front interface
/back  --> Dotnet core 10 back end services
/docs  --> document file about the project

### Runtime services (Aspire)

- `cache` — Redis
- `postgres` / `authdb` — Postgres database
- `scryfallservice` — gRPC
- `authapiservice` — REST, depends on `authdb`
- `apiservice` — REST, depends on `scryfallservice` + `authapiservice` + `authdb`, exposed on port **4300**
- `aiservice` — REST, depends on `lmstudio`, exposed on port **5000**
- `lmstudio` — local LLM inference server (LM Studio), used by `aiservice`
- `frontend` — `npm start` from `../../front`, port **4201**

Start the whole stack from the repo root with:
```
dotnet run --project .\back\MTGArchitectServices.AppHost\MTGArchitectServices.AppHost.csproj
```

## graphify

This project has a graphify knowledge graph at graphify-out/.

Rules:
- Before answering architecture or codebase questions, read graphify-out/GRAPH_REPORT.md for god nodes and community structure
- If graphify-out/wiki/index.md exists, navigate it instead of reading raw files
- For cross-module "how does X relate to Y" questions, prefer `graphify query "<question>"`, `graphify path "<A>" "<B>"`, or `graphify explain "<concept>"` over grep — these traverse the graph's EXTRACTED + INFERRED edges instead of scanning files
- After modifying code files in this session, run `graphify update .` to keep the graph current (AST-only, no API cost)

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

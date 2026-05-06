# Plan: AI Chat Panel — Workspace Bottom Area

## Context

The workspace layout has a `p-splitter` with a bottom panel currently showing a `TODO` comment. This plan replaces that placeholder with a full AI chat feature that streams responses from `GET /api/ai/chat?prompt={prompt}` via SSE. The backend emits three chunk types: `Reasoning` (thinking tokens), `Answer` (final answer), and `Metadata` (reserved). The UI gives the user a chat interface, a collapsible "Thinking" panel, and a "Coming soon" Metadata placeholder. History is in-memory only (API persistence planned for a future iteration).

---

## Design Decisions (brainstorm output)

| Decision | Choice |
|---|---|
| Overall layout | Chat column (left) + right accordion column, separated by horizontal `p-splitter` |
| Chat style | Terminal/doc — prompt label + answer block (not chat bubbles) |
| Streaming | Progressive rendering with `▌` cursor, chunk-by-chunk |
| Thinking accordion | Closed by default; spinner in header during stream; live chunks if user opens it |
| Metadata accordion | "Coming soon" placeholder (future: MTG card suggestions from AI) |
| Chat/right split | `p-splitter` horizontal, `[70, 30]` default, resizable |
| Input | `p-textarea`, Enter sends, Shift+Enter adds newline |
| During streaming | Input + send button disabled; no stop button |
| History | In-memory `signal<ChatMessage[]>()` only |

---

## File Structure

```
front/src/app/feature/ai-chat/
├── ai-chat.ts                  ← root shell component (hosts the splitter)
├── ai-chat.html
├── ai-chat.css
├── ai-chat.service.ts          ← SSE streaming service
├── ai-chat-messages/
│   ├── ai-chat-messages.ts     ← scrollable message history + streaming cursor
│   ├── ai-chat-messages.html
│   └── ai-chat-messages.css
├── ai-chat-input/
│   ├── ai-chat-input.ts        ← textarea + send button, disabled when streaming
│   ├── ai-chat-input.html
│   └── ai-chat-input.css
└── ai-chat-panel/
    ├── ai-chat-panel.ts        ← right column: thinking + metadata accordions
    ├── ai-chat-panel.html
    └── ai-chat-panel.css
```

**Integration point:** `front/src/app/feature/workspace-layout/workspace-layout.html` — replace `TODO` comment with `<app-ai-chat>`.

---

## Data Model

```typescript
// ai-chat.service.ts
interface AiChunk {
  content: string;
  type: 'Reasoning' | 'Answer' | 'Metadata';
}

// ai-chat.ts (or a shared models file)
interface ChatMessage {
  prompt: string;
  answerChunks: string[];      // accumulated Answer chunks
  reasoningChunks: string[];   // accumulated Reasoning chunks
  metadataChunks: string[];    // accumulated Metadata chunks
  isStreaming: boolean;
}
```

---

## SSE Service (`ai-chat.service.ts`)

`EventSource` cannot set custom headers, so use `fetch` with `ReadableStream` + `AbortController`.

```typescript
// Pseudocode
sendPrompt(prompt: string, callbacks: {
  onAnswer: (chunk: string) => void;
  onReasoning: (chunk: string) => void;
  onDone: () => void;
  onError: (err: Error) => void;
}): AbortController {
  const ctrl = new AbortController();
  const token = this.authService.token();  // from AuthService signal

  fetch(`${apiBase}/api/ai/chat?prompt=${encodeURIComponent(prompt)}`, {
    headers: { Authorization: `Bearer ${token}` },
    signal: ctrl.signal,
  }).then(res => {
    const reader = res.body!.getReader();
    // read loop: decode lines, parse "data: {...}" SSE format
    // dispatch to callbacks by chunk.type
    // call onDone when reader.done === true
  });

  return ctrl;  // caller stores it to cancel if needed
}
```

The service is `providedIn: 'root'` and injects `AuthService` and reads `environment.apiUrl` for the base URL.

---

## Component Responsibilities

### `ai-chat.ts` (root shell)
- Owns `messages = signal<ChatMessage[]>([])`
- Owns `isStreaming = signal(false)`
- Owns current `AbortController | null`
- On `sendPrompt(prompt)`: push new `ChatMessage`, call service, update signals as chunks arrive, set `isStreaming(false)` on done
- Template: `p-splitter [panelSizes]="[70, 30]"` — left: `<app-ai-chat-messages>` + `<app-ai-chat-input>`, right: `<app-ai-chat-panel>`

### `ai-chat-messages.ts`
- `input()` — `messages: ChatMessage[]`, `isStreaming: boolean`
- Scrollable area (CSS `overflow-y: auto`) with `@for` over messages
- Each message: prompt label + answer text (last streaming message shows `▌` cursor appended)
- Auto-scrolls to bottom on new chunk (use `ViewChild` + `effect()`)

### `ai-chat-input.ts`
- `input()` — `disabled: boolean` (true when streaming)
- `output()` — `send = output<string>()`
- `p-textarea` with auto-grow, `(keydown)` handler: Enter → emit send, Shift+Enter → allow newline
- `p-button` Send, also disabled when streaming

### `ai-chat-panel.ts`
- `input()` — `reasoningChunks: string[]`, `isStreaming: boolean`
- `p-accordion` with two panels:
  1. **Thinking** — header shows `p-progressspinner` (small) when `isStreaming`, content renders `reasoningChunks` joined
  2. **Metadata** — header "Metadata", content shows "Coming soon" text

---

## Critical Files to Modify

| File | Change |
|---|---|
| [workspace-layout.html](front/src/app/feature/workspace-layout/workspace-layout.html) | Replace `TODO` comment with `<app-ai-chat>` |
| [workspace-layout.ts](front/src/app/feature/workspace-layout/workspace-layout.ts) | Import `AiChatComponent` |

---

## Reuse

- `p-splitter` — already used in workspace-layout, same pattern
- `p-accordion` / `p-accordion-panel` / `p-accordion-header` / `p-accordion-content` — used in workspace.html and card-detail-panel.html
- `p-textarea` — used in deck-form.html
- `p-button` — used everywhere
- `p-progressspinner` — used in search-show.html
- `AuthService.token()` signal — read directly in `ai-chat.service.ts` (same pattern as `auth-bearer.interceptor.ts`)
- `ChangeDetectionStrategy.OnPush` + `signal()` + `inject()` — standard project pattern

---

## Verification

1. Start the Aspire stack: `dotnet run --project .\back\MTGArchitectServices.AppHost\MTGArchitectServices.AppHost.csproj`
2. Open `http://localhost:4201`, log in, navigate to workspace
3. Bottom panel should show the AI chat area
4. Type a prompt → answer streams in progressively with `▌` cursor
5. Open "Thinking" accordion during streaming → reasoning chunks appear live
6. After stream ends → `▌` cursor disappears, input re-enables, Thinking accordion shows full reasoning
7. Metadata accordion shows "Coming soon"
8. Send a second prompt → new block appears below, history accumulates in-memory
9. Refresh page → history is cleared (in-memory only)

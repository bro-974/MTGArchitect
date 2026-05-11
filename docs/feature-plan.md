# Feature Plan: Chat Session List in Workspace Accordion

## Goal
Move the AI chat session list from the dedicated `ai-chat-sidebar` into the left workspace accordion (after "Advanced Search"). Introduce a shared `AiChatStateService` so both the accordion panel and the chat message area read from a common source.

## Decisions
| # | Decision |
|---|----------|
| State sharing | New `AiChatStateService` (`providedIn: 'root'`) holds `sessions`, `activeSession`, `completedMessages` |
| Streaming state | Stays local in `ai-chat.ts` (ephemeral, not shared) |
| `completedMessages` | Moves into `AiChatStateService`; loaded on session select, appended after stream |
| Session list component | `AiChatSidebar` → renamed to `AiChatSessionList`, self-contained (injects services) |
| Display | `p-tree` matching `WorkspaceDeckList` pattern (flat nodes now, folder-ready) |
| "New Chat" button | Inside `AiChatSessionList` header, same layout as deck list |
| Accordion panel | Third panel after "Advanced Search", disabled when no deck selected |
| i18n | Transloco keys under `workspace.accordion.chatSessions.*` |

## Files Changed
| File | Change |
|------|--------|
| `front/src/app/feature/ai-chat/ai-chat-state.service.ts` | NEW — shared session + message state |
| `front/src/app/feature/ai-chat/ai-chat-session-list/` | NEW — renamed + refactored from ai-chat-sidebar |
| `front/src/app/feature/workspace/workspace.ts` | Add `AiChatSessionList`, `isChatDisabled` computed |
| `front/src/app/feature/workspace/workspace.html` | Add third accordion panel |
| `front/src/app/feature/ai-chat/ai-chat.ts` | Inject `AiChatStateService`, remove session signals |
| `front/src/app/feature/ai-chat/ai-chat.html` | Remove sidebar, simplified layout |
| `front/src/app/feature/ai-chat/ai-chat.css` | Remove sidebar styles |
| `front/public/i18n/en.json` | Add `chatSessions` keys |
| `front/public/i18n/fr.json` | Add `chatSessions` keys |
| `front/src/app/feature/ai-chat/ai-chat-sidebar/` | DELETED |

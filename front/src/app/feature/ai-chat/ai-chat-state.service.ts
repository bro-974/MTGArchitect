import { effect, inject, Injectable, signal } from '@angular/core';
import { CompletedMessage, ChatSession } from './ai-chat.models';
import { ChatSessionService } from './chat-session.service';
import { WorkspaceDeckStateService } from '../workspace/workspace-deck-state.service';

@Injectable({ providedIn: 'root' })
export class AiChatStateService {
  private readonly chatSessionService = inject(ChatSessionService);
  private readonly deckState = inject(WorkspaceDeckStateService);

  readonly sessions = signal<readonly ChatSession[]>([]);
  readonly activeSession = signal<ChatSession | null>(null);
  readonly completedMessages = signal<readonly CompletedMessage[]>([]);

  constructor() {
    effect(() => {
      const deck = this.deckState.selectedDeck();
      if (deck) {
        this.loadSessions(deck.id);
      } else {
        this.sessions.set([]);
        this.activeSession.set(null);
        this.completedMessages.set([]);
      }
    });
  }

  private loadSessions(deckId: string): void {
    this.chatSessionService.getSessions(deckId).subscribe((sessions) => {
      this.sessions.set(sessions);
      const current = this.activeSession();
      const stillExists = current ? sessions.some((s) => s.id === current.id) : false;
      if (!stillExists) {
        const first = sessions[0] ?? null;
        this.activeSession.set(first);
        this.completedMessages.set([]);
        if (first) {
          this.loadMessages(first.id);
        }
      }
    });
  }

  private loadMessages(sessionId: string): void {
    this.chatSessionService.getSessionMessages(sessionId).subscribe((messages) => {
      this.completedMessages.set(
        messages.map((m) => ({ prompt: m.userPrompt, answer: m.answer, reasoning: '' }))
      );
    });
  }

  selectSession(session: ChatSession): void {
    this.activeSession.set(session);
    this.completedMessages.set([]);
    this.loadMessages(session.id);
  }

  createSession(): void {
    const deck = this.deckState.selectedDeck();
    if (!deck) return;
    this.chatSessionService.createSession(deck.id).subscribe((session) => {
      this.sessions.update((s) => [session, ...s]);
      this.activeSession.set(session);
      this.completedMessages.set([]);
    });
  }

  deleteSession(sessionId: string): void {
    this.chatSessionService.deleteSession(sessionId).subscribe(() => {
      const updated = this.sessions().filter((s) => s.id !== sessionId);
      this.sessions.set(updated);
      if (this.activeSession()?.id === sessionId) {
        const first = updated[0] ?? null;
        this.activeSession.set(first);
        this.completedMessages.set([]);
        if (first) {
          this.loadMessages(first.id);
        }
      }
    });
  }

  renameSession(id: string, name: string): void {
    this.chatSessionService.renameSession(id, name).subscribe(() => {
      this.sessions.update((sessions) =>
        sessions.map((s) => (s.id === id ? { ...s, displayName: name } : s))
      );
      if (this.activeSession()?.id === id) {
        this.activeSession.update((s) => (s ? { ...s, displayName: name } : null));
      }
    });
  }

  addCompletedMessage(msg: CompletedMessage): void {
    this.completedMessages.update((msgs) => [...msgs, msg]);
  }
}

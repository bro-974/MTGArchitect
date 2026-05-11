import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatMessage, ChatSession } from './ai-chat.models';

@Injectable({ providedIn: 'root' })
export class ChatSessionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/chat/sessions`;

  getSessions(deckId: string): Observable<ChatSession[]> {
    return this.http.get<ChatSession[]>(`${this.base}?deckId=${deckId}`);
  }

  createSession(deckId: string): Observable<ChatSession> {
    return this.http.post<ChatSession>(this.base, { deckId });
  }

  deleteSession(sessionId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${sessionId}`);
  }

  renameSession(sessionId: string, name: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${sessionId}/name`, { name });
  }

  getSessionMessages(sessionId: string): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.base}/${sessionId}/messages`);
  }
}

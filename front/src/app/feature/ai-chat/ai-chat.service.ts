import { inject, Injectable } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { environment } from '../../../environments/environment';

export interface AiChunk {
  readonly content: string;
  readonly type: 'Reasoning' | 'Answer' | 'Metadata';
}

export interface AiChatCallbacks {
  onChunk: (chunk: AiChunk) => void;
  onDone: () => void;
  onError: (err: Error) => void;
}

@Injectable({ providedIn: 'root' })
export class AiChatService {
  private readonly authService = inject(AuthService);

  stream(prompt: string, sessionId: string, callbacks: AiChatCallbacks): AbortController {
    const ctrl = new AbortController();
    const token = this.authService.token();
    const url = `${environment.apiUrl}/api/ai/chat?prompt=${encodeURIComponent(prompt)}&sessionId=${sessionId}`;

    fetch(url, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
      signal: ctrl.signal,
    })
      .then((res) => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`);

        const reader = res.body!.getReader();
        const decoder = new TextDecoder();
        let buffer = '';

        const processBuffer = (): void => {
          const lines = buffer.split('\n');
          buffer = lines.pop() ?? '';

          for (const line of lines) {
            const trimmed = line.trim();
            if (!trimmed.startsWith('data:')) continue;
            const json = trimmed.slice(5).trim();
            if (!json) continue;
            try {
              callbacks.onChunk(JSON.parse(json) as AiChunk);
            } catch {
              // ignore malformed chunk
            }
          }
        };

        const read = (): Promise<void> =>
          reader.read().then(({ done, value }) => {
            if (done) {
              processBuffer();
              callbacks.onDone();
              return;
            }
            buffer += decoder.decode(value, { stream: true });
            processBuffer();
            return read();
          });

        return read();
      })
      .catch((err: Error) => {
        if (err.name !== 'AbortError') {
          callbacks.onError(err);
        }
      });

    return ctrl;
  }
}

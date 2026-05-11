import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { SplitterModule } from 'primeng/splitter';
import { AiChatStateService } from './ai-chat-state.service';
import { AiChatService } from './ai-chat.service';
import { AiChatMessages } from './ai-chat-messages/ai-chat-messages';
import { AiChatInput } from './ai-chat-input/ai-chat-input';
import { AiChatPanel } from './ai-chat-panel/ai-chat-panel';

@Component({
  selector: 'app-ai-chat',
  templateUrl: './ai-chat.html',
  styleUrl: './ai-chat.css',
  imports: [SplitterModule, AiChatMessages, AiChatInput, AiChatPanel],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AiChat {
  private readonly aiChatService = inject(AiChatService);
  readonly chatState = inject(AiChatStateService);

  readonly currentPrompt = signal<string | null>(null);
  readonly currentAnswer = signal('');
  readonly currentReasoning = signal('');
  readonly isStreaming = signal(false);

  private ctrl: AbortController | null = null;

  onSend(prompt: string): void {
    const session = this.chatState.activeSession();
    if (!session) return;

    this.isStreaming.set(true);
    this.currentPrompt.set(prompt);
    this.currentAnswer.set('');
    this.currentReasoning.set('');

    this.ctrl = this.aiChatService.stream(
      prompt,
      session.id,
      {
        onChunk: (chunk) => {
          if (chunk.type === 'Answer') {
            this.currentAnswer.update((v) => v + chunk.content);
          } else if (chunk.type === 'Reasoning') {
            this.currentReasoning.update((v) => v + chunk.content);
          }
        },
        onDone: () => {
          this.chatState.addCompletedMessage({
            prompt: this.currentPrompt()!,
            answer: this.currentAnswer(),
            reasoning: this.currentReasoning(),
          });
          this.currentPrompt.set(null);
          this.currentAnswer.set('');
          this.currentReasoning.set('');
          this.isStreaming.set(false);
          this.ctrl = null;
        },
        onError: () => {
          this.isStreaming.set(false);
          this.ctrl = null;
        },
      },
      this.chatState.selectedDeckId(),
    );
  }
}

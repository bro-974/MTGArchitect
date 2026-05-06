import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  input,
  viewChild,
} from '@angular/core';
import { CompletedMessage } from '../ai-chat.models';

@Component({
  selector: 'app-ai-chat-messages',
  templateUrl: './ai-chat-messages.html',
  styleUrl: './ai-chat-messages.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AiChatMessages {
  readonly completedMessages = input.required<readonly CompletedMessage[]>();
  readonly currentPrompt = input<string | null>(null);
  readonly currentAnswer = input('');
  readonly isStreaming = input(false);

  private readonly scrollContainer = viewChild<ElementRef<HTMLDivElement>>('scrollContainer');

  constructor() {
    effect(() => {
      this.completedMessages();
      this.currentAnswer();
      requestAnimationFrame(() => {
        const el = this.scrollContainer()?.nativeElement;
        if (el) el.scrollTop = el.scrollHeight;
      });
    });
  }
}

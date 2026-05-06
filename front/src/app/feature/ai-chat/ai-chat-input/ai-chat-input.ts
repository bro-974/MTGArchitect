import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
  signal,
} from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-ai-chat-input',
  templateUrl: './ai-chat-input.html',
  styleUrl: './ai-chat-input.css',
  imports: [TextareaModule, ButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AiChatInput {
  readonly disabled = input(false);
  readonly send = output<string>();

  readonly text = signal('');

  onTextInput(event: Event): void {
    this.text.set((event.target as HTMLTextAreaElement).value);
  }

  handleKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.submit();
    }
  }

  submit(): void {
    const value = this.text().trim();
    if (!value || this.disabled()) return;
    this.send.emit(value);
    this.text.set('');
  }
}

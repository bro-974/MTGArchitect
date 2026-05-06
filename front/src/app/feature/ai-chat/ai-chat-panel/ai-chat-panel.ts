import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AccordionModule } from 'primeng/accordion';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-ai-chat-panel',
  templateUrl: './ai-chat-panel.html',
  styleUrl: './ai-chat-panel.css',
  imports: [AccordionModule, ProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AiChatPanel {
  readonly currentReasoning = input('');
  readonly isStreaming = input(false);
}

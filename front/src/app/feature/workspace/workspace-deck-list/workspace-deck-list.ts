import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { WorkspaceDeck } from '../workspace.models';

@Component({
  selector: 'app-workspace-deck-list',
  imports: [TranslocoPipe],
  templateUrl: './workspace-deck-list.html',
  styleUrl: './workspace-deck-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceDeckList {
  readonly decks = input.required<readonly WorkspaceDeck[]>();
  readonly selectedDeckId = input<string | null>(null);
  readonly selectDeck = output<string>();

  handleSelect(deckId: string): void {
    this.selectDeck.emit(deckId);
  }

  trackDeck(index: number, deck: WorkspaceDeck): string {
    return deck.id;
  }
}
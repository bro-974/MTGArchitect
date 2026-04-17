import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { catchError, EMPTY, Subscription } from 'rxjs';
import { WorkspaceDeck } from '../workspace.models';
import { WorkspaceService } from '../workspace.service';

@Component({
  selector: 'app-workspace-deck-list',
  imports: [TranslocoPipe, ButtonModule],
  templateUrl: './workspace-deck-list.html',
  styleUrl: './workspace-deck-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceDeckList {
  private readonly workspaceService = inject(WorkspaceService);
  private readonly subscriptions = new Subscription();

  readonly decks = signal<readonly WorkspaceDeck[]>([]);
  readonly selectedDeckId = input<string | null>(null);
  readonly selectDeck = output<string>();
  readonly createDeck = output<void>();

  constructor() {
    this.workspaceService
      .getDecks()
      .pipe(
        catchError(() => {
          this.decks.set([]);
          return EMPTY;
        })
      )
      .subscribe((decks) => {
        this.decks.set(decks);
      });

    this.subscriptions.add(
      this.workspaceService.deckCreated$.subscribe((createdDeck) => {
        this.decks.update((decks) => [createdDeck, ...decks]);
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  handleSelect(deckId: string): void {
    this.selectDeck.emit(deckId);
  }

  handleCreateDeck(): void {
    this.createDeck.emit();
  }

  trackDeck(index: number, deck: WorkspaceDeck): string {
    return deck.id;
  }
}
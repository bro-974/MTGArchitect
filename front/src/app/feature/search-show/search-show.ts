import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { catchError, filter, map, of, startWith, switchMap, take } from 'rxjs';
import type { Observable } from 'rxjs';

import { CardExplorerCard, CardExplorerService } from '../card-explorer/card-explorer.service';
import { WorkspaceDeckStateService } from '../workspace/workspace-deck-state.service';
import { WorkspaceService } from '../workspace/workspace.service';
import { WorkspaceDeckCard, WorkspaceDeckCardAdd } from '../workspace/workspace.models';

type SearchState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; cards: CardExplorerCard[] }
  | { status: 'error' };

@Component({
  selector: 'app-search-show',
  imports: [TranslocoPipe, Button, Tooltip],
  templateUrl: './search-show.html',
  styleUrl: './search-show.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchShow {
  readonly queryText = input.required<string>();
  readonly isActive = input(true);

  private readonly cardExplorerService = inject(CardExplorerService);
  private readonly workspaceService = inject(WorkspaceService);
  private readonly workspaceDeckState = inject(WorkspaceDeckStateService);
  private readonly messageService = inject(MessageService);
  private readonly transloco = inject(TranslocoService);

  readonly #state = toSignal(
    toObservable(this.isActive).pipe(
      filter((isActive) => isActive),
      take(1),
      switchMap(() =>
        this.cardExplorerService.searchCardsAdvanced({ name: this.queryText() }).pipe(
          map((cards): SearchState => ({ status: 'success', cards })),
          catchError((): Observable<SearchState> => of({ status: 'error' })),
          startWith<SearchState>({ status: 'loading' })
        )
      )
    ),
    { initialValue: { status: 'idle' } as SearchState }
  );

  readonly status = computed(() => this.#state().status);
  readonly cards = computed(() => {
    const s = this.#state();
    return s.status === 'success' ? s.cards : [];
  });

  readonly selectedDeck = computed(() => this.workspaceDeckState.selectedDeck());

  addCard(card: CardExplorerCard, isSideBoard: boolean): void {
    const deck = this.workspaceDeckState.selectedDeck();
    if (!deck) return;

    const snapshot = deck;
    const payload: WorkspaceDeckCardAdd = {
      cardName: card.name,
      scryFallId: card.id,
      quantity: 1,
      type: card.typeLine,
      cost: card.manaCost || null,
      isSideBoard
    };

    const existingIndex = deck.cards.findIndex(
      (c) => c.scryFallId === card.id && c.isSideBoard === isSideBoard
    );
    const updatedCards: readonly WorkspaceDeckCard[] =
      existingIndex >= 0
        ? deck.cards.map((c, i) => (i === existingIndex ? { ...c, quantity: c.quantity + 1 } : c))
        : [
            ...deck.cards,
            {
              id: '',
              cardName: card.name,
              scryFallId: card.id,
              quantity: 1,
              type: card.typeLine,
              cost: card.manaCost || null,
              isSideBoard
            }
          ];

    this.workspaceDeckState.selectedDeck.set({ ...deck, cards: updatedCards });

    this.workspaceService.addCardToDeck(deck.id, payload).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.transloco.translate('workspace.addCard.successSummary'),
          detail: this.transloco.translate('workspace.addCard.successDetail', { name: card.name }),
          life: 3000
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.transloco.translate('workspace.addCard.errorSummary'),
          detail: this.transloco.translate('workspace.addCard.errorDetail'),
          life: 4000
        });
        this.workspaceDeckState.selectedDeck.set(snapshot);
        this.workspaceService
          .getDeckById(deck.id)
          .subscribe((freshDeck) => this.workspaceDeckState.selectedDeck.set(freshDeck));
      }
    });
  }
}

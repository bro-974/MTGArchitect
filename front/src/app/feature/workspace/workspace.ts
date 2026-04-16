import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { AccordionModule } from 'primeng/accordion';
import { catchError, EMPTY } from 'rxjs';
import { WorkspaceDeckList } from './workspace-deck-list/workspace-deck-list';
import { WorkspaceSearchForm } from './workspace-search/workspace-search-form';
import {
  WorkspaceDeck,
  WorkspaceDeckUpsert,
  WorkspaceQuerySearchUpsert
} from './workspace.models';
import { WorkspaceService } from './workspace.service';

@Component({
  selector: 'app-workspace',
  imports: [
    TranslocoPipe,
    AccordionModule,
    WorkspaceDeckList,
    WorkspaceSearchForm
  ],
  templateUrl: './workspace.html',
  styleUrl: './workspace.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Workspace {
  private readonly workspaceService = inject(WorkspaceService);

  readonly decks = signal<readonly WorkspaceDeck[]>([]);
  readonly selectedDeckId = signal<string | null>(null);
  readonly activeAccordionPanel = signal('decks');
  readonly isLoading = signal(true);
  readonly isSavingSearch = signal(false);
  readonly hasLoadError = signal(false);

  readonly selectedDeck = computed(
    () => this.decks().find((deck) => deck.id === this.selectedDeckId()) ?? null
  );

  constructor() {
    this.workspaceService
      .getDecks()
      .pipe(
        catchError(() => {
          this.decks.set([]);
          this.selectedDeckId.set(null);
          this.hasLoadError.set(true);
          this.isLoading.set(false);
          return EMPTY;
        })
      )
      .subscribe((decks) => {
        this.decks.set(decks);
        this.hasLoadError.set(false);
        this.selectedDeckId.set(decks[0]?.id ?? null);
        this.isLoading.set(false);
      });
  }

  handleDeckSelection(deckId: string): void {
    this.selectedDeckId.set(deckId);
    this.activeAccordionPanel.set('decks');
  }

  handleSaveQuery(query: string): void {
    const selectedDeck = this.selectedDeck();
    if (!selectedDeck || !query.trim()) {
      return;
    }

    this.isSavingSearch.set(true);

    const request = this.toDeckUpsertRequest(selectedDeck, {
      id: null,
      query: query.trim(),
      searchEngine: 'scryfall'
    });

    this.workspaceService
      .updateDeck(selectedDeck.id, request)
      .pipe(
        catchError(() => {
          this.isSavingSearch.set(false);
          return EMPTY;
        })
      )
      .subscribe((updatedDeck) => {
        this.decks.update((decks) =>
          decks.map((deck) => (deck.id === updatedDeck.id ? updatedDeck : deck))
        );
        this.isSavingSearch.set(false);
      });
  }

  setAccordionPanel(value: string | number | string[] | number[] | null | undefined): void {
    this.activeAccordionPanel.set(typeof value === 'string' ? value : 'decks');
  }

  private toDeckUpsertRequest(
    deck: WorkspaceDeck,
    extraQuery: WorkspaceQuerySearchUpsert
  ): WorkspaceDeckUpsert {
    return {
      name: deck.name,
      type: deck.type,
      note: deck.note,
      querySearches: [...deck.querySearches, extraQuery].map((query) => ({
        id: query.id,
        query: query.query,
        searchEngine: query.searchEngine
      })),
      cards: deck.cards.map((card) => ({
        cardName: card.cardName,
        scryFallId: card.scryFallId,
        quantity: card.quantity,
        type: card.type,
        cost: card.cost,
        isSideBoard: card.isSideBoard
      }))
    };
  }
}
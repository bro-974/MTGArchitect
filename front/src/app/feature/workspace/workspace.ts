import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  output,
  signal
} from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { AccordionModule } from 'primeng/accordion';
import { catchError, EMPTY, Subscription } from 'rxjs';
import { WorkspaceDeckList } from './workspace-deck-list/workspace-deck-list';
import { WorkspaceSearchForm } from './workspace-search/workspace-search-form';
import {
  WorkspaceDeck,
  WorkspaceDeckUpsert
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
  private readonly subscriptions = new Subscription();

  readonly requestCreateDeck = output<void>();
  readonly selectedDeckChange = output<WorkspaceDeck | null>();

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
    effect(() => {
      this.selectedDeckChange.emit(this.selectedDeck());
    });

    this.subscriptions.add(
      this.workspaceService.deckCreated$.subscribe((createdDeck) => {
        this.decks.update((decks) => [createdDeck, ...decks]);
        this.selectedDeckId.set(createdDeck.id);
        this.activeAccordionPanel.set('decks');
      })
    );

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

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  handleDeckSelection(deckId: string): void {
    this.selectedDeckId.set(deckId);
    this.activeAccordionPanel.set('decks');
  }

  handleRequestCreateDeck(): void {
    this.requestCreateDeck.emit();
  }

  handleSaveQuery(query: string): void {
    const selectedDeck = this.selectedDeck();
    if (!selectedDeck || !query.trim()) {
      return;
    }

    this.isSavingSearch.set(true);

    this.workspaceService
      .addQuerySearch(selectedDeck.id, query.trim(), 'scryfall')
      .pipe(
        catchError(() => {
          this.isSavingSearch.set(false);
          return EMPTY;
        })
      )
      .subscribe((queryResponse) => {
        this.decks.update((decks) =>
          decks.map((deck) => {
            if (deck.id === selectedDeck.id) {
              return {
                ...deck,
                querySearches: [
                  ...deck.querySearches,
                  {
                    id: queryResponse.id,
                    query: queryResponse.query,
                    searchEngine: queryResponse.searchEngine
                  }
                ]
              };
            }
            return deck;
          })
        );
        this.isSavingSearch.set(false);
      });
  }

  setAccordionPanel(value: string | number | string[] | number[] | null | undefined): void {
    this.activeAccordionPanel.set(typeof value === 'string' ? value : 'decks');
  }
}
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { MessageService } from 'primeng/api';
import { DividerModule } from 'primeng/divider';
import { TabsModule } from 'primeng/tabs';
import { ManaCostComponent } from '../../core/components/mana-cost/mana-cost.component';
import { SearchShow } from '../search-show/search-show';
import { WorkspaceDeckStateService } from '../workspace/workspace-deck-state.service';
import { WorkspaceService } from '../workspace/workspace.service';
import { WorkspaceDeckCard, WorkspaceQuerySearch } from '../workspace/workspace.models';

interface OpenSearchTab {
  readonly id: string;
  readonly query: WorkspaceQuerySearch;
}

@Component({
  selector: 'app-workspace-deck-selected',
  imports: [TranslocoPipe, TabsModule, DividerModule, ManaCostComponent, SearchShow],
  templateUrl: './workspace-deck-selected.html',
  styleUrl: './workspace-deck-selected.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceDeckSelected {
  private readonly deckStateService = inject(WorkspaceDeckStateService);
  private readonly workspaceService = inject(WorkspaceService);
  private readonly messageService = inject(MessageService);
  private readonly transloco = inject(TranslocoService);

  readonly deck = this.deckStateService.selectedDeck;
  readonly activeTab = signal('overview');
  readonly openSearchTabs = signal<readonly OpenSearchTab[]>([]);
  readonly mainboard = computed(() =>
    (this.deck()?.cards ?? []).filter((card) => !card.isSideBoard)
  );
  readonly sideboard = computed(() =>
    (this.deck()?.cards ?? []).filter((card) => card.isSideBoard)
  );
  readonly colorIdentityPips = computed(() => {
    const ci = this.deck()?.colorIdentity;
    if (!ci) return null;
    return ci.split('').map((c) => `{${c}}`).join('');
  });

  private currentDeckId: string | null = null;
  private hiddenQueryIds = new Set<string>();
  private knownQueryIds = new Set<string>();

  constructor() {
    effect(() => {
      const currentDeck = this.deck();
      if (!currentDeck) {
        this.currentDeckId = null;
        this.hiddenQueryIds = new Set<string>();
        this.knownQueryIds = new Set<string>();
        this.openSearchTabs.set([]);
        this.activeTab.set('overview');
        return;
      }

      if (this.currentDeckId !== currentDeck.id) {
        this.currentDeckId = currentDeck.id;
        this.hiddenQueryIds = new Set<string>();
        this.knownQueryIds = new Set(currentDeck.querySearches.map((query) => query.id));
        this.openSearchTabs.set(
          currentDeck.querySearches.map((query) => ({
            id: query.id,
            query
          }))
        );
        this.activeTab.set('overview');
        return;
      }

      const currentQueryIds = new Set(currentDeck.querySearches.map((query) => query.id));

      this.hiddenQueryIds = new Set(
        Array.from(this.hiddenQueryIds).filter((queryId) => currentQueryIds.has(queryId))
      );

      const newQueryIds = currentDeck.querySearches
        .map((query) => query.id)
        .filter((queryId) => !this.knownQueryIds.has(queryId));

      this.openSearchTabs.set(
        currentDeck.querySearches
          .filter((query) => !this.hiddenQueryIds.has(query.id))
          .map((query) => ({
            id: query.id,
            query
          }))
      );

      if (newQueryIds.length > 0) {
        const newestQueryId = newQueryIds[newQueryIds.length - 1];
        this.activeTab.set(newestQueryId);
      }

      this.knownQueryIds = currentQueryIds;
    });
  }

  trackCard(index: number, card: WorkspaceDeckCard): string {
    return card.id;
  }

  trackSearchTab(index: number, tab: OpenSearchTab): string {
    return tab.id;
  }

  setActiveTab(value: string | number | null | undefined): void {
    this.activeTab.set(typeof value === 'string' ? value : 'overview');
  }

  openSearch(querySearch: WorkspaceQuerySearch): void {
    this.hiddenQueryIds.delete(querySearch.id);

    // Check if this search is already open
    const isAlreadyOpen = this.openSearchTabs().some((tab) => tab.id === querySearch.id);
    if (isAlreadyOpen) {
      // Just switch to this tab if already open
      this.activeTab.set(querySearch.id);
      return;
    }

    // Add new search tab
    const newTab: OpenSearchTab = {
      id: querySearch.id,
      query: querySearch
    };
    this.openSearchTabs.update((tabs) => [...tabs, newTab]);
    this.activeTab.set(querySearch.id);
  }

  closeSearch(queryId: string, event: Event): void {
    event.stopPropagation();

    const deck = this.deck();
    if (!deck) return;

    const closedTab = this.openSearchTabs().find((tab) => tab.id === queryId);
    this.hiddenQueryIds.add(queryId);
    this.openSearchTabs.update((tabs) => tabs.filter((tab) => tab.id !== queryId));
    if (this.activeTab() === queryId) {
      this.activeTab.set('overview');
    }

    this.workspaceService.removeQuerySearch(deck.id, queryId).subscribe({
      next: () => {
        this.deckStateService.selectedDeck.update((d) =>
          d ? { ...d, querySearches: d.querySearches.filter((q) => q.id !== queryId) } : null
        );
      },
      error: () => {
        this.hiddenQueryIds.delete(queryId);
        if (closedTab) {
          this.openSearchTabs.update((tabs) => [...tabs, closedTab]);
        }
        this.messageService.add({
          severity: 'error',
          summary: this.transloco.translate('workspace.addCard.errorSummary'),
          detail: this.transloco.translate('workspace.addCard.errorDetail'),
        });
      },
    });
  }
}

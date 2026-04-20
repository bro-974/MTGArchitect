import { ChangeDetectionStrategy, Component, computed, effect, input, signal } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { TabsModule } from 'primeng/tabs';
import { CardQuerySearch } from '../../core/models/card-query-search.model';
import { SearchShow } from '../search-show/search-show';
import { WorkspaceDeck, WorkspaceDeckCard, WorkspaceQuerySearch } from '../workspace/workspace.models';

interface OpenSearchTab {
  readonly id: string;
  readonly query: WorkspaceQuerySearch;
}

@Component({
  selector: 'app-workspace-deck-selected',
  imports: [TranslocoPipe, TabsModule, SearchShow],
  templateUrl: './workspace-deck-selected.html',
  styleUrl: './workspace-deck-selected.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceDeckSelected {
  readonly deck = input<WorkspaceDeck | null>(null);
  readonly activeTab = signal('overview');
  readonly openSearchTabs = signal<readonly OpenSearchTab[]>([]);
  readonly mainboard = computed(() =>
    (this.deck()?.cards ?? []).filter((card) => !card.isSideBoard)
  );

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
    this.hiddenQueryIds.add(queryId);
    this.openSearchTabs.update((tabs) => tabs.filter((tab) => tab.id !== queryId));
    // Switch back to overview if the closed tab was active
    if (this.activeTab() === queryId) {
      this.activeTab.set('overview');
    }
  }
}

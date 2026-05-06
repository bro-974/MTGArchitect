import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TreeModule } from 'primeng/tree';
import { TreeNode } from 'primeng/api';
import { catchError, EMPTY, Subscription } from 'rxjs';
import { WorkspaceDeck } from '../workspace.models';
import { WorkspaceService } from '../workspace.service';

interface DeckTreeNodeData {
  readonly kind: 'type' | 'deck';
  readonly deckId?: string;
}

@Component({
  selector: 'app-workspace-deck-list',
  imports: [TranslocoPipe, ButtonModule, TreeModule, ProgressSpinnerModule],
  templateUrl: './workspace-deck-list.html',
  styleUrl: './workspace-deck-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceDeckList {
  private readonly workspaceService = inject(WorkspaceService);
  private readonly subscriptions = new Subscription();

  readonly isLoading = signal(true);
  readonly decks = signal<readonly WorkspaceDeck[]>([]);
  readonly groupedDecks = computed(() => {
    const grouped = new Map<string, WorkspaceDeck[]>();

    for (const deck of this.decks()) {
      const type = deck.type.trim();
      const existing = grouped.get(type);

      if (existing) {
        existing.push(deck);
        continue;
      }

      grouped.set(type, [deck]);
    }

    return Array.from(grouped.entries()).map(([type, decks]) => ({
      type,
      decks
    }));
  });
  readonly treeNodes = computed<TreeNode<DeckTreeNodeData>[]>(() => {
    return this.groupedDecks().map((group) => ({
      key: `type:${group.type}`,
      label: group.type,
      data: { kind: 'type' },
      expanded: true,
      selectable: false,
      children: group.decks.map((deck) => ({
        key: `deck:${deck.id}`,
        label: deck.name,
        data: {
          kind: 'deck',
          deckId: deck.id
        }
      }))
    }));
  });
  readonly selectedNode = computed<TreeNode<DeckTreeNodeData> | null>(() => {
    const selectedDeckId = this.selectedDeckId();
    if (!selectedDeckId) {
      return null;
    }

    for (const typeNode of this.treeNodes()) {
      const matchingDeckNode = typeNode.children?.find((node) => node.data?.deckId === selectedDeckId);
      if (matchingDeckNode) {
        return matchingDeckNode;
      }
    }

    return null;
  });
  readonly selectedDeckId = input<string | null>(null);
  readonly selectDeck = output<string>();
  readonly createDeck = output<void>();

  constructor() {
    this.workspaceService
      .getDecks()
      .pipe(
        catchError(() => {
          this.decks.set([]);
          this.isLoading.set(false);
          return EMPTY;
        })
      )
      .subscribe((decks) => {
        this.decks.set(decks);
        this.isLoading.set(false);
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

  handleNodeSelect(event: { node: TreeNode<DeckTreeNodeData> }): void {
    const deckId = event.node.data?.deckId;
    if (!deckId) {
      return;
    }

    this.handleSelect(deckId);
  }

  handleCreateDeck(): void {
    this.createDeck.emit();
  }

}
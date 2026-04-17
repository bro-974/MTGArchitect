import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { TabsModule } from 'primeng/tabs';
import { WorkspaceDeck, WorkspaceDeckCard } from '../workspace/workspace.models';

@Component({
  selector: 'app-workspace-deck-selected',
  imports: [TranslocoPipe, TabsModule],
  templateUrl: './workspace-deck-selected.html',
  styleUrl: './workspace-deck-selected.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceDeckSelected {
  readonly deck = input<WorkspaceDeck | null>(null);
  readonly activeTab = signal('overview');
  readonly mainboard = computed(() =>
    (this.deck()?.cards ?? []).filter((card) => !card.isSideBoard)
  );

  trackCard(index: number, card: WorkspaceDeckCard): string {
    return card.id;
  }

  setActiveTab(value: string | number | null | undefined): void {
    this.activeTab.set(typeof value === 'string' ? value : 'overview');
  }
}
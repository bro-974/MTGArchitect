import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { ManaCostComponent } from '../../core/components/mana-cost/mana-cost.component';
import { CardExplorerCard } from '../card-explorer/card-explorer.service';

@Component({
  selector: 'app-card-explorer-list',
  imports: [TranslocoPipe, ManaCostComponent, SkeletonModule, TagModule],
  templateUrl: './card-explorer-list.html',
  styleUrl: './card-explorer-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardExplorerList {
  readonly cards = input.required<readonly CardExplorerCard[]>();
  readonly query = input('');
  readonly loading = input(false);
  readonly error = input(false);
}
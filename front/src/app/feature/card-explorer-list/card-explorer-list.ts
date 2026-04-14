import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { CardExplorerCard } from '../card-explorer/card-explorer.service';

@Component({
  selector: 'app-card-explorer-list',
  imports: [TranslocoPipe],
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
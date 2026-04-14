import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { catchError, EMPTY, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject } from 'rxjs';
import { TranslocoPipe } from '@jsverse/transloco';
import { CardExplorerList } from '../card-explorer-list/card-explorer-list';
import { CardExplorerSearch } from '../card-explorer-search/card-explorer-search';
import { CardExplorerCard, CardExplorerService } from './card-explorer.service';

type ExplorerState = 'idle' | 'loading' | 'ready' | 'error';

@Component({
  selector: 'app-card-explorer',
  imports: [CardExplorerSearch, CardExplorerList, TranslocoPipe],
  templateUrl: './card-explorer.html',
  styleUrl: './card-explorer.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardExplorer {
  private readonly cardExplorerService = inject(CardExplorerService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchRequests = new Subject<string>();

  readonly state = signal<ExplorerState>('idle');
  readonly query = signal('');
  readonly cards = signal<readonly CardExplorerCard[]>([]);

  constructor() {
    this.searchRequests
      .pipe(
        tap((query) => {
          this.query.set(query);

          if (!query) {
            this.cards.set([]);
            this.state.set('idle');
          }
        }),
        switchMap((query) => {
          if (!query) {
            return EMPTY;
          }

          this.state.set('loading');

          return this.cardExplorerService.searchCards(query).pipe(
            tap((cards) => {
              this.cards.set(cards);
              this.state.set('ready');
            }),
            catchError(() => {
              this.cards.set([]);
              this.state.set('error');
              return EMPTY;
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  handleSearch(query: string): void {
    this.searchRequests.next(query);
  }
}
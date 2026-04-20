import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { TranslocoPipe } from '@jsverse/transloco';
import { catchError, filter, map, of, startWith, switchMap, take } from 'rxjs';
import type { Observable } from 'rxjs';

import { CardExplorerCard, CardExplorerService } from '../card-explorer/card-explorer.service';

type SearchState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; cards: CardExplorerCard[] }
  | { status: 'error' };

@Component({
  selector: 'app-search-show',
  imports: [TranslocoPipe],
  templateUrl: './search-show.html',
  styleUrl: './search-show.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchShow {
  readonly queryText = input.required<string>();
  readonly isActive = input(true);

  private readonly cardExplorerService = inject(CardExplorerService);

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
}


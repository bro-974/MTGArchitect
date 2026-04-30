import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { SlicePipe, UpperCasePipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { Accordion, AccordionContent, AccordionHeader, AccordionPanel } from 'primeng/accordion';
import { Button } from 'primeng/button';
import { Skeleton } from 'primeng/skeleton';
import { Tag } from 'primeng/tag';
import { catchError, filter, map, of, startWith, switchMap } from 'rxjs';
import type { Observable } from 'rxjs';
import { CardDetail, CardDetailService } from './card-detail.service';

type PanelState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; detail: CardDetail }
  | { status: 'error' };

const RARITY_SEVERITY: Record<string, 'secondary' | 'info' | 'warn' | 'danger' | 'success' | 'contrast'> = {
  common: 'secondary',
  uncommon: 'info',
  rare: 'warn',
  mythic: 'danger',
  special: 'contrast',
  bonus: 'contrast',
};

@Component({
  selector: 'app-card-detail-panel',
  imports: [
    TranslocoPipe,
    UpperCasePipe,
    SlicePipe,
    Button,
    Skeleton,
    Tag,
    Accordion,
    AccordionPanel,
    AccordionHeader,
    AccordionContent,
  ],
  templateUrl: './card-detail-panel.html',
  styleUrl: './card-detail-panel.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(keydown.escape)': 'close.emit()',
  },
})
export class CardDetailPanel {
  readonly cardId = input.required<string>();
  readonly close = output<void>();
  readonly addCard = output<{ isSideBoard: boolean }>();

  private readonly cardDetailService = inject(CardDetailService);

  readonly selectedImageUrl = signal<string | null>(null);

  readonly #state = toSignal(
    toObservable(this.cardId).pipe(
      filter((id) => !!id),
      switchMap((id) =>
        this.cardDetailService.getCardDetail(id).pipe(
          map((detail): PanelState => {
            this.selectedImageUrl.set(detail.imageLargeUrl || detail.imageUrl);
            return { status: 'success', detail };
          }),
          catchError((): Observable<PanelState> => of({ status: 'error' })),
          startWith<PanelState>({ status: 'loading' })
        )
      )
    ),
    { initialValue: { status: 'idle' } as PanelState }
  );

  readonly status = computed(() => this.#state().status);
  readonly detail = computed(() => {
    const s = this.#state();
    return s.status === 'success' ? s.detail : null;
  });

  readonly displayImageUrl = computed(() => this.selectedImageUrl() ?? this.detail()?.imageUrl ?? '');

  readonly legalFormats = computed(() => {
    const d = this.detail();
    if (!d) return [];
    return Object.entries(d.legalities)
      .filter(([, v]) => v === 'legal')
      .map(([k]) => k);
  });

  readonly hasRulings = computed(() => (this.detail()?.rulings?.length ?? 0) > 0);

  raritySeverity(rarity: string) {
    return RARITY_SEVERITY[rarity?.toLowerCase()] ?? 'secondary';
  }

  selectPrinting(imageUrl: string): void {
    this.selectedImageUrl.set(imageUrl);
  }
}

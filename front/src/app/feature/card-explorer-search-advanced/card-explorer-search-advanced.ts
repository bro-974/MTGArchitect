import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { catchError, EMPTY, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject } from 'rxjs';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { CardExplorerList } from '../card-explorer-list/card-explorer-list';
import {
  CardExplorerCard,
  CardExplorerService
} from '../card-explorer/card-explorer.service';
import {
  CardFormat,
  CardQuerySearch,
  CardRarity,
  ComparisonOperator
} from '../../core/models/card-query-search.model';

type AdvancedSearchState = 'idle' | 'loading' | 'ready' | 'error';

interface SelectOption<T> {
  label: string;
  value: T;
}

type ManaColorValue = 'W' | 'U' | 'B' | 'R' | 'G' | 'C';

interface ManaColorOption {
  value: ManaColorValue;
  labelKey: string;
  background: string;
  border: string;
  color: string;
}

@Component({
  selector: 'app-card-explorer-search-advanced',
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    ButtonModule,
    InputTextModule,
    SelectModule,
    CheckboxModule,
    CardExplorerList
  ],
  templateUrl: './card-explorer-search-advanced.html',
  styleUrl: './card-explorer-search-advanced.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardExplorerSearchAdvanced {
  private readonly cardExplorerService = inject(CardExplorerService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchRequests = new Subject<CardQuerySearch>();

  readonly state = signal<AdvancedSearchState>('idle');
  readonly cards = signal<readonly CardExplorerCard[]>([]);
  readonly submittedQuery = signal<CardQuerySearch | null>(null);
  readonly selectedManaColors = signal<readonly ManaColorValue[]>([]);
  readonly selectedManaIdentityColors = signal<readonly ManaColorValue[]>([]);

  readonly manaColorOrder: readonly ManaColorValue[] = ['W', 'U', 'B', 'R', 'G', 'C'];
  readonly manaColorOptions: readonly ManaColorOption[] = [
    { value: 'W', labelKey: 'cardExplorerAdvanced.color.white', background: '#f8fafc', border: '#cbd5e1', color: '#111827' },
    { value: 'U', labelKey: 'cardExplorerAdvanced.color.blue', background: '#60a5fa', border: '#2563eb', color: '#0b1220' },
    { value: 'B', labelKey: 'cardExplorerAdvanced.color.black', background: '#1f2937', border: '#111827', color: '#f9fafb' },
    { value: 'R', labelKey: 'cardExplorerAdvanced.color.red', background: '#f87171', border: '#dc2626', color: '#111827' },
    { value: 'G', labelKey: 'cardExplorerAdvanced.color.green', background: '#4ade80', border: '#16a34a', color: '#0b1220' },
    { value: 'C', labelKey: 'cardExplorerAdvanced.color.gray', background: '#9ca3af', border: '#6b7280', color: '#111827' }
  ];

  readonly operatorOptions: SelectOption<ComparisonOperator>[] = [
    { label: '=', value: 'Equal' },
    { label: '≠', value: 'NotEqual' },
    { label: '>', value: 'GreaterThan' },
    { label: '≥', value: 'GreaterThanOrEqual' },
    { label: '<', value: 'LessThan' },
    { label: '≤', value: 'LessThanOrEqual' }
  ];

  readonly colorOperatorOptions: SelectOption<ComparisonOperator>[] = [
    { label: '= Exactly these colors', value: 'Equal' },
    { label: '<= At most these colors', value: 'LessThanOrEqual' },
    { label: '>= Including these colors', value: 'GreaterThanOrEqual' }
  ];

  readonly rarityOptions: SelectOption<CardRarity>[] = [
    { label: 'Common', value: 'Common' },
    { label: 'Uncommon', value: 'Uncommon' },
    { label: 'Rare', value: 'Rare' },
    { label: 'Special', value: 'Special' },
    { label: 'Mythic', value: 'Mythic' },
    { label: 'Bonus', value: 'Bonus' }
  ];

  readonly formatOptions: SelectOption<CardFormat>[] = [
    { label: 'Standard', value: 'Standard' },
    { label: 'Pioneer', value: 'Pioneer' },
    { label: 'Modern', value: 'Modern' },
    { label: 'Legacy', value: 'Legacy' },
    { label: 'Vintage', value: 'Vintage' },
    { label: 'Commander', value: 'Commander' },
    { label: 'Pauper', value: 'Pauper' },
    { label: 'Historic', value: 'Historic' },
    { label: 'Timeless', value: 'Timeless' },
    { label: 'Alchemy', value: 'Alchemy' },
    { label: 'Brawl', value: 'Brawl' },
    { label: 'Oathbreaker', value: 'Oathbreaker' },
    { label: 'Future', value: 'Future' },
    { label: 'Gladiator', value: 'Gladiator' },
    { label: 'Penny', value: 'Penny' },
    { label: 'Predh', value: 'Predh' },
    { label: 'Premodern', value: 'Premodern' },
    { label: 'OldSchool', value: 'OldSchool' },
    { label: 'Duel', value: 'Duel' },
    { label: 'PauperCommander', value: 'PauperCommander' },
    { label: 'StandardBrawl', value: 'StandardBrawl' }
  ];

  readonly form = new FormGroup({
    name: new FormControl<string>('', { nonNullable: true }),
    exactName: new FormControl<boolean>(false, { nonNullable: true }),
    color: new FormControl<string>('', { nonNullable: true }),
    colorOperator: new FormControl<ComparisonOperator>('Equal', { nonNullable: true }),
    colorIdentity: new FormControl<string>('', { nonNullable: true }),
    types: new FormControl<string>('', { nonNullable: true }),
    excludedTypes: new FormControl<string>('', { nonNullable: true }),
    oracleText: new FormControl<string>('', { nonNullable: true }),
    keyword: new FormControl<string>('', { nonNullable: true }),
    manaCost: new FormControl<string>('', { nonNullable: true }),
    manaValue: new FormControl<number | null>(null),
    manaValueOperator: new FormControl<ComparisonOperator>('Equal', { nonNullable: true }),
    power: new FormControl<string>('', { nonNullable: true }),
    powerOperator: new FormControl<ComparisonOperator>('Equal', { nonNullable: true }),
    toughness: new FormControl<string>('', { nonNullable: true }),
    toughnessOperator: new FormControl<ComparisonOperator>('Equal', { nonNullable: true }),
    rarity: new FormControl<CardRarity | null>(null),
    rarityOperator: new FormControl<ComparisonOperator>('Equal', { nonNullable: true }),
    format: new FormControl<CardFormat | null>(null)
  });

  constructor() {
    this.searchRequests
      .pipe(
        tap((query) => {
          this.submittedQuery.set(query);
          this.state.set('loading');
        }),
        switchMap((query) =>
          this.cardExplorerService.searchCardsAdvanced(query).pipe(
            tap((result) => {
              this.cards.set(result.cards);
              this.state.set('ready');
            }),
            catchError(() => {
              this.cards.set([]);
              this.state.set('error');
              return EMPTY;
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  handleSubmit(): void {
    const v = this.form.getRawValue();
    const query: CardQuerySearch = {};

    if (v.name) {
      query.name = v.name;
      query.exactName = v.exactName || undefined;
    }
    if (v.color) {
      query.color = v.color;
      query.colorOperator = v.colorOperator;
    }
    if (v.colorIdentity) {
      query.colorIdentity = v.colorIdentity;
      query.colorIdentityOperator = 'Equal';
    }
    const types = v.types.split(',').map(s => s.trim()).filter(Boolean);
    const excludedTypes = v.excludedTypes.split(',').map(s => s.trim()).filter(Boolean);
    if (types.length) query.types = types;
    if (excludedTypes.length) query.excludedTypes = excludedTypes;
    if (v.oracleText) query.oracleText = v.oracleText;
    if (v.keyword) query.keyword = v.keyword;
    if (v.manaCost) query.manaCost = v.manaCost;
    if (v.manaValue !== null) {
      query.manaValue = v.manaValue;
      query.manaValueOperator = v.manaValueOperator;
    }
    if (v.power) {
      query.power = v.power;
      query.powerOperator = v.powerOperator;
    }
    if (v.toughness) {
      query.toughness = v.toughness;
      query.toughnessOperator = v.toughnessOperator;
    }
    if (v.rarity) {
      query.rarity = v.rarity;
      query.rarityOperator = v.rarityOperator;
    }
    if (v.format) query.format = v.format;

    this.cardExplorerService.searchCardsAdvanced(query).subscribe({
      next: (result) => {
        this.cards.set(result.cards);
        this.submittedQuery.set(query);
        this.state.set('ready');
      },
      error: () => {
        this.cards.set([]);
        this.state.set('error');
      }
    });
  }

  handleReset(): void {
    this.form.reset();
    this.selectedManaColors.set([]);
    this.selectedManaIdentityColors.set([]);
    this.cards.set([]);
    this.submittedQuery.set(null);
    this.state.set('idle');
  }

  isManaColorSelected(value: ManaColorValue): boolean {
    return this.selectedManaColors().includes(value);
  }

  toggleManaColor(value: ManaColorValue): void {
    const selected = new Set(this.selectedManaColors());
    if (selected.has(value)) {
      selected.delete(value);
    } else {
      selected.add(value);
    }

    const ordered = this.manaColorOrder.filter((color) => selected.has(color));
    this.selectedManaColors.set(ordered);
    this.form.controls.color.setValue(ordered.join(''));
  }

  isManaIdentityColorSelected(value: ManaColorValue): boolean {
    return this.selectedManaIdentityColors().includes(value);
  }

  toggleManaIdentityColor(value: ManaColorValue): void {
    const selected = new Set(this.selectedManaIdentityColors());
    if (selected.has(value)) {
      selected.delete(value);
    } else {
      selected.add(value);
    }

    const ordered = this.manaColorOrder.filter((color) => selected.has(color));
    this.selectedManaIdentityColors.set(ordered);
    this.form.controls.colorIdentity.setValue(ordered.join(''));
  }

  get queryLabel(): string {
    const q = this.submittedQuery();
    if (!q) return '';
    const parts: string[] = [];
    if (q.name) parts.push(q.name);
    if (q.types?.length) parts.push(q.types.join(', '));
    if (q.manaCost) parts.push(q.manaCost);
    return parts.join(' · ') || '(advanced query)';
  }
}

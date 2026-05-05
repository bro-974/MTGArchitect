import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { CardFormat, CardQuerySearch, ComparisonOperator } from '../../../core/models/card-query-search.model';

type ManaColorValue = 'W' | 'U' | 'B' | 'R' | 'G' | 'C';

interface SelectOption<T> {
  label: string;
  value: T;
}

interface ManaColorOption {
  value: ManaColorValue;
  labelKey: string;
  background: string;
  border: string;
  color: string;
}

@Component({
  selector: 'app-workspace-search-form',
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    ButtonModule,
    CheckboxModule,
    InputTextModule,
    SelectModule,
    TooltipModule
  ],
  templateUrl: './workspace-search-form.html',
  styleUrl: './workspace-search-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceSearchForm {
  readonly saving = input(false);
  readonly deckFormat = input<CardFormat | null>(null);
  readonly deckColorIdentity = input<string | null>(null);
  readonly saveQuery = output<string>();

  readonly selectedColors = signal<readonly ManaColorValue[]>([]);
  readonly selectedIdentityColors = signal<readonly ManaColorValue[]>([]);
  readonly savedQuery = signal<CardQuerySearch | null>(null);

  readonly manaColorOrder: readonly ManaColorValue[] = ['W', 'U', 'B', 'R', 'G', 'C'];
  readonly manaColorOptions: readonly ManaColorOption[] = [
    { value: 'W', labelKey: 'workspace.search.color.white', background: '#f8fafc', border: '#cbd5e1', color: '#111827' },
    { value: 'U', labelKey: 'workspace.search.color.blue', background: '#60a5fa', border: '#2563eb', color: '#0b1220' },
    { value: 'B', labelKey: 'workspace.search.color.black', background: '#1f2937', border: '#111827', color: '#f9fafb' },
    { value: 'R', labelKey: 'workspace.search.color.red', background: '#f87171', border: '#dc2626', color: '#111827' },
    { value: 'G', labelKey: 'workspace.search.color.green', background: '#4ade80', border: '#16a34a', color: '#0b1220' },
    { value: 'C', labelKey: 'workspace.search.color.gray', background: '#9ca3af', border: '#6b7280', color: '#111827' }
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
    { label: 'Future', value: 'Future' },
    { label: 'Premodern', value: 'Premodern' }
  ];

  readonly form = new FormGroup({
    name: new FormControl<string>('', { nonNullable: true }),
    exactName: new FormControl<boolean>(false, { nonNullable: true }),
    color: new FormControl<string>('', { nonNullable: true }),
    colorOperator: new FormControl<ComparisonOperator>('Equal', { nonNullable: true }),
    colorIdentity: new FormControl<string>('', { nonNullable: true }),
    types: new FormControl<string>('', { nonNullable: true }),
    oracleText: new FormControl<string>('', { nonNullable: true }),
    manaValue: new FormControl<number | null>(null),
    manaValueOperator: new FormControl<ComparisonOperator>('Equal', { nonNullable: true }),
    format: new FormControl<CardFormat | null>(null)
  });

  constructor() {
    console.log('[WorkspaceSearchForm] constructor');
    toObservable(this.deckFormat)
      .pipe(takeUntilDestroyed())
      .subscribe((format) => {
        console.log('[WorkspaceSearchForm] deckFormat changed →', format);
        this.form.controls.format.setValue(format);
        console.log('[WorkspaceSearchForm] form.format after setValue →', this.form.controls.format.value);
      });

    toObservable(this.deckColorIdentity)
      .pipe(takeUntilDestroyed())
      .subscribe((ci) => {
        const colors = ci ? (ci.split('') as ManaColorValue[]) : [];
        this.selectedIdentityColors.set(colors);
        this.form.controls.colorIdentity.setValue(ci ?? '');
      });
  }

  readonly savedQueryLabel = computed(() => {
    const query = this.savedQuery();
    if (!query) {
      return '';
    }

    return Object.entries(query)
      .filter(([key]) => key !== 'colorOperator' && key !== 'manaValueOperator' && key !== 'exactName')
      .map(([key, value]) => `${key}: ${value}`)
      .join(' · ');
  });

  handleSubmit(): void {
    const value = this.form.getRawValue();
    const query: CardQuerySearch = {};

    if (value.name) {
      query.name = value.name;
      if (value.exactName) {
        query.exactName = true;
      }
    }

    if (value.color) {
      query.color = value.color;
      query.colorOperator = value.colorOperator;
    }

    if (value.colorIdentity) {
      query.colorIdentity = value.colorIdentity;
    }

    if (value.types) {
      query.types = [value.types];
    }

    if (value.oracleText) {
      query.oracleText = value.oracleText;
    }

    if (value.manaValue !== null) {
      query.manaValue = value.manaValue;
      query.manaValueOperator = value.manaValueOperator;
    }

    if (value.format) {
      query.format = value.format;
    }

    this.savedQuery.set(query);
    this.saveQuery.emit(JSON.stringify(query));
  }

  handleReset(): void {
    const ci = this.deckColorIdentity();
    this.form.reset({
      name: '',
      exactName: false,
      color: '',
      colorOperator: 'Equal',
      colorIdentity: ci ?? '',
      types: '',
      oracleText: '',
      manaValue: null,
      manaValueOperator: 'Equal',
      format: this.deckFormat()
    });
    this.selectedColors.set([]);
    this.selectedIdentityColors.set(ci ? (ci.split('') as ManaColorValue[]) : []);
    this.savedQuery.set(null);
  }

  isColorSelected(value: ManaColorValue): boolean {
    return this.selectedColors().includes(value);
  }

  isIdentityColorSelected(value: ManaColorValue): boolean {
    return this.selectedIdentityColors().includes(value);
  }

  toggleColor(value: ManaColorValue): void {
    const nextValues = this.toggleValue(this.selectedColors(), value);
    this.selectedColors.set(nextValues);
    this.form.controls.color.setValue(nextValues.join(''));
  }

  toggleIdentityColor(value: ManaColorValue): void {
    const nextValues = this.toggleValue(this.selectedIdentityColors(), value);
    this.selectedIdentityColors.set(nextValues);
    this.form.controls.colorIdentity.setValue(nextValues.join(''));
  }

  private toggleValue(values: readonly ManaColorValue[], toggledValue: ManaColorValue): readonly ManaColorValue[] {
    const selected = new Set(values);
    if (selected.has(toggledValue)) {
      selected.delete(toggledValue);
    } else {
      selected.add(toggledValue);
    }

    return this.manaColorOrder.filter((value) => selected.has(value));
  }

}
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { CardFormat, ComparisonOperator } from '../../../core/models/card-query-search.model';

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

interface WorkspaceSavedQuery {
  name?: string;
  exactName?: true;
  color?: string;
  colorIdentity?: string;
  types?: string;
  oracleText?: string;
  manaValue?: string;
  format?: CardFormat;
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
  readonly saveQuery = output<string>();

  readonly selectedColors = signal<readonly ManaColorValue[]>([]);
  readonly selectedIdentityColors = signal<readonly ManaColorValue[]>([]);
  readonly savedQuery = signal<WorkspaceSavedQuery | null>(null);

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

  readonly savedQueryLabel = computed(() => {
    const query = this.savedQuery();
    if (!query) {
      return '';
    }

    return Object.entries(query)
      .map(([key, value]) => `${key}: ${value}`)
      .join(' · ');
  });

  handleSubmit(): void {
    const value = this.form.getRawValue();
    const query: WorkspaceSavedQuery = {};

    if (value.name) {
      query.name = value.name;
      if (value.exactName) {
        query.exactName = true;
      }
    }

    if (value.color) {
      query.color = `${value.colorOperator}:${value.color}`;
    }

    if (value.colorIdentity) {
      query.colorIdentity = value.colorIdentity;
    }

    if (value.types) {
      query.types = value.types;
    }

    if (value.oracleText) {
      query.oracleText = value.oracleText;
    }

    if (value.manaValue !== null) {
      query.manaValue = `${value.manaValueOperator}:${value.manaValue}`;
    }

    if (value.format) {
      query.format = value.format;
    }

    this.savedQuery.set(query);

    const queryString = this.toQueryString(query);
    if (queryString) {
      this.saveQuery.emit(queryString);
    }
  }

  handleReset(): void {
    this.form.reset({
      name: '',
      exactName: false,
      color: '',
      colorOperator: 'Equal',
      colorIdentity: '',
      types: '',
      oracleText: '',
      manaValue: null,
      manaValueOperator: 'Equal',
      format: null
    });
    this.selectedColors.set([]);
    this.selectedIdentityColors.set([]);
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

  private toQueryString(query: WorkspaceSavedQuery): string {
    const tokens: string[] = [];

    if (query.name) {
      tokens.push(query.exactName ? `!"${query.name}"` : query.name);
    }

    if (query.color) {
      tokens.push(`c:${query.color}`);
    }

    if (query.colorIdentity) {
      tokens.push(`id:${query.colorIdentity}`);
    }

    if (query.types) {
      tokens.push(`t:${query.types}`);
    }

    if (query.oracleText) {
      tokens.push(`o:${query.oracleText}`);
    }

    if (query.manaValue) {
      tokens.push(`mv${query.manaValue}`);
    }

    if (query.format) {
      tokens.push(`f:${query.format.toLowerCase()}`);
    }

    return tokens.join(' ').trim();
  }
}
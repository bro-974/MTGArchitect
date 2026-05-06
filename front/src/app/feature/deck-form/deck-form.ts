import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { TranslocoPipe } from '@jsverse/transloco';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { catchError, debounceTime, distinctUntilChanged, EMPTY, finalize, Subject, switchMap } from 'rxjs';
import { WorkspaceService } from '../workspace/workspace.service';
import { WorkspaceLayoutStateService } from '../workspace-layout/workspace-layout-state.service';
import { CardFormat } from '../../core/models/card-query-search.model';

type ManaColorValue = 'W' | 'U' | 'B' | 'R' | 'G' | 'C';

interface ManaColorOption {
  value: ManaColorValue;
  labelKey: string;
  background: string;
  border: string;
  color: string;
}

interface CommanderSuggestion {
  readonly name: string;
  readonly typeLine: string;
  readonly colorIdentity: readonly string[];
}

interface ScryfallCard {
  readonly name: string;
  readonly type_line: string;
  readonly color_identity: readonly string[];
}

interface ScryfallSearchResponse {
  readonly data: readonly ScryfallCard[];
  readonly object: string;
}

const COMMANDER_FORMATS = ['Commander', 'Brawl', 'Oathbreaker'] as const;

@Component({
  selector: 'app-deck-form',
  templateUrl: './deck-form.html',
  styleUrl: './deck-form.css',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TranslocoPipe,
    AutoCompleteModule,
    ButtonModule,
    DividerModule,
    InputTextModule,
    TextareaModule,
    SelectModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeckForm {
  private readonly workspaceService = inject(WorkspaceService);
  private readonly layoutStateService = inject(WorkspaceLayoutStateService);
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);

  // ── Form state ────────────────────────────────────────
  readonly submitting = signal(false);
  readonly hasError = signal(false);

  readonly deckForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)]
    }),
    format: new FormControl<CardFormat | null>(null),
    note: new FormControl('', { nonNullable: true })
  });

  // ── Format options ────────────────────────────────────
  readonly formatOptions = [
    { label: 'Commander / EDH', value: 'Commander' },
    { label: 'Standard', value: 'Standard' },
    { label: 'Modern', value: 'Modern' },
    { label: 'Pioneer', value: 'Pioneer' },
    { label: 'Legacy', value: 'Legacy' },
    { label: 'Vintage', value: 'Vintage' },
    { label: 'Pauper', value: 'Pauper' },
    { label: 'Brawl', value: 'Brawl' },
    { label: 'Oathbreaker', value: 'Oathbreaker' }
  ];

  // ── Color identity ────────────────────────────────────
  readonly manaColorOrder: readonly ManaColorValue[] = ['W', 'U', 'B', 'R', 'G', 'C'];
  readonly manaColorOptions: readonly ManaColorOption[] = [
    { value: 'W', labelKey: 'workspace.createDeck.color.white', background: '#f8fafc', border: '#cbd5e1', color: '#111827' },
    { value: 'U', labelKey: 'workspace.createDeck.color.blue', background: '#60a5fa', border: '#2563eb', color: '#0b1220' },
    { value: 'B', labelKey: 'workspace.createDeck.color.black', background: '#1f2937', border: '#111827', color: '#f9fafb' },
    { value: 'R', labelKey: 'workspace.createDeck.color.red', background: '#f87171', border: '#dc2626', color: '#111827' },
    { value: 'G', labelKey: 'workspace.createDeck.color.green', background: '#4ade80', border: '#16a34a', color: '#0b1220' },
    { value: 'C', labelKey: 'workspace.createDeck.color.colorless', background: '#9ca3af', border: '#6b7280', color: '#111827' }
  ];
  readonly selectedColors = signal<readonly ManaColorValue[]>([]);

  isManaColorSelected(value: ManaColorValue): boolean {
    return this.selectedColors().includes(value);
  }

  toggleManaColor(value: ManaColorValue): void {
    const selected = new Set(this.selectedColors());
    if (selected.has(value)) {
      selected.delete(value);
    } else {
      selected.add(value);
    }
    this.selectedColors.set(this.manaColorOrder.filter((c) => selected.has(c)));
  }

  // ── Commander search ──────────────────────────────────
  readonly commanderSuggestions = signal<CommanderSuggestion[]>([]);
  readonly commanderSearching = signal(false);
  readonly selectedCommander = signal<CommanderSuggestion | null>(null);
  readonly commanderInputValue = signal<CommanderSuggestion | string | null>(null);
  private readonly commanderSearch$ = new Subject<string>();

  readonly needsCommander = computed(() =>
    COMMANDER_FORMATS.includes(this.deckForm.controls.format.value as typeof COMMANDER_FORMATS[number])
  );

  constructor() {
    this.commanderSearch$.pipe(
      debounceTime(320),
      distinctUntilChanged(),
      switchMap((query) => {
        const q = query.trim();
        if (q.length < 2) {
          this.commanderSuggestions.set([]);
          this.commanderSearching.set(false);
          return EMPTY;
        }
        this.commanderSearching.set(true);
        const scryfallQuery = encodeURIComponent(`${q} type:legendary type:creature`);
        return this.http
          .get<ScryfallSearchResponse>(
            `https://api.scryfall.com/cards/search?q=${scryfallQuery}&unique=names&order=name`
          )
          .pipe(
            catchError(() => {
              this.commanderSuggestions.set([]);
              this.commanderSearching.set(false);
              return EMPTY;
            })
          );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((response) => {
      this.commanderSearching.set(false);
      if (response.object === 'error' || !response.data?.length) {
        this.commanderSuggestions.set([]);
        return;
      }
      this.commanderSuggestions.set(
        response.data.slice(0, 8).map((c) => ({
          name: c.name,
          typeLine: c.type_line,
          colorIdentity: c.color_identity
        }))
      );
    });
  }

  searchCommander(event: { query: string }): void {
    this.commanderSearch$.next(event.query);
  }

  onCommanderValueChange(value: CommanderSuggestion | string | null): void {
    if (value && typeof value === 'object') {
      this.selectedCommander.set(value);
      this.commanderInputValue.set(null);
      this.commanderSuggestions.set([]);
      const ordered = this.manaColorOrder.filter((c) => value.colorIdentity.includes(c));
      this.selectedColors.set(ordered);
    } else {
      this.commanderInputValue.set(value);
    }
  }

  clearCommander(): void {
    this.selectedCommander.set(null);
    this.commanderInputValue.set(null);
    this.commanderSuggestions.set([]);
  }

  // ── Live preview computed values ──────────────────────
  readonly previewName = computed(() => {
    const name = this.deckForm.controls.name.value.trim();
    return name || null;
  });

  readonly previewFormat = computed(() => {
    const fmt = this.deckForm.controls.format.value;
    if (!fmt) return null;
    return this.formatOptions.find((f) => f.value === fmt)?.label ?? fmt;
  });

  // ── Form validity as a signal (statusChanges is an Observable, not a signal,
  //    so we bridge it with toSignal to keep computed() reactive)
  private readonly formStatus = toSignal(this.deckForm.statusChanges, {
    initialValue: this.deckForm.status
  });

  // ── Submit / Cancel ───────────────────────────────────
  readonly isSubmitDisabled = computed(() => this.formStatus() !== 'VALID' || this.submitting());

  handleSubmit(): void {
    this.deckForm.markAllAsTouched();
    if (this.deckForm.invalid || this.submitting()) return;

    const name = this.deckForm.controls.name.value.trim();
    const format = this.deckForm.controls.format.value;
    const noteValue = this.deckForm.controls.note.value.trim();
    const commander = this.selectedCommander();

    if (!name) return;

    this.hasError.set(false);
    this.submitting.set(true);

    this.workspaceService
      .createDeck({
        name,
        type: format ?? 'Custom',
        commander: commander?.name ?? null,
        colorIdentity: this.selectedColors().join('') || null,
        note: noteValue || null,
        querySearches: [],
        cards: []
      })
      .pipe(
        catchError(() => {
          this.hasError.set(true);
          return EMPTY;
        }),
        finalize(() => this.submitting.set(false))
      )
      .subscribe(() => {
        this.resetForm();
        this.layoutStateService.closeCreateDeckForm();
      });
  }

  handleCancel(): void {
    this.resetForm();
    this.hasError.set(false);
    this.layoutStateService.closeCreateDeckForm();
  }

  resetForm(): void {
    this.deckForm.reset({ name: '', format: null, note: '' });
    this.selectedColors.set([]);
    this.selectedCommander.set(null);
    this.commanderInputValue.set(null);
    this.commanderSuggestions.set([]);
  }
}

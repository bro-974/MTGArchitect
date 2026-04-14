import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { InputTextModule } from 'primeng/inputtext';
import { debounceTime, distinctUntilChanged, map, startWith } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-card-explorer-search',
  imports: [InputTextModule, ReactiveFormsModule, TranslocoPipe],
  templateUrl: './card-explorer-search.html',
  styleUrl: './card-explorer-search.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardExplorerSearch {
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = input(false);
  readonly search = output<string>();
  readonly searchControl = new FormControl('', { nonNullable: true });

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        startWith(this.searchControl.value),
        map((value) => value.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((value) => {
        this.search.emit(value);
      });
  }
}
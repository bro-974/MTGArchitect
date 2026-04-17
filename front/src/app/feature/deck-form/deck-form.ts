import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal
} from '@angular/core';
import { FormControl,FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { catchError, EMPTY, finalize } from 'rxjs';
import { WorkspaceService } from '../workspace/workspace.service';
import { WorkspaceLayoutStateService } from '../workspace-layout/workspace-layout-state.service';

@Component({
  selector: 'app-deck-form',
  templateUrl: './deck-form.html',
  styleUrl: './deck-form.css',
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    InputTextModule,
    TextareaModule,
    ButtonModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeckForm {
  private readonly workspaceService = inject(WorkspaceService);
  private readonly layoutStateService = inject(WorkspaceLayoutStateService);

  readonly submitting = signal(false);
  readonly hasError = signal(false);

  readonly deckForm = new FormGroup({
    name:new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(120)]
    }),
    type: new FormControl('Commander', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(80)]}),
    note: new FormControl('',{
        nonNullable: true})
  });

  readonly isSubmitDisabled = computed(() => this.deckForm.invalid || this.submitting());

  handleSubmit(): void {
    this.deckForm.markAllAsTouched();
    if (this.deckForm.invalid || this.submitting()) {
      return;
    }

    const name = this.deckForm.controls.name.value.trim();
    const type = this.deckForm.controls.type.value.trim();
    const noteValue = this.deckForm.controls.note.value.trim();

    if (!name || !type) {
      return;
    }

    this.hasError.set(false);
    this.submitting.set(true);

    this.workspaceService
      .createDeck({
        name,
        type,
        note: noteValue ? noteValue : null,
        querySearches: [],
        cards: []
      })
      .pipe(
        catchError(() => {
          this.hasError.set(true);
          return EMPTY;
        }),
        finalize(() => {
          this.submitting.set(false);
        })
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
    this.deckForm.reset({
      name: '',
      type: 'Commander',
      note: ''
    });
  }
}

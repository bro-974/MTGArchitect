import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class WorkspaceLayoutStateService {
  readonly isCreateDeckVisible = signal(false);

  openCreateDeckForm(): void {
    this.isCreateDeckVisible.set(true);
  }

  closeCreateDeckForm(): void {
    this.isCreateDeckVisible.set(false);
  }
}

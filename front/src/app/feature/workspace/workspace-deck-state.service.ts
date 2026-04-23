import { Injectable, signal } from '@angular/core';
import { WorkspaceDeck } from './workspace.models';

@Injectable({ providedIn: 'root' })
export class WorkspaceDeckStateService {
  readonly selectedDeck = signal<WorkspaceDeck | null>(null);
}

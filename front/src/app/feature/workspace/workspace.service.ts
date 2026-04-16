import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { WorkspaceDeck, WorkspaceDeckUpsert } from './workspace.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  private readonly http = inject(HttpClient);

  getDecks(): Observable<WorkspaceDeck[]> {
    return this.http.get<WorkspaceDeck[]>(`${environment.apiUrl}/api/decks/all`);
  }

  updateDeck(deckId: string, request: WorkspaceDeckUpsert): Observable<WorkspaceDeck> {
    return this.http.put<WorkspaceDeck>(`${environment.apiUrl}/api/deck/${deckId}`, request);
  }
}

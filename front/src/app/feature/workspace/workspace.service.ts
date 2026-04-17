import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, Subject, tap } from 'rxjs';
import { WorkspaceDeck, WorkspaceDeckUpsert } from './workspace.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  private readonly http = inject(HttpClient);
  private readonly deckCreatedSubject = new Subject<WorkspaceDeck>();

  readonly deckCreated$ = this.deckCreatedSubject.asObservable();

  getDecks(): Observable<WorkspaceDeck[]> {
    return this.http.get<WorkspaceDeck[]>(`${environment.apiUrl}/api/decks/all`);
  }

  updateDeck(deckId: string, request: WorkspaceDeckUpsert): Observable<WorkspaceDeck> {
    return this.http.put<WorkspaceDeck>(`${environment.apiUrl}/api/deck/${deckId}`, request);
  }

  createDeck(request: WorkspaceDeckUpsert): Observable<WorkspaceDeck> {
    return this.http
      .post<WorkspaceDeck>(`${environment.apiUrl}/api/deck`, request)
      .pipe(tap((deck) => this.deckCreatedSubject.next(deck)));
  }
}

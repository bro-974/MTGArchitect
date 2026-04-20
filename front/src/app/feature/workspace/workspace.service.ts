import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, Subject, tap } from 'rxjs';
import { WorkspaceDeck, WorkspaceDeckUpsert, WorkspaceQuerySearch } from './workspace.models';
import { environment } from '../../../environments/environment';

interface QueryInfoUpsertRequest {
  readonly id: string | null;
  readonly queryJson: string;
  readonly searchEngine: string;
}

interface QueryInfoResponse {
  readonly id: string;
  readonly query: string;
  readonly searchEngine: string;
}

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

  addQuerySearch(deckId: string, queryJson: string, searchEngine: string): Observable<QueryInfoResponse> {
    const request: QueryInfoUpsertRequest = {
      id: null,
      queryJson,
      searchEngine
    };
    return this.http.post<QueryInfoResponse>(
      `${environment.apiUrl}/api/deck/${deckId}/query-search`,
      request
    );
  }

  removeQuerySearch(deckId: string, queryId: string): Observable<void> {
    return this.http.delete<void>(
      `${environment.apiUrl}/api/deck/${deckId}/query-search/${queryId}`
    );
  }
}

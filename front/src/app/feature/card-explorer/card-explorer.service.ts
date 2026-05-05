import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CardQuerySearch } from '../../core/models/card-query-search.model';
import { environment } from '../../../environments/environment';

export interface CardExplorerCard {
  id: string;
  name: string;
  manaCost: string;
  typeLine: string;
  setCode: string;
  imageUrl: string;
}

export interface CardExplorerSearchResult {
  cards: CardExplorerCard[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class CardExplorerService {
  private readonly http = inject(HttpClient);

  searchCards(query: string, pageSize = 18): Observable<CardExplorerCard[]> {
    const params = new HttpParams()
      .set('q', query)
      .set('pageSize', pageSize);

    return this.http.get<CardExplorerCard[]>(`${environment.apiUrl}/api/cards/search`, { params });
  }

  searchCardsAdvanced(query: CardQuerySearch, page = 0): Observable<CardExplorerSearchResult> {
    query.pageSize = 30;
    query.page = page;
    return this.http.post<CardExplorerSearchResult>(`${environment.apiUrl}/api/cards/search/advanced`, query);
  }
}
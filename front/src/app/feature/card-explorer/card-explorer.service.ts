import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CardQuerySearch } from '../../core/models/card-query-search.model';

export interface CardExplorerCard {
  id: string;
  name: string;
  manaCost: string;
  typeLine: string;
  setCode: string;
  imageUrl: string;
}

@Injectable({ providedIn: 'root' })
export class CardExplorerService {
  private readonly http = inject(HttpClient);

  searchCards(query: string, pageSize = 18): Observable<CardExplorerCard[]> {
    const params = new HttpParams()
      .set('q', query)
      .set('pageSize', pageSize);

    return this.http.get<CardExplorerCard[]>('/api/cards/search', { params });
  }

  searchCardsAdvanced(query: CardQuerySearch, pageSize = 18): Observable<CardExplorerCard[]> {
    query.pageSize = pageSize;
    return this.http.post<CardExplorerCard[]>(`/api/cards/search/advanced`, query);
  }
}
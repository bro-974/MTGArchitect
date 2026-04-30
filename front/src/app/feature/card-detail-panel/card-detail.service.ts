import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface CardDetailPrinting {
  id: string;
  setCode: string;
  setName: string;
  rarity: string;
  imageUrl: string;
}

export interface CardDetailRuling {
  publishedAt: string;
  comment: string;
}

export interface CardDetail {
  id: string;
  name: string;
  manaCost: string;
  cmc: number;
  typeLine: string;
  oracleText: string;
  power: string;
  toughness: string;
  loyalty: string;
  rarity: string;
  setCode: string;
  setName: string;
  legalities: Record<string, string>;
  flavorText: string;
  artist: string;
  imageUrl: string;
  imageLargeUrl: string;
  printings: CardDetailPrinting[];
  rulings: CardDetailRuling[];
}

@Injectable({ providedIn: 'root' })
export class CardDetailService {
  private readonly http = inject(HttpClient);
  private readonly cache = new Map<string, CardDetail>();

  getCardDetail(id: string): Observable<CardDetail> {
    const cached = this.cache.get(id);
    if (cached) return of(cached);

    return this.http
      .get<CardDetail>(`${environment.apiUrl}/api/cards/${id}`)
      .pipe(tap((detail) => this.cache.set(id, detail)));
  }
}

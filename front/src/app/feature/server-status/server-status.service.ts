import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface ServerStatusResponse {
  status: string;
  checkedAt: string;
}

@Injectable({ providedIn: 'root' })
export class ServerStatusService {
  private readonly http = inject(HttpClient);

  getServerStatus(): Observable<ServerStatusResponse> {
    return this.http.get<ServerStatusResponse>('/api/server-status');
  }
}

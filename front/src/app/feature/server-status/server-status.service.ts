import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ServerStatusResponse {
  status: string;
  checkedAt: string;
  services: Record<string, string>;
}

@Injectable({ providedIn: 'root' })
export class ServerStatusService {
  private readonly http = inject(HttpClient);

  getServerStatus(): Observable<ServerStatusResponse> {
    return this.http.get<ServerStatusResponse>(`${environment.apiUrl}/api/server-status`);
  }
}

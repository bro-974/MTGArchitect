import { DatePipe, KeyValuePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { ServerStatusResponse, ServerStatusService } from './server-status.service';

type StatusState = 'idle' | 'loading' | 'online' | 'error';

@Component({
  selector: 'app-server-status',
  imports: [ButtonModule, TranslocoPipe, DatePipe, KeyValuePipe],
  templateUrl: './server-status.html',
  styleUrl: './server-status.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerStatus {
  private readonly serverStatusService = inject(ServerStatusService);

  readonly state = signal<StatusState>('idle');
  readonly response = signal<ServerStatusResponse | null>(null);

  constructor() {
    this.refreshStatus();
  }

    refreshStatus(): void {
    this.state.set('loading');

    this.serverStatusService.getServerStatus().subscribe({
        next: (result) => {
        this.response.set(result);
        this.state.set('online');
        },
        error: () => {
        this.response.set(null);
        this.state.set('error');
        }
    });
}
}

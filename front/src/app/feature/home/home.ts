import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'app-home',
  imports: [TranslocoPipe],
  template: `
    <section class="surface-card border-round p-4">
      <h1 class="m-0">{{ 'app.title' | transloco }}</h1>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Home {}

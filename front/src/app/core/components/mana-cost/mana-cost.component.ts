import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

interface ManaToken {
  label: string;
  background: string;
  border: string;
  color: string;
}

const MANA_PALETTE: Record<string, Omit<ManaToken, 'label'>> = {
  W: { background: '#f8fafc', border: '#cbd5e1', color: '#111827' },
  U: { background: '#60a5fa', border: '#2563eb', color: '#0b1220' },
  B: { background: '#1f2937', border: '#111827', color: '#f9fafb' },
  R: { background: '#f87171', border: '#dc2626', color: '#111827' },
  G: { background: '#4ade80', border: '#16a34a', color: '#0b1220' },
};

const COLORLESS = { background: '#9ca3af', border: '#6b7280', color: '#111827' };

@Component({
  selector: 'app-mana-cost',
  templateUrl: './mana-cost.component.html',
  styleUrl: './mana-cost.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ManaCostComponent {
  readonly cost = input<string | null>(null);
  readonly small = input(false);

  readonly tokens = computed<ManaToken[]>(() => {
    const c = this.cost();
    if (!c) return [];
    return (c.match(/\{[^}]+\}/g) ?? []).map((token) => {
      const symbol = token.slice(1, -1).toUpperCase();
      const palette = MANA_PALETTE[symbol] ?? COLORLESS;
      return { label: token.slice(1, -1), ...palette };
    });
  });
}

import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

const SCRYFALL_SVG_BASE = 'https://svgs.scryfall.io/card-symbols';

interface ManaToken {
  label: string;
  svgUrl: string;
}

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
      const filename = token.slice(1, -1).replace('/', '').toUpperCase();
      return { label: token.slice(1, -1), svgUrl: `${SCRYFALL_SVG_BASE}/${filename}.svg` };
    });
  });
}

import { Directive, ElementRef, HostListener, input, OnDestroy, Renderer2 } from '@angular/core';

const PREVIEW_WIDTH = 320;
const PREVIEW_HEIGHT = 446;
const OFFSET_X = 16;
const OFFSET_Y = 8;
const EDGE_RIGHT = 350;
const EDGE_BOTTOM = 460;
const HOVER_DELAY_MS = 150;

@Directive({
  selector: '[cardHoverPreview]',
  standalone: true
})
export class CardHoverPreviewDirective implements OnDestroy {
  readonly cardHoverPreview = input.required<string>();

  private img: HTMLImageElement | null = null;
  private timer: ReturnType<typeof setTimeout> | null = null;

  constructor(private readonly renderer: Renderer2) {}

  @HostListener('mouseenter', ['$event'])
  onMouseEnter(event: MouseEvent): void {
    this.timer = setTimeout(() => this.show(event), HOVER_DELAY_MS);
  }

  @HostListener('mousemove', ['$event'])
  onMouseMove(event: MouseEvent): void {
    if (this.img) {
      this.position(event);
    }
  }

  @HostListener('mouseleave')
  onMouseLeave(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
    this.hide();
  }

  ngOnDestroy(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
    }
    this.hide();
  }

  private show(event: MouseEvent): void {
    const scryFallId = this.cardHoverPreview();
    if (!scryFallId) return;

    const url = `https://cards.scryfall.io/normal/front/${scryFallId[0]}/${scryFallId[1]}/${scryFallId}.jpg`;

    this.img = this.renderer.createElement('img') as HTMLImageElement;
    this.renderer.setAttribute(this.img, 'src', url);
    this.renderer.setStyle(this.img, 'position', 'fixed');
    this.renderer.setStyle(this.img, 'width', `${PREVIEW_WIDTH}px`);
    this.renderer.setStyle(this.img, 'height', `${PREVIEW_HEIGHT}px`);
    this.renderer.setStyle(this.img, 'object-fit', 'cover');
    this.renderer.setStyle(this.img, 'border-radius', '10px');
    this.renderer.setStyle(this.img, 'box-shadow', '0 8px 32px rgba(0,0,0,0.45)');
    this.renderer.setStyle(this.img, 'pointer-events', 'none');
    this.renderer.setStyle(this.img, 'z-index', '9999');
    this.renderer.setStyle(this.img, 'transition', 'opacity 0.1s');
    this.renderer.appendChild(document.body, this.img);

    this.position(event);
  }

  private position(event: MouseEvent): void {
    if (!this.img) return;

    const flipX = event.clientX + OFFSET_X + PREVIEW_WIDTH > window.innerWidth - EDGE_RIGHT + PREVIEW_WIDTH;
    const flipY = event.clientY - OFFSET_Y + PREVIEW_HEIGHT > window.innerHeight - EDGE_BOTTOM + PREVIEW_HEIGHT;

    const x = flipX
      ? event.clientX - OFFSET_X - PREVIEW_WIDTH
      : event.clientX + OFFSET_X;

    const y = flipY
      ? event.clientY - OFFSET_Y - PREVIEW_HEIGHT
      : event.clientY - OFFSET_Y;

    this.renderer.setStyle(this.img, 'left', `${x}px`);
    this.renderer.setStyle(this.img, 'top', `${y}px`);
  }

  private hide(): void {
    if (this.img) {
      this.renderer.removeChild(document.body, this.img);
      this.img = null;
    }
  }
}

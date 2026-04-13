import { effect, Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly DARK_MODE_KEY = 'darkMode';

  readonly isDarkMode = signal(localStorage.getItem(this.DARK_MODE_KEY) === 'true');

  constructor() {
    effect(() => {
      document.documentElement.classList.toggle('app-dark', this.isDarkMode());
      localStorage.setItem(this.DARK_MODE_KEY, this.isDarkMode().toString());
    });
  }

  toggleTheme(): void {
    this.isDarkMode.update(current => !current);
  }
}

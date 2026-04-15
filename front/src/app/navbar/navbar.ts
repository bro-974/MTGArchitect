import { Component, effect, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';
import { SelectModule } from 'primeng/select';
import { TranslocoPipe } from '@jsverse/transloco';
import { AppLanguage, I18nService } from '../core/i18n/i18n.service';
import { ThemeService } from '../core/theme/theme.service';

@Component({
  selector: 'app-navbar',
  imports: [ButtonModule, CommonModule, SelectModule, ReactiveFormsModule, TranslocoPipe],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  readonly i18nService = inject(I18nService);
  readonly themeService = inject(ThemeService);
  readonly router = inject(Router);
  readonly languageControl = new FormControl<AppLanguage>(this.i18nService.activeLanguage(), {
    nonNullable: true
  });

  isDarkMode = this.themeService.isDarkMode;

  constructor() {
    this.languageControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((language) => {
        this.i18nService.setLanguage(language);
      });

    effect(() => {
      const activeLanguage = this.i18nService.activeLanguage();
      if (this.languageControl.value !== activeLanguage) {
        this.languageControl.setValue(activeLanguage, { emitEvent: false });
      }
    });
  }

  goHome(): void {
    this.router.navigate(['/']);
  }

  goServerStatus(): void {
    this.router.navigate(['/feature/server-status']);
  }

  goCardExplorer(): void {
    this.router.navigate(['/feature/card-explorer']);
  }

  goCardExplorerAdvanced(): void {
    this.router.navigate(['/feature/card-explorer-search-advanced']);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}

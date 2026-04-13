import { computed, effect, inject, Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { TranslocoService } from '@jsverse/transloco';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export type AppLanguage = 'en' | 'fr';
export interface LanguageOption {
  labelKey: string;
  value: AppLanguage;
}

@Injectable({ providedIn: 'root' })
export class I18nService {
  private static readonly STORAGE_KEY = 'appLanguage';
  private static readonly SUPPORTED_LANGUAGES: AppLanguage[] = ['en', 'fr'];
  private static readonly DEFAULT_LANGUAGE: AppLanguage = 'en';

  private readonly transloco = inject(TranslocoService);
  private readonly title = inject(Title);

  readonly supportedLanguages = I18nService.SUPPORTED_LANGUAGES;
  readonly activeLanguage = computed(() => this.transloco.activeLang() as AppLanguage);
  readonly languageOptions = computed<LanguageOption[]>(() => {
    return this.supportedLanguages.map((language) => ({
      labelKey: `navbar.languages.${language}`,
      value: language
    }));
  });

  constructor() {
    this.transloco.setAvailableLangs(I18nService.SUPPORTED_LANGUAGES);
    this.transloco.setDefaultLang(I18nService.DEFAULT_LANGUAGE);
    this.setLanguage(this.getInitialLanguage());

    effect(() => {
      const language = this.activeLanguage();
      document.documentElement.lang = language;
      localStorage.setItem(I18nService.STORAGE_KEY, language);
    });

    this.transloco.selectTranslate('app.title')
      .pipe(takeUntilDestroyed())
      .subscribe((translatedTitle) => {
        this.title.setTitle(translatedTitle);
      });
  }

  setLanguage(language: AppLanguage): void {
    if (!I18nService.SUPPORTED_LANGUAGES.includes(language)) {
      return;
    }

    this.transloco.setActiveLang(language);
  }

  private getInitialLanguage(): AppLanguage {
    const persistedLanguage = localStorage.getItem(I18nService.STORAGE_KEY);
    if (this.isSupportedLanguage(persistedLanguage)) {
      return persistedLanguage;
    }

    const browserLanguage = navigator.language?.split('-')[0] ?? I18nService.DEFAULT_LANGUAGE;
    if (this.isSupportedLanguage(browserLanguage)) {
      return browserLanguage;
    }

    return I18nService.DEFAULT_LANGUAGE;
  }

  private isSupportedLanguage(value: string | null): value is AppLanguage {
    return value === 'en' || value === 'fr';
  }
}

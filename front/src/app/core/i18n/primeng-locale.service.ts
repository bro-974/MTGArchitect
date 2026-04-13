import { effect, inject, Injectable } from '@angular/core';
import type { Translation } from 'primeng/api';
import { PrimeNG } from 'primeng/config';
import { AppLanguage, I18nService } from './i18n.service';

const PRIME_NG_TRANSLATIONS: Record<AppLanguage, Translation> = {
  en: {
    accept: 'Yes',
    reject: 'No',
    choose: 'Choose',
    upload: 'Upload',
    cancel: 'Cancel',
    clear: 'Clear',
    today: 'Today',
    weekHeader: 'Wk',
    firstDayOfWeek: 1,
    dateFormat: 'mm/dd/yy',
    weak: 'Weak',
    medium: 'Medium',
    strong: 'Strong',
    passwordPrompt: 'Enter a password',
    emptyMessage: 'No available options',
    emptyFilterMessage: 'No results found'
  },
  fr: {
    accept: 'Oui',
    reject: 'Non',
    choose: 'Choisir',
    upload: 'Televerser',
    cancel: 'Annuler',
    clear: 'Effacer',
    today: "Aujourd'hui",
    weekHeader: 'Sem',
    firstDayOfWeek: 1,
    dateFormat: 'dd/mm/yy',
    weak: 'Faible',
    medium: 'Moyen',
    strong: 'Fort',
    passwordPrompt: 'Saisissez un mot de passe',
    emptyMessage: 'Aucune option disponible',
    emptyFilterMessage: 'Aucun resultat'
  }
};

@Injectable({ providedIn: 'root' })
export class PrimeNgLocaleService {
  private readonly i18nService = inject(I18nService);
  private readonly primeNg = inject(PrimeNG);

  constructor() {
    effect(() => {
      const language = this.i18nService.activeLanguage();
      this.primeNg.setTranslation(PRIME_NG_TRANSLATIONS[language]);
    });
  }
}

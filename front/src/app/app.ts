import { Component, inject } from '@angular/core';
import { Navbar } from "./navbar/navbar";
import { RouterOutlet } from "@angular/router";
import { ToastModule } from 'primeng/toast';
import { I18nService } from './core/i18n/i18n.service';
import { PrimeNgLocaleService } from './core/i18n/primeng-locale.service';

@Component({
  selector: 'app-root',
  imports: [Navbar, RouterOutlet, ToastModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly i18n = inject(I18nService);
  readonly primeLocale = inject(PrimeNgLocaleService);
}

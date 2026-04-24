# Frontend Conventions (Angular / TypeScript)

Source of truth: `.github/instructions/angular.instructions.md`.
Applies to `front/**/*.{ts,html,css,scss}`.

Stack: **Angular 21**, **PrimeNG 21** + PrimeFlex + `@primeuix/themes` (Aura preset),
**Transloco** for i18n, Vitest + jsdom for tests, Prettier (print width 100, single quotes).

---

## TypeScript

- Strict type checking.
- Prefer type inference when the type is obvious.
- Avoid `any`; use `unknown` when type is uncertain.

## Angular

- Standalone components only — **do not** set `standalone: true` (it's the default in v20+).
- Use **signals** for state; `computed()` for derived state; `update`/`set` (never `mutate`). Prefer signals over RxJS for local component state; reach for observables when the source is already async (HTTP, events).
- Lazy-load feature routes.
- Do **not** use `@HostBinding` / `@HostListener` — use the `host` object in the decorator.
- Use `NgOptimizedImage` for static images (not for base64).
- `changeDetection: ChangeDetectionStrategy.OnPush` on every `@Component`.
- `input()` / `output()` functions — not decorators.
- Reactive forms over template-driven.
- Use `class` and `style` bindings — not `ngClass` / `ngStyle`.
- Use native control flow: `@if`, `@for`, `@switch` (not `*ngIf`, `*ngFor`, `*ngSwitch`).
- Use the async pipe for observables.
- Don't assume globals (e.g. `new Date()`) — inject.

## Services

- Single responsibility.
- `providedIn: 'root'` for singletons.
- Use the `inject()` function — not constructor injection.

## Environment & API URLs

- **Never** hardcode API base URLs.
- All API roots come from `src/environments/environment.ts` (dev) and `environment.prod.ts` (prod).
- `angular.json` production config must include a `fileReplacements` entry for these.
- Example:
  ```typescript
  import { environment } from '../../../environments/environment';
  this.http.get(`${environment.authApiUrl}/api/auth/me`);
  ```

## PrimeNG theme

- PrimeNG is the default visual system; configure via `providePrimeNG` in `src/app/app.config.ts`.
- Default preset: **Aura**.
- Dark mode via CSS class `.app-dark` on `document.documentElement`.
- Persist theme in `localStorage`, restore at startup.
- Prefer PrimeNG components (`p-button`, `p-select`, …) and PrimeFlex utilities over custom layout.
- Ask the user before deep-customizing PrimeNG visuals/behavior.

## i18n (Transloco)

- `@jsverse/transloco` runtime i18n (not Angular compile-time).
- Supported languages: `en`, `fr`.
- Translation files: `public/i18n/{lang}.json`.
- Config in `src/app/app.config.ts`:
  - `availableLangs: ['en', 'fr']`
  - `defaultLang: 'en'`
  - `fallbackLang: 'en'`
  - `reRenderOnLangChange: true`
- Use stable, namespaced keys: `app.title`, `navbar.home`, `navbar.language`.
- Never hardcode user-facing strings in templates.
- Persist chosen language in `localStorage`, update `document.documentElement.lang`.
- Keep PrimeNG locale in sync with Transloco through a dedicated service.

## Accessibility

- Must pass all AXE checks.
- Must meet WCAG AA minimums: focus management, color contrast, ARIA attributes.

## Frontend layout

```
front/src/app/
├── app.config.ts / app.ts / app.routes.ts / app.html / app.css
├── core/
│   ├── auth/          authentication service, guards, interceptors
│   ├── i18n/          Transloco wiring, locale sync with PrimeNG
│   ├── models/        shared TypeScript contracts
│   └── theme/         PrimeNG theme + dark-mode management
├── feature/
│   ├── card-explorer / card-explorer-list / card-explorer-search / card-explorer-search-advanced
│   ├── deck-form
│   ├── home
│   ├── login
│   ├── search-show
│   ├── server-status
│   ├── workspace / workspace-deck-selected / workspace-layout
└── navbar/
```



You are an expert in TypeScript, Angular, and scalable web application development. You write functional, maintainable, performant, and accessible code following Angular and TypeScript best practices.

## TypeScript Best Practices

- Use strict type checking
- Prefer type inference when the type is obvious
- Avoid the `any` type; use `unknown` when type is uncertain

## Angular Best Practices

- Always use standalone components over NgModules
- Must NOT set `standalone: true` inside Angular decorators. It's the default in Angular v20+.
- Use signals for state management
- Implement lazy loading for feature routes
- Do NOT use the `@HostBinding` and `@HostListener` decorators. Put host bindings inside the `host` object of the `@Component` or `@Directive` decorator instead
- Use `NgOptimizedImage` for all static images.
  - `NgOptimizedImage` does not work for inline base64 images.

## Accessibility Requirements

- It MUST pass all AXE checks.
- It MUST follow all WCAG AA minimums, including focus management, color contrast, and ARIA attributes.

### Components

- Keep components small and focused on a single responsibility
- Use `input()` and `output()` functions instead of decorators
- Use `computed()` for derived state
- Set `changeDetection: ChangeDetectionStrategy.OnPush` in `@Component` decorator
- Prefer inline templates for small components
- Prefer Reactive forms instead of Template-driven ones
- Do NOT use `ngClass`, use `class` bindings instead
- Do NOT use `ngStyle`, use `style` bindings instead
- When using external templates/styles, use paths relative to the component TS file.

## State Management

- Use signals for local component state
- Use `computed()` for derived state
- Keep state transformations pure and predictable
- Do NOT use `mutate` on signals, use `update` or `set` instead

## Templates

- Keep templates simple and avoid complex logic
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- Use the async pipe to handle observables
- Do not assume globals like (`new Date()`) are available.

## Services

- Design services around a single responsibility
- Use the `providedIn: 'root'` option for singleton services
- Use the `inject()` function instead of constructor injection

## PrimeNG Theme Rules

- Use PrimeNG theme configuration in `src/app/app.config.ts` with `providePrimeNG`.
- Keep Aura as the default preset unless explicitly requested otherwise.
- Use CSS class based dark mode with `.app-dark` on `document.documentElement`.
- Persist theme choice in `localStorage` and restore it at startup.
- For global utility classes, use PrimeFlex (`primeflex/primeflex.css`) instead of custom layout helpers when possible.
- Prefer PrimeNG components (`p-button`, `p-select`, etc.) for UI consistency.

## Transloco i18n Rules

- Use Transloco runtime i18n with `@jsverse/transloco` (not Angular compile-time i18n for dynamic switching).
- Keep supported languages to `en` and `fr` unless scope changes.
- Store translation files in `public/i18n/{lang}.json`.
- Configure Transloco in `src/app/app.config.ts` with:
  - `availableLangs: ['en', 'fr']`
  - `defaultLang: 'en'`
  - `fallbackLang: 'en'`
  - `reRenderOnLangChange: true`
- Use stable, namespaced keys (for example: `app.title`, `navbar.home`, `navbar.language`).
- Do not hardcode user-facing strings in templates; use Transloco pipe/directive.
- Persist selected language in `localStorage` and update `document.documentElement.lang` on change.
- Keep PrimeNG locale synchronized with current Transloco language via a dedicated service.

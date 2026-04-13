---
name: i18n-missing-keys
description: 'Audit translation files for missing i18n keys by using public/i18n/en.json as reference and checking other locales like fr.json. Use when adding new UI text, reviewing localization completeness, or validating translation parity.'
argument-hint: 'Optional: target folder (default public/i18n) and reference locale (default en.json)'
user-invocable: true
---

# I18n Missing Keys Audit

## What This Skill Produces
- A parity report that compares all locale files in an i18n folder against `en.json`.
- A list of missing keys per locale.
- Optional notes for extra keys and type mismatches.
- An optional guided fix flow that asks the user whether to add missing keys.
- If approved, missing keys are inserted with proper translated values in the target language.
- A completion verdict: pass or fail.

## When to Use
- After adding or renaming translation keys in the reference language.
- Before PR review to ensure localization completeness.
- During regression checks when translated UI strings appear as raw keys.

## Inputs
- I18n folder path. Default: `public/i18n`.
- Reference locale file. Default: `en.json`.
- Target locales. Default: all `*.json` files except the reference file.

## Procedure
1. Resolve scope and files.
- Use the provided i18n folder or default to `public/i18n`.
- Confirm `en.json` exists and is valid JSON.
- Enumerate all locale JSON files except `en.json` (for example `fr.json`).

2. Build canonical key map from the reference file.
- Flatten nested objects into dot-path keys, for example `navbar.languages.en`.
- Record expected leaf value types (`string`, `number`, `boolean`, `null`, `object`, `array`).
- Treat each leaf key as required unless explicitly excluded.

3. Compare each target locale against the reference.
- Compute missing keys: keys in reference but absent in target locale.
- Compute extra keys: keys in target locale but absent in reference.
- Compute type mismatches: key exists in both but leaf types differ.

4. Apply decision logic.
- If missing keys exist: mark locale as failed and list all missing paths.
- If no missing keys but type mismatches exist: mark locale as warning/fail based on team policy.
- If only extra keys exist: mark locale as warning and recommend cleanup review.
- If none of the above: mark locale as pass.

5. Ask the user whether to auto-add missing keys.
- If at least one locale has missing keys, ask: "Do you want me to add the missing keys now?"
- If the user declines, stop after reporting findings.
- If the user accepts, continue with the fix flow below.

6. Add missing keys with correct translations.
- For each missing key, read the source value from `en.json`.
- Translate the source value to the target locale language (for example English to French for `fr.json`).
- Write the translated value at the same dot-path in the target locale file.
- Preserve existing JSON style and key hierarchy.
- If translation is ambiguous, ask a short clarification question before writing the value.

7. Re-run validation and produce final report.
- Summarize per locale: missing count, extra count, mismatch count.
- Include deterministic key order (alphabetical) for reproducible diffs.
- Confirm which keys were added and which translated values were written.
- Recommend concrete next action per failed locale.

## Completion Criteria
- Reference file parsed successfully.
- Every non-reference locale file was checked.
- Missing keys are reported as full dot-paths.
- If user approved auto-fix, missing keys were added to target locales with translated values.
- If user approved auto-fix, a second audit pass confirms the updated status.
- Final report clearly states pass/fail per locale.

## Output Format
- Reference: `<folder>/en.json`
- Checked locales: comma-separated file names
- For each locale:
  - Missing keys (`[]` if none)
  - Extra keys (`[]` if none)
  - Type mismatches (`[]` if none)
  - Keys added (`[]` if none or user declined)
  - Status: `PASS`, `WARN`, or `FAIL`

## User Interaction Rules
- Always ask before modifying translation files.
- Never overwrite existing translated values unless the user explicitly asks.
- When adding values, prioritize accurate natural translation in the target language, not English copy placeholders.

## Notes
- For this repository, use `public/i18n/en.json` as source of truth and compare against available alternatives such as `public/i18n/fr.json`.
- Keep key names stable and namespaced (for example `app.title`, `navbar.home`).
